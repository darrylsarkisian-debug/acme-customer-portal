# Builds, tests, and smoke tests the Acme Customer Portal. Logs everything to .demo-kit\verify.log
# Run from the repo folder:
#   powershell -ExecutionPolicy Bypass -File .\scripts\verify-local.ps1
# Add -KeepRunning to leave the site running at http://localhost:5080 afterwards.

param([switch]$KeepRunning)

$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo
$logDir = Join-Path $repo '.demo-kit'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$log = Join-Path $logDir 'verify.log'
"Verify started $(Get-Date -Format s)" | Out-File $log -Encoding utf8

function Log($text) {
    Write-Host $text
    $text | Out-File $log -Append -Encoding utf8
}

function Run-Step($name, [scriptblock]$command) {
    Log ""
    Log "=== $name ==="
    & $command 2>&1 | Tee-Object -FilePath $log -Append
    if ($LASTEXITCODE -ne 0) {
        Log "RESULT: $name FAILED (exit code $LASTEXITCODE)"
        Log "Log saved at $log"
        exit 1
    }
    Log "RESULT: $name passed"
}

Log "dotnet version: $(dotnet --version)"

Run-Step "1. dotnet build" { dotnet build src/AcmePortal.Web }
Run-Step "2. dotnet test"  { dotnet test tests/AcmePortal.Tests }

Log ""
Log "=== 3. dotnet run and smoke test ==="
$webProject = Join-Path $repo 'src\AcmePortal.Web'
$outLog = Join-Path $logDir 'app-out.log'
$errLog = Join-Path $logDir 'app-err.log'
$app = Start-Process -FilePath 'dotnet' `
    -ArgumentList 'run', '--no-build', '--project', "`"$webProject`"", '--urls', 'http://localhost:5080' `
    -WorkingDirectory $webProject `
    -RedirectStandardOutput $outLog -RedirectStandardError $errLog `
    -PassThru -WindowStyle Hidden

$base = 'http://localhost:5080'
$ready = $false
for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Seconds 1
    try {
        $r = Invoke-WebRequest -Uri $base -UseBasicParsing -TimeoutSec 3
        if ($r.StatusCode -eq 200) { $ready = $true; break }
    } catch { }
}

$failed = $false
if (-not $ready) {
    Log "RESULT: site did not respond on $base within 30 seconds"
    $failed = $true
} else {
    Log "Site is up."
    try {
        $homePage = Invoke-WebRequest -Uri $base -UseBasicParsing
        Log "GET /            -> $($homePage.StatusCode), contains welcome text: $($homePage.Content -match 'Welcome to the Acme Customer Portal')"

        $login = Invoke-WebRequest -Uri "$base/Account/Login" -UseBasicParsing
        Log "GET /Account/Login -> $($login.StatusCode)"

        $signIn = Invoke-WebRequest -Uri "$base/Account/Login" -Method Post -Body @{ email = 'dana@acme.example' } `
            -SessionVariable portal -UseBasicParsing
        $hasOrders = $signIn.Content -match 'Industrial label printer'
        Log "POST sign in     -> $($signIn.StatusCode), shows Dana's orders: $hasOrders"

        $otherStatus = 0
        try {
            $other = Invoke-WebRequest -Uri "$base/Orders/Details/1004" -WebSession $portal -UseBasicParsing
            $otherStatus = [int]$other.StatusCode
        } catch {
            if ($_.Exception.Response) { $otherStatus = [int]$_.Exception.Response.StatusCode }
        }
        Log "GET /Orders/Details/1004 (Marcus's order, signed in as Dana) -> $otherStatus (expect 404)"

        if (-not $hasOrders -or $otherStatus -ne 404) { $failed = $true }
    } catch {
        Log "Smoke test error: $($_.Exception.Message)"
        $failed = $true
    }
}

if ($KeepRunning -and -not $failed) {
    Log "Leaving the site running at $base (process id $($app.Id)). Stop it with: taskkill /PID $($app.Id) /T /F"
    Start-Process $base
} else {
    if (-not $app.HasExited) { taskkill /PID $app.Id /T /F | Out-Null }
    Start-Sleep -Seconds 1
    Log ""
    Log "--- app stdout ---"
    if (Test-Path $outLog) { Get-Content $outLog | Out-File $log -Append -Encoding utf8 }
    Log "--- app stderr ---"
    if (Test-Path $errLog) { Get-Content $errLog | Out-File $log -Append -Encoding utf8 }
}

Log ""
if ($failed) { Log "RESULT: SMOKE TEST FAILED. Log saved at $log"; exit 1 }
Log "RESULT: ALL CHECKS PASSED. Log saved at $log"
