param(
    [Parameter(Mandatory = $true)]
    [guid]$StudentId,
    [string]$BaseUrl = "https://localhost:7032",
    [switch]$ConfirmTestDatabase
)
$ErrorActionPreference = "Stop"
if (-not $ConfirmTestDatabase) {
    throw "This script writes QA records. Specify -ConfirmTestDatabase only for Neon TEST."
}
if ([string]::IsNullOrWhiteSpace($env:PAQUITOBOT_QA_ADMIN_JWT)) {
    throw "PAQUITOBOT_QA_ADMIN_JWT is required."
}
$base = $BaseUrl.TrimEnd('/')
$head = @{ Authorization = "Bearer $env:PAQUITOBOT_QA_ADMIN_JWT" }
$id = [guid]::NewGuid().ToString("N").Substring(0, 10)

function Post([string]$route, [hashtable]$body) {
    return Invoke-RestMethod -Uri "$base$route" -Headers $head -Method Post `
        -ContentType "application/json" -Body ($body | ConvertTo-Json -Depth 8)
}
$program = Post "/api/programs" @{
    frappeExternalId = "QA-FIN-PROG-$id"
    name = "QA Finance Program $id"
    status = "Active"
}
$enrollment = Post "/api/program-enrollments" @{
    studentId = "$StudentId"
    programId = $program.programId
    frappeExternalId = "QA-FIN-ENROLL-$id"
    status = "Active"
}
$structure = Post "/api/fee-structures" @{
    name = "QA Finance Structure $id"
    programId = $program.programId
    currencyCode = "PEN"
    totalAmount = 250
}
$schedule = Post "/api/fee-schedules" @{
    name = "QA Finance Installment $id"
    feeStructureId = $structure.feeStructureId
    scheduledAmount = 250
    dueDate = (Get-Date).AddDays(20).ToString("yyyy-MM-dd")
}
$fee = Post "/api/tuition-fees" @{
    studentId = "$StudentId"
    feeScheduleId = $schedule.feeScheduleId
    description = "QA Student Installment $id"
    currencyCode = "PEN"
    originalAmount = 250
    outstandingBalance = 250
    status = "Pending"
}
$uri = "$base/api/student-finances/$StudentId/schedules?programId=$($program.programId)"
$data = Invoke-RestMethod -Uri $uri -Headers $head
$found = @($data.enrollments | Where-Object { $_.programEnrollmentId -eq $enrollment.programEnrollmentId })
if ($found.Count -ne 1) { throw "Expected exactly one new enrollment in schedule catalog" }
$structures = @($found[0].feeStructures | Where-Object { $_.feeStructureId -eq $structure.feeStructureId })
if ($structures.Count -ne 1) { throw "Expected fee structure under enrollment" }
$schedules = @($structures[0].schedules | Where-Object { $_.feeScheduleId -eq $schedule.feeScheduleId })
if ($schedules.Count -ne 1) { throw "Expected schedule under fee structure" }
$linkedFees = @($schedules[0].studentTuitionFees | Where-Object { $_.tuitionFeeId -eq $fee.tuitionFeeId })
if ($linkedFees.Count -ne 1 -or $linkedFees[0].originalAmount -ne 250) {
    throw "Expected student-specific tuition fee PEN 250 in its schedule"
}
Write-Host "OK - Academic enrollment, structure, schedule and individual fee linked"
Write-Host "Created QA ProgramId: $($program.programId)"
Write-Host "Created QA EnrollmentId: $($enrollment.programEnrollmentId)"
Write-Host "FINANCIAL SCHEDULES FIXTURE COMPLETE"
