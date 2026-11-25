param(
    [Parameter(Mandatory=$true)][guid]$StudentId,
    [string]$BaseUrl = "https://localhost:7032"
)

$ErrorActionPreference = 'Stop'
$token = $env:PAQUITOBOT_TEST_JWT
if ([string]::IsNullOrWhiteSpace($token)) {
    throw 'Set PAQUITOBOT_TEST_JWT to a valid JWT in this PowerShell session. Do not save it in a file.'
}

# Read-only checks. No inserts or updates.
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
$base = $BaseUrl.TrimEnd('/')
$statement = "$base/api/student-finances/$StudentId/statement"
$payments = "$base/api/student-finances/$StudentId/payments"
$headers = @{ Authorization = "Bearer $token" }

function Check-Status {
    param([string]$Label,[string]$Uri,[int]$Expected,[hashtable]$RequestHeaders)
    try {
        $answer = Invoke-WebRequest -Uri $Uri -Method Get -Headers $RequestHeaders -UseBasicParsing
        $actual = [int]$answer.StatusCode
    } catch [System.Net.WebException] {
        if ($_.Exception.Response -eq $null) { throw }
        $actual = [int]$_.Exception.Response.StatusCode
    }
    if ($actual -ne $Expected) { throw "${Label}: expected HTTP $Expected, received HTTP $actual" }
    Write-Host "OK HTTP $Expected - $Label" -ForegroundColor Green
}

Check-Status 'Statement requires JWT' $statement 401 @{}
Check-Status 'Authorized student statement' $statement 200 $headers
Check-Status 'Authorized payment history' $payments 200 $headers
Check-Status 'Date range validation' "${payments}?from=2026-12-31&to=2026-01-01" 400 $headers
Check-Status 'Unknown student returns 404' "$base/api/student-finances/11111111-1111-4111-8111-111111111111/statement" 404 $headers
Write-Host 'FINANCIAL READ SMOKE TEST COMPLETE (5/5)' -ForegroundColor Cyan
