$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $here)
$envFile = Join-Path $here '.env'

if (-not (Test-Path -LiteralPath $envFile)) {
    throw 'Run infra/frappe/setup.ps1 first.'
}

$settings = Get-Content -LiteralPath $envFile | ConvertFrom-StringData

foreach ($required in @('FRAPPE_HTTP_PORT', 'FRAPPE_API_KEY', 'FRAPPE_API_SECRET')) {
    if (-not $settings.ContainsKey($required)) {
        throw "Missing $required in infra/frappe/.env. Run setup.ps1 again."
    }
}

$env:Frappe__BaseUrl = "http://education.localhost:$($settings.FRAPPE_HTTP_PORT)"
$env:Frappe__ApiKey = $settings.FRAPPE_API_KEY
$env:Frappe__ApiSecret = $settings.FRAPPE_API_SECRET

dotnet run --project (Join-Path $repoRoot 'PaquitoBot.Api/PaquitoBot.Api.csproj') --launch-profile http
