[CmdletBinding()]
param(
    [ValidateSet('start', 'stop', 'status', 'logs', 'reset')]
    [string]$Action = 'status'
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$envFile = Join-Path $here '.env'
$composeFile = Join-Path $here 'compose.yaml'

if (-not (Test-Path -LiteralPath $envFile)) {
    throw 'The environment has not been configured. Run setup.ps1 first.'
}

$arguments = @('compose', '--env-file', $envFile, '--file', $composeFile)

switch ($Action) {
    'start'  { docker @arguments up --detach }
    'stop'   { docker @arguments down }
    'status' { docker @arguments ps }
    'logs'   { docker @arguments logs --follow --tail 100 }
    'reset' {
        $answer = Read-Host 'This deletes the local Frappe database and files. Type RESET to continue'
        if ($answer -ne 'RESET') {
            Write-Host 'Reset cancelled.'
            exit 0
        }

        docker @arguments down --volumes --remove-orphans
        $remainingSettings = Get-Content -LiteralPath $envFile |
            Where-Object { $_ -notmatch '^FRAPPE_API_(KEY|SECRET)=' }
        $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::WriteAllLines($envFile, $remainingSettings, $utf8NoBom)
        Write-Host 'Local Frappe data was deleted. Run setup.ps1 -SkipBuild to recreate it.'
    }
}

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
