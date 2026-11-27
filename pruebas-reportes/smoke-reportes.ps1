param(
    [string]$BaseUrl = "http://localhost:5114",
    [switch]$AllowRemoteTest,
    [int]$ExpectedEnrollmentTotal = -1
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

try { $baseUri = [Uri]$BaseUrl } catch { throw "BaseUrl is invalid." }
if ($baseUri.Scheme -notin @("http", "https")) { throw "Only HTTP(S) is supported." }
if (-not $AllowRemoteTest -and $baseUri.Host -notin @("localhost", "127.0.0.1", "::1")) {
    throw "Remote host refused. Use a TEST environment and -AllowRemoteTest explicitly."
}
if ($ExpectedEnrollmentTotal -lt -1) { throw "ExpectedEnrollmentTotal must be -1 or greater." }
$root = $BaseUrl.TrimEnd("/")

function Get-HttpStatus([string]$Url, [hashtable]$Headers) {
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -Headers $Headers -UseBasicParsing -TimeoutSec 25
        return [int]$response.StatusCode
    } catch {
        if ($null -ne $_.Exception.Response) {
            return [int]$_.Exception.Response.StatusCode
        }
        throw
    }
}

function Assert-HttpStatus([string]$Url, [hashtable]$Headers, [int]$Expected, [string]$Label) {
    $actual = Get-HttpStatus $Url $Headers
    if ($actual -ne $Expected) { throw "$Label expected HTTP $Expected, received $actual" }
    Write-Host "PASS: $Label (HTTP $actual)"
}

$reports = @("enrollments", "attendance", "assessments")
foreach ($report in $reports) {
    Assert-HttpStatus "$root/api/reports/$report" @{} 401 "anonymous $report"
}

$studentJwt = [Environment]::GetEnvironmentVariable("PAQUITOBOT_QA_STUDENT_JWT")
if (-not [string]::IsNullOrWhiteSpace($studentJwt)) {
    $studentHeaders = @{ Authorization = "Bearer $studentJwt" }
    foreach ($report in $reports) {
        Assert-HttpStatus "$root/api/reports/$report" $studentHeaders 403 "student $report"
    }
} else {
    Write-Host "SKIP: student JWT not configured"
}

$adminJwt = [Environment]::GetEnvironmentVariable("PAQUITOBOT_QA_ADMIN_JWT")
if ([string]::IsNullOrWhiteSpace($adminJwt)) {
    Write-Host "SKIP: admin JWT not configured; JSON/XLSX checks not run"
    exit 0
}
$adminHeaders = @{ Authorization = "Bearer $adminJwt" }

foreach ($report in $reports) {
    Assert-HttpStatus "$root/api/reports/$report" $adminHeaders 200 "admin $report JSON"
}

# Verify administrative enrollment aggregations match the TEST environment expectation.
$enrollmentResponse = Invoke-RestMethod -Uri "$root/api/reports/enrollments" `
    -Method Get -Headers $adminHeaders -TimeoutSec 25
$enrollmentRows = @()
if ($null -ne $enrollmentResponse) { $enrollmentRows = @($enrollmentResponse) }
$enrollmentTotal = 0
foreach ($row in $enrollmentRows) {
    if ([int]$row.enrollmentRecords -lt 0 -or [int]$row.distinctStudents -lt 0 -or
        [int]$row.distinctStudents -gt [int]$row.enrollmentRecords) {
        throw "Invalid enrollment counts in the JSON response"
    }
    $enrollmentTotal += [int]$row.enrollmentRecords
}
if ($ExpectedEnrollmentTotal -ge 0 -and $enrollmentTotal -ne $ExpectedEnrollmentTotal) {
    throw "Enrollment total mismatch: expected $ExpectedEnrollmentTotal, got $enrollmentTotal"
}
Write-Host "PASS: enrollment JSON rows=$($enrollmentRows.Count); records=$enrollmentTotal"

Assert-HttpStatus "$root/api/reports/attendance?from=2026-11-30&to=2026-11-01" `
    $adminHeaders 400 "invalid attendance dates"

# Ensure all XLSX routes produce usable workbooks, not just a ZIP prefix.
Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($report in $reports) {
    $file = Join-Path ([IO.Path]::GetTempPath()) ("paquitobot-qa-" + [Guid]::NewGuid() + ".xlsx")
    try {
        Invoke-WebRequest -Uri "$root/api/reports/${report}.xlsx" -Headers $adminHeaders `
            -OutFile $file -UseBasicParsing -TimeoutSec 40 | Out-Null
        $bytes = [IO.File]::ReadAllBytes($file)
        if ($bytes.Length -lt 4 -or $bytes[0] -ne 80 -or $bytes[1] -ne 75) {
            throw "Invalid XLSX ZIP signature for $report"
        }
        $archive = [IO.Compression.ZipFile]::OpenRead($file)
        try {
            if ($null -eq $archive.GetEntry("xl/workbook.xml")) { throw "Missing workbook.xml: $report" }
            if ($null -eq $archive.GetEntry("[Content_Types].xml")) { throw "Missing content types: $report" }
        } finally {
            $archive.Dispose()
        }
        Write-Host "PASS: admin $report XLSX valid"
    } finally {
        Remove-Item -LiteralPath $file -Force -ErrorAction SilentlyContinue
    }
}
Write-Host "PASS: reporting smoke complete; all requests were read-only"
