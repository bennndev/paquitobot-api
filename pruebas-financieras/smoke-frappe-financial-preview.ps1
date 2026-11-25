param(
    [Parameter(Mandatory = $true)]
    [guid]$StudentId,
    [string]$BaseUrl = "https://localhost:7032",
    [ValidateSet("Fees", "Sales Invoice")]
    [string]$Source = "Fees"
)
$ErrorActionPreference = "Stop"
$uri = "$($BaseUrl.TrimEnd('/'))/api/student-finances/$StudentId/frappe/preview?source=$([uri]::EscapeDataString($Source))"

function Get-HttpCode([hashtable]$Headers) {
    try {
        $r = Invoke-WebRequest -Uri $uri -Method Post -Headers $Headers -UseBasicParsing
        return [int]$r.StatusCode
    } catch [System.Net.WebException] {
        if ($_.Exception.Response) { return [int]$_.Exception.Response.StatusCode }
        throw
    }
}
$code = Get-HttpCode @{}
if ($code -ne 401) { throw "Expected HTTP 401 without JWT; got $code" }
Write-Host "OK HTTP 401 - Frappe financial preview requires JWT"

$jwt = $env:PAQUITOBOT_QA_ADMIN_JWT
if ([string]::IsNullOrWhiteSpace($jwt)) {
    Write-Host "SKIP - PAQUITOBOT_QA_ADMIN_JWT absent: did not call Frappe"
    exit 0
}
$result = Invoke-RestMethod -Method Post -Uri $uri -Headers @{Authorization="Bearer $jwt"}
if ($result.studentId -ne "$StudentId" -or $null -eq $result.syncRunId) {
    throw "Invalid preview response"
}
if ($result.safeToApply -ne $false) {
    throw "Preview MUST NOT authorize an import"
}
if ($result.sourceDocumentType -ne $Source) {
    throw "Incorrect source document type"
}
Write-Host "OK HTTP 200 - Read-only $Source preview audited"
Write-Host ("Source records: {0}; submitted: {1}; issues: {2}" -f `
    $result.sourceRecords,$result.submittedDocuments,$result.issueCount)
if ($result.warnings) { $result.warnings | ForEach-Object { Write-Host "WARNING - $_" } }
Write-Host "FRAPPE FINANCIAL PREVIEW FINISHED (FINANCIAL WRITES DISABLED)"
