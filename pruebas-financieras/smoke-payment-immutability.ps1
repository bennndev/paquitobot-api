param(
    [string]$BaseUrl = 'https://localhost:7032'
)

$ErrorActionPreference = 'Stop'
$base = $BaseUrl.TrimEnd('/')
$id = [guid]::NewGuid().ToString()

function AssertHttp([string]$Label, [string]$Method, [string]$Path, [int]$Expected) {
    $uri = "$base$Path"
    $output = & curl.exe -k -sS -o NUL -w '%{http_code}' -X $Method $uri
    if ($LASTEXITCODE -ne 0) {
        throw "${Label}: curl.exe failed with code $LASTEXITCODE"
    }
    $actual = [int]$output
    if ($actual -ne $Expected) {
        throw "${Label}: expected HTTP $Expected, received HTTP $actual"
    }
    Write-Host "OK HTTP $actual - $Label"
}

# No payment is modified. The GUID is synthetic and does not exist in the database.
AssertHttp 'Global payments require JWT' 'GET' '/api/payments' 401
AssertHttp 'Direct payment update disabled' 'PUT' "/api/payments/$id" 405
AssertHttp 'Physical payment deletion disabled' 'DELETE' "/api/payments/$id" 405

Write-Host 'FINANCIAL PAYMENT IMMUTABILITY SMOKE TEST COMPLETE (3/3)'
