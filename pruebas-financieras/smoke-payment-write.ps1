# PaquitoBot / Issue #41 / Phase 2A - TEST ONLY
# PowerShell 5.1. Requires a QA-admin user promoted in Neon TEST.
param(
    [Parameter(Mandatory = $true)][Guid]$StudentId,
    [string]$BaseUrl = 'https://localhost:7032'
)

$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')
$email = $env:PAQUITOBOT_QA_ADMIN_EMAIL
$password = $env:PAQUITOBOT_QA_ADMIN_PASSWORD
if ([string]::IsNullOrWhiteSpace($email) -or [string]::IsNullOrWhiteSpace($password)) {
    throw 'Configure PAQUITOBOT_QA_ADMIN_EMAIL and PAQUITOBOT_QA_ADMIN_PASSWORD in the current PowerShell session.'
}

function AsJson([hashtable]$data) { return ($data | ConvertTo-Json -Depth 5 -Compress) }
function PostJson([string]$url, [hashtable]$data, [hashtable]$headers = @{}) {
    return Invoke-RestMethod -Uri $url -Method Post -ContentType 'application/json' -Headers $headers -Body (AsJson $data)
}
function ExpectHttp([string]$label, [int]$expected, [string]$url, [string]$method, [hashtable]$data, [hashtable]$headers) {
    try {
        $params = @{ Uri = $url; Method = $method; Headers = $headers; UseBasicParsing = $true; ErrorAction = 'Stop' }
        if ($null -ne $data) {
            $params.ContentType = 'application/json'
            $params.Body = AsJson $data
        }
        $response = Invoke-WebRequest @params
        $actual = [int]$response.StatusCode
    }
    catch [System.Net.WebException] {
        if (!$_.Exception.Response) { throw }
        $actual = [int]$_.Exception.Response.StatusCode
    }
    if ($actual -ne $expected) {
        throw "$label : expected HTTP $expected; got $actual"
    }
    Write-Host "OK HTTP $expected - $label"
}

$login = PostJson "$BaseUrl/api/Auth/login" @{ email = $email; password = $password }
if ([string]::IsNullOrWhiteSpace($login.token) -or $login.role -cne 'Admin') {
    throw 'QA user does not have role Admin. Verify Neon TEST branch and SQL UPDATE.'
}
$headers = @{ Authorization = "Bearer $($login.token)" }
ExpectHttp 'Admin can read payment list' 200 "$BaseUrl/api/payments" 'GET' $null $headers

# A new isolated obligation keeps the earlier 500/200/300 fixture unchanged.
$suffix = [Guid]::NewGuid().ToString('N')
$fee = PostJson "$BaseUrl/api/tuition-fees" @{
    studentId = "$StudentId"
    frappeExternalId = "QA-FIN-FEE-$suffix"
    description = 'Issue41 write test - isolated fee'
    originalAmount = 300
    outstandingBalance = 300
    currencyCode = 'PEN'
    status = 'Pending'
}
if (!$fee.tuitionFeeId) { throw 'Fee creation did not return tuitionFeeId.' }
Write-Host 'OK - Isolated TEST tuition fee created (PEN 300)'

$key = "QA-FIN-PAY-$suffix"
$body = @{
    studentId = "$StudentId"
    tuitionFeeId = "$($fee.tuitionFeeId)"
    frappeExternalId = $key
    amount = 100
    currencyCode = 'PEN'
    paidOn = (Get-Date -Format 'yyyy-MM-dd')
    status = 'Paid'
}
$first = PostJson "$BaseUrl/api/payments" $body $headers
if (!$first.paymentId) { throw 'First POST did not return paymentId.' }
Write-Host 'OK HTTP 201 - First payment PEN 100 created'

$repeat = PostJson "$BaseUrl/api/payments" $body $headers
if ($repeat.paymentId -ne $first.paymentId) {
    throw 'Idempotency failure: repeated request created a different paymentId.'
}
Write-Host 'OK - Same key and body returns same paymentId (idempotent)'

$modified = $body.Clone()
$modified.amount = 120
ExpectHttp 'Same key with modified amount rejected' 409 "$BaseUrl/api/payments" 'POST' $modified $headers

$overpay = $body.Clone()
$overpay.frappeExternalId = "QA-FIN-OVER-$suffix"
$overpay.amount = 201
ExpectHttp 'Overpayment PEN 201 rejected (balance PEN 200)' 409 "$BaseUrl/api/payments" 'POST' $overpay $headers

$wrongCurrency = $body.Clone()
$wrongCurrency.frappeExternalId = "QA-FIN-EUR-$suffix"
$wrongCurrency.amount = 1
$wrongCurrency.currencyCode = 'EUR'
ExpectHttp 'Currency mismatch rejected' 409 "$BaseUrl/api/payments" 'POST' $wrongCurrency $headers

$feeAfter = Invoke-RestMethod -Uri "$BaseUrl/api/tuition-fees/$($fee.tuitionFeeId)" -Headers $headers
if ([decimal]$feeAfter.outstandingBalance -ne 200 -or $feeAfter.status -ne 'Partial') {
    throw "Balance/status error: expected PEN 200 and Partial; actual $($feeAfter.outstandingBalance) / $($feeAfter.status)"
}
Write-Host 'OK - Tuition fee balance PEN 200, status Partial'

$statement = Invoke-RestMethod -Uri "$BaseUrl/api/student-finances/$StudentId/statement" -Headers $headers
$obligation = @($statement.obligations | Where-Object { "$($_.tuitionFeeId)" -eq "$($fee.tuitionFeeId)" })
if ($obligation.Count -ne 1 -or [decimal]$obligation[0].confirmedPaidAmount -ne 100 -or
    [decimal]$obligation[0].outstandingAmount -ne 200) {
    throw 'Statement does not reflect the isolated fee: expected paid 100 / outstanding 200.'
}
Write-Host 'OK - Student statement shows paid PEN 100 / outstanding PEN 200'
Write-Host 'FINANCIAL WRITE SMOKE TEST COMPLETE (9/9)' -ForegroundColor Green
