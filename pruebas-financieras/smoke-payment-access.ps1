param(
    [string]$BaseUrl = "https://localhost:7032",
    [Parameter(Mandatory=$true)][guid]$StudentId
)
$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')

function HttpCode([string]$url, [string]$method = 'GET', [hashtable]$headers = @{}) {
    try {
        $result = Invoke-WebRequest -Uri $url -Method $method -Headers $headers -UseBasicParsing
        return [int]$result.StatusCode
    } catch [System.Net.WebException] {
        if ($_.Exception.Response) {
            return [int]$_.Exception.Response.StatusCode
        }
        throw
    }
}

function Expect([string]$name, [int]$expected, [int]$actual) {
    if ($actual -ne $expected) { throw "$name expected $expected, got $actual" }
    Write-Host "OK HTTP $expected - $name"
}

Expect 'Administrative payment list requires JWT' 401 (HttpCode "$BaseUrl/api/payments")
$jwt = $env:PAQUITOBOT_TEST_JWT
if ([string]::IsNullOrWhiteSpace($jwt)) { throw 'Set PAQUITOBOT_TEST_JWT to a student TEST token.' }
$h = @{ Authorization = "Bearer $jwt" }
Expect 'Student cannot read every payment' 403 (HttpCode "$BaseUrl/api/payments" 'GET' $h)
Expect 'Student can read own statement' 200 (HttpCode "$BaseUrl/api/student-finances/$StudentId/statement" 'GET' $h)
Write-Host 'FINANCIAL ACCESS SMOKE TEST COMPLETE (3/3)'
