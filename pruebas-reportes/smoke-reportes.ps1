param(
    [string]$BaseUrl = "http://localhost:5114",
    [switch]$AllowRemoteTest
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

try { $baseUri = [Uri]$BaseUrl } catch { throw "BaseUrl is invalid." }
if ($baseUri.Scheme -notin @("http", "https")) { throw "Only HTTP(S) is supported." }
if (-not $AllowRemoteTest -and $baseUri.Host -notin @("localhost", "127.0.0.1", "::1")) {
    throw "Remote host refused. Use a TEST environment and -AllowRemoteTest explicitly."
}
$root = $BaseUrl.TrimEnd("/")
$endpoint = "$root/api/reports/enrollments"

function Get-HttpStatus([string]$Url, [hashtable]$Headers) {
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -Headers $Headers -UseBasicParsing -TimeoutSec 20
        return [int]$response.StatusCode
    } catch {
        if ($_.Exception.Response) {
            return [int]$_.Exception.Response.StatusCode
        }
        throw
    }
}

$anonymous = Get-HttpStatus $endpoint @{}
if ($anonymous -ne 401) { throw "Expected 401 without JWT, received HTTP $anonymous" }
Write-Host "PASS: anonymous reports request returned HTTP 401"

$studentJwt = [Environment]::GetEnvironmentVariable("PAQUITOBOT_QA_STUDENT_JWT")
if (-not [string]::IsNullOrWhiteSpace($studentJwt)) {
    $studentCode = Get-HttpStatus $endpoint @{ Authorization = "Bearer $studentJwt" }
    if ($studentCode -ne 403) { throw "Expected HTTP 403 for student, received $studentCode" }
    Write-Host "PASS: student JWT cannot access general reports (403)"
} else {
    Write-Host "SKIP: student JWT not configured; student access not tested"
}

$adminJwt = [Environment]::GetEnvironmentVariable("PAQUITOBOT_QA_ADMIN_JWT")
if ([string]::IsNullOrWhiteSpace($adminJwt)) {
    Write-Host "SKIP: admin JWT not configured; live JSON/XLSX checks not run"
    exit 0
}
$headers = @{ Authorization = "Bearer $adminJwt" }
$jsonCode = Get-HttpStatus $endpoint $headers
if ($jsonCode -ne 200) { throw "Expected HTTP 200 for admin JSON; received $jsonCode" }
Write-Host "PASS: admin can access enrollment report JSON (200)"

$invalidDates = "$root/api/reports/attendance?from=2026-11-30&to=2026-11-01"
$invalidCode = Get-HttpStatus $invalidDates $headers
if ($invalidCode -ne 400) { throw "Expected HTTP 400 for invalid date range; received $invalidCode" }
Write-Host "PASS: invalid attendance date interval rejected (400)"

$file = Join-Path ([IO.Path]::GetTempPath()) ("paquitobot-qa-report-" + [Guid]::NewGuid() + ".xlsx")
try {
    Invoke-WebRequest -Uri "$root/api/reports/enrollments.xlsx" -Headers $headers -OutFile $file -UseBasicParsing -TimeoutSec 40 | Out-Null
    $bytes = [IO.File]::ReadAllBytes($file)
    if ($bytes.Length -lt 4 -or $bytes[0] -ne 80 -or $bytes[1] -ne 75) {
        throw "Export does not begin with a valid ZIP/XLSX signature"
    }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($file)
    try {
        if ($null -eq $archive.GetEntry("xl/workbook.xml")) { throw "Missing XLSX workbook.xml" }
        if ($null -eq $archive.GetEntry("[Content_Types].xml")) { throw "Missing XLSX content types" }
    } finally {
        $archive.Dispose()
    }
    Write-Host "PASS: admin XLSX download is a valid workbook ZIP"
} finally {
    Remove-Item -LiteralPath $file -Force -ErrorAction SilentlyContinue
}
Write-Host "Live reporting smoke complete. Read-only checks; database was not modified."
