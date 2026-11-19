param(
    [Parameter(Mandatory = $true)]
    [guid]$StudentId,
    [string]$BaseUrl = "https://localhost:7032"
)
$ErrorActionPreference = "Stop"
$base = $BaseUrl.TrimEnd('/')

function CodeOf([string]$url, [string]$token = "") {
    $headers = @{}
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    try {
        $answer = Invoke-WebRequest -Method Get -Uri $url -Headers $headers -UseBasicParsing
        return [int]$answer.StatusCode
    }
    catch [System.Net.WebException] {
        if ($_.Exception.Response) {
            return [int]$_.Exception.Response.StatusCode
        }
        throw
    }
}

$url = "$base/api/student-finances/$StudentId/schedules"
$unauth = CodeOf $url
if ($unauth -ne 401) { throw "Expected 401 without JWT, got $unauth" }
Write-Host "OK HTTP 401 - Schedules require JWT"

$admin = $env:PAQUITOBOT_QA_ADMIN_JWT
if ([string]::IsNullOrWhiteSpace($admin)) {
    Write-Host "SKIP - PAQUITOBOT_QA_ADMIN_JWT not supplied (authenticated tests not run)"
    Write-Host "SCHEDULES SMOKE FINISHED: 1 PASS, AUTH TESTS SKIPPED"
    exit 0
}

$headers = @{ Authorization = "Bearer $admin" }
$answer = Invoke-RestMethod -Method Get -Uri $url -Headers $headers
if ($answer.studentId -ne "$StudentId" -or $null -eq $answer.enrollments) {
    throw "Schedules response is missing studentId or enrollments"
}
Write-Host "OK HTTP 200 - Authorized schedules read"

$randomProgram = [guid]::NewGuid()
$filtered = Invoke-RestMethod -Method Get -Uri "${url}?programId=$randomProgram" -Headers $headers
if ($filtered.enrollments.Count -ne 0) {
    throw "Unknown programId filter returned enrollments"
}
Write-Host "OK HTTP 200 - Program filter excludes unrelated enrollments"

$randomStudent = [guid]::NewGuid()
$missing = CodeOf "$base/api/student-finances/$randomStudent/schedules" $admin
if ($missing -ne 404) { throw "Expected 404 for missing student, got $missing" }
Write-Host "OK HTTP 404 - Unknown student"

$student = $env:PAQUITOBOT_QA_STUDENT_JWT
if (![string]::IsNullOrWhiteSpace($student)) {
    $code = CodeOf $url $student
    if ($code -ne 200 -and $code -ne 404) {
        throw "Student JWT must see own enrollment (200) or be denied (404), got $code"
    }
    Write-Host "INFO - Student JWT got HTTP $code (check whether it is linked to this student)"
}
Write-Host "FINANCIAL SCHEDULES SMOKE COMPLETE (4/4 with admin)"
