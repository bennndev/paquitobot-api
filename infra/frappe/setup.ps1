[CmdletBinding()]
param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$envFile = Join-Path $here '.env'
$composeFile = Join-Path $here 'compose.yaml'
$upstreamDir = Join-Path $here '.frappe-docker'
$upstreamCommit = '0ad1a1152f93b1285104bcd0a8059e036dc5bd3d'

function New-LocalSecret {
    return ([Guid]::NewGuid().ToString('N') + [Guid]::NewGuid().ToString('N')).Substring(0, 32)
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Invoke-Checked {
    param([Parameter(Mandatory)][string]$FilePath, [Parameter(ValueFromRemainingArguments)][string[]]$Arguments)

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed: $FilePath $($Arguments -join ' ')"
    }
}

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker was not found. Install Docker Desktop with WSL 2 support first.'
}

$savedErrorPreference = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
docker info 2> $null | Out-Null
$dockerInfoExitCode = $LASTEXITCODE
$ErrorActionPreference = $savedErrorPreference

if ($dockerInfoExitCode -ne 0) {
    throw 'Docker Desktop is installed but its Linux engine is not running. Start Docker Desktop and run this script again.'
}

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    throw 'Git was not found.'
}

if (-not (Test-Path -LiteralPath $envFile)) {
    $dbPassword = New-LocalSecret
    $adminPassword = New-LocalSecret

    $environmentContent = @"
COMPOSE_PROJECT_NAME=paquitobot-frappe
CUSTOM_IMAGE=paquitobot/frappe-education
CUSTOM_TAG=15.126.0
SITE_NAME=education.localhost
FRAPPE_HTTP_PORT=8000
DB_ROOT_PASSWORD=$dbPassword
ADMIN_PASSWORD=$adminPassword
"@
    [System.IO.File]::WriteAllText($envFile, $environmentContent, $utf8NoBom)

    Write-Host 'Created infra/frappe/.env with local random passwords.' -ForegroundColor Green
}

if (-not $SkipBuild) {
    if (-not (Test-Path -LiteralPath (Join-Path $upstreamDir '.git'))) {
        Invoke-Checked git clone --filter=blob:none https://github.com/frappe/frappe_docker.git $upstreamDir
    }

    Invoke-Checked git -C $upstreamDir fetch origin main
    Invoke-Checked git -C $upstreamDir checkout --detach $upstreamCommit

    Invoke-Checked docker build `
        --build-arg FRAPPE_BRANCH=v15.126.0 `
        --build-arg FRAPPE_IMAGE_TAG=version-15 `
        --build-arg EDUCATION_COMMIT=61408d3da51833a4045b5506be61d47b3a0a8d70 `
        --secret "id=apps_json,src=$(Join-Path $here 'apps.json')" `
        --tag paquitobot/frappe-education:15.126.0 `
        --file (Join-Path $here 'Containerfile') `
        $upstreamDir
}

$composeArgs = @('compose', '--env-file', $envFile, '--file', $composeFile)

Invoke-Checked docker @composeArgs up create-site
Invoke-Checked docker @composeArgs up --detach

$settings = Get-Content -LiteralPath $envFile | ConvertFrom-StringData
$baseUrl = "http://localhost:$($settings.FRAPPE_HTTP_PORT)"

if (-not $settings.FRAPPE_API_KEY -or -not $settings.FRAPPE_API_SECRET) {
    Write-Host 'Generating local API credentials...' -ForegroundColor Cyan

    $ready = $false
    for ($attempt = 1; $attempt -le 30; $attempt++) {
        try {
            Invoke-WebRequest -Uri "$baseUrl/api/method/ping" -UseBasicParsing -TimeoutSec 5 | Out-Null
            $ready = $true
            break
        }
        catch {
            Start-Sleep -Seconds 2
        }
    }

    if (-not $ready) {
        throw "Frappe did not become available at $baseUrl. Run frappe.ps1 logs to inspect it."
    }

    $login = Invoke-WebRequest `
        -Uri "$baseUrl/api/method/login" `
        -Method Post `
        -Body @{ usr = 'Administrator'; pwd = $settings.ADMIN_PASSWORD } `
        -SessionVariable frappeSession `
        -UseBasicParsing

    $keys = Invoke-RestMethod `
        -Uri "$baseUrl/api/method/frappe.core.doctype.user.user.generate_keys" `
        -Method Post `
        -Body @{ user = 'Administrator' } `
        -WebSession $frappeSession

    $apiCredentialContent = @"

FRAPPE_API_KEY=$($keys.message.api_key)
FRAPPE_API_SECRET=$($keys.message.api_secret)
"@
    [System.IO.File]::AppendAllText($envFile, $apiCredentialContent, $utf8NoBom)

    $settings = Get-Content -LiteralPath $envFile | ConvertFrom-StringData
}

Write-Host ''
Write-Host 'Frappe Education is ready.' -ForegroundColor Green
Write-Host "URL: http://$($settings.SITE_NAME):$($settings.FRAPPE_HTTP_PORT)"
Write-Host 'User: Administrator'
Write-Host "Password: $($settings.ADMIN_PASSWORD)"
Write-Host ''
Write-Host 'Keep .env private. It is excluded from Git.' -ForegroundColor Yellow
Write-Host 'Run infra/frappe/run-api.ps1 to start PaquitoBot with these credentials.'
