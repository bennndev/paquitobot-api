param(
    [Parameter(Mandatory = $true)] [guid] $StudentId,
    [string] $BaseUrl = 'https://localhost:7032',
    [string] $AdminToken = $env:PAQUITOBOT_QA_ADMIN_JWT,
    [string] $StudentToken = $env:PAQUITOBOT_QA_STUDENT_JWT
)

$ErrorActionPreference = 'Stop'
$base = $BaseUrl.TrimEnd('/')
$endpoint = "${base}/api/student-finances/${StudentId}/reconcile"

function Get-Status([string]$url, [string]$method, [string]$token) {
    $headers = @{ Accept = 'application/json' }
    if ($token) { $headers.Authorization = "Bearer $token" }
    try {
        $response = Invoke-WebRequest -Uri $url -Method $method -Headers $headers -UseBasicParsing
        return [int]$response.StatusCode
    }
    catch [System.Net.WebException] {
        if ($_.Exception.Response) {
            return [int]$_.Exception.Response.StatusCode
        }
        throw
    }
}

$actual = Get-Status $endpoint 'POST' $null
if ($actual -ne 401) { throw "Anonymous reconciliation expected HTTP 401; received $actual" }
Write-Host 'OK HTTP 401 - Reconciliation requires JWT'

$actual = Get-Status "${base}/api/sync-runs" 'GET' $null
if ($actual -ne 401) { throw "Anonymous sync history expected HTTP 401; received $actual" }
Write-Host 'OK HTTP 401 - Sync runs require JWT'

if ($StudentToken) {
    $actual = Get-Status $endpoint 'POST' $StudentToken
    if ($actual -ne 403) { throw "Student reconciliation expected HTTP 403; received $actual" }
    Write-Host 'OK HTTP 403 - Students cannot trigger reconciliation'
}
else {
    Write-Host 'SKIP - Student JWT not supplied (PAQUITOBOT_QA_STUDENT_JWT)'
}

if ($AdminToken) {
    $headers = @{ Authorization = "Bearer $AdminToken"; Accept = 'application/json' }
    $result = Invoke-RestMethod -Uri $endpoint -Method Post -Headers $headers
    if (!$result.syncRunId -or $result.studentId -ne "$StudentId" -or
        $result.status -notin @('completed','failed') -or
        $result.processedRecords -ne ($result.tuitionFeesChecked + $result.paymentsChecked) -or
        $result.discrepancies -ne @($result.issues).Count) {
        throw 'Invalid reconciliation response; inspect returned values.'
    }
    Write-Host "OK HTTP 200 - Local reconciliation completed: $($result.status); discrepancies=$($result.discrepancies)"
    $run = Invoke-RestMethod -Uri "${base}/api/sync-runs/$($result.syncRunId)" -Headers $headers
    if ($run.syncRunId -ne $result.syncRunId -or
        $run.processedRecords -ne $result.processedRecords) {
        throw 'SyncRun does not match reconciliation result.'
    }
    Write-Host 'OK HTTP 200 - SyncRun persisted and matches the audit'
}
else {
    Write-Host 'SKIP - Admin JWT not supplied (PAQUITOBOT_QA_ADMIN_JWT)'
}

Write-Host 'FINANCIAL RECONCILIATION SMOKE TEST FINISHED (reported skips are NOT passes)'
