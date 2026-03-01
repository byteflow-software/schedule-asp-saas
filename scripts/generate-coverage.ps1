$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$ApiDir = Join-Path $RootDir "src\Scheduly.Api"
$CoverageOut = Join-Path $ApiDir "wwwroot\coverage"

Write-Host "=== Cleaning previous coverage data ===" -ForegroundColor Cyan
if (Test-Path $CoverageOut) { Remove-Item $CoverageOut -Recurse -Force }
$unitResults = Join-Path $RootDir "tests\Scheduly.UnitTests\TestResults"
$intResults = Join-Path $RootDir "tests\Scheduly.IntegrationTests\TestResults"
if (Test-Path $unitResults) { Remove-Item $unitResults -Recurse -Force }
if (Test-Path $intResults) { Remove-Item $intResults -Recurse -Force }
$frontendCov = Join-Path $RootDir "frontend\coverage"
if (Test-Path $frontendCov) { Remove-Item $frontendCov -Recurse -Force }

New-Item -ItemType Directory -Path (Join-Path $CoverageOut "backend") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $CoverageOut "frontend") -Force | Out-Null

# ── BACKEND COVERAGE (dotnet-coverage — profiler-based, no assembly rewriting) ──
Write-Host ""
Write-Host "=== Running backend tests with coverage ===" -ForegroundColor Cyan

# Build first so --no-build works
dotnet build (Join-Path $RootDir "tests\Scheduly.UnitTests") --verbosity minimal
dotnet build (Join-Path $RootDir "tests\Scheduly.IntegrationTests") --verbosity minimal

$unitCov = Join-Path $unitResults "coverage.cobertura.xml"
$intCov = Join-Path $intResults "coverage.cobertura.xml"

New-Item -ItemType Directory -Path $unitResults -Force | Out-Null
New-Item -ItemType Directory -Path $intResults -Force | Out-Null

Write-Host "Running unit tests..." -ForegroundColor Yellow
dotnet-coverage collect `
  -o "$unitCov" `
  -f cobertura `
  "dotnet test $(Join-Path $RootDir 'tests\Scheduly.UnitTests') --no-build --verbosity minimal"

Write-Host "Running integration tests..." -ForegroundColor Yellow
dotnet-coverage collect `
  -o "$intCov" `
  -f cobertura `
  "dotnet test $(Join-Path $RootDir 'tests\Scheduly.IntegrationTests') --no-build --verbosity minimal"

Write-Host ""
Write-Host "=== Generating backend HTML report ===" -ForegroundColor Cyan

$coverageFiles = Get-ChildItem -Path (Join-Path $RootDir "tests") -Filter "coverage.cobertura.xml" -Recurse |
  Select-Object -ExpandProperty FullName
$coverageArg = $coverageFiles -join ";"

if (-not $coverageArg) {
  Write-Host "WARNING: No coverage XML files found. Skipping backend report." -ForegroundColor Yellow
} else {
  Write-Host "Found coverage files: $coverageArg"
  $backendOut = Join-Path $CoverageOut "backend"
  reportgenerator `
    "-reports:$coverageArg" `
    "-targetdir:$backendOut" `
    "-reporttypes:Html" `
    "-assemblyfilters:+Scheduly.Api;+Scheduly.Application;+Scheduly.Domain;+Scheduly.Infrastructure" `
    "-classfilters:-Scheduly.Infrastructure.Persistence.Migrations.*"

  Write-Host "Backend coverage report generated." -ForegroundColor Green
}

# ── FRONTEND COVERAGE ──
Write-Host ""
Write-Host "=== Running frontend tests with coverage ===" -ForegroundColor Cyan

Push-Location (Join-Path $RootDir "frontend")
npx ng test --watch=false --coverage
Pop-Location

Write-Host ""
Write-Host "=== Copying frontend coverage report ===" -ForegroundColor Cyan

if (Test-Path $frontendCov) {
  Copy-Item -Path (Join-Path $frontendCov "*") -Destination (Join-Path $CoverageOut "frontend") -Recurse -Force
  Write-Host "Frontend coverage report generated." -ForegroundColor Green
} else {
  Write-Host "WARNING: Frontend coverage directory not found." -ForegroundColor Yellow
}

# ── INDEX PAGE ──
Write-Host ""
Write-Host "=== Creating coverage index page ===" -ForegroundColor Cyan

$indexHtml = @"
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Scheduly - Coverage Reports</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #f5f5f5; color: #333; min-height: 100vh; display: flex; align-items: center; justify-content: center; }
    .container { max-width: 500px; width: 100%; padding: 2rem; }
    h1 { font-size: 1.5rem; margin-bottom: 1.5rem; text-align: center; color: #1a1a2e; }
    .card { background: #fff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,.1); padding: 1.5rem; margin-bottom: 1rem; text-decoration: none; display: block; color: inherit; transition: box-shadow .2s; }
    .card:hover { box-shadow: 0 4px 16px rgba(0,0,0,.15); }
    .card h2 { font-size: 1.1rem; margin-bottom: .5rem; }
    .card p { color: #666; font-size: .9rem; }
    .footer { text-align: center; margin-top: 1.5rem; font-size: .8rem; color: #999; }
  </style>
</head>
<body>
  <div class="container">
    <h1>Scheduly - Coverage Reports</h1>
    <a class="card" href="./backend/index.html">
      <h2>Backend (.NET)</h2>
      <p>Unit Tests + Integration Tests - Cobertura do C# (API, Application, Domain, Infrastructure)</p>
    </a>
    <a class="card" href="./frontend/index.html">
      <h2>Frontend (Angular)</h2>
      <p>Jest Tests - Cobertura dos componentes, services, pipes, guards e interceptors</p>
    </a>
    <div class="footer">Gerado com coverlet + ReportGenerator + Jest</div>
  </div>
</body>
</html>
"@

Set-Content -Path (Join-Path $CoverageOut "index.html") -Value $indexHtml -Encoding UTF8

Write-Host ""
Write-Host "=== Done! ===" -ForegroundColor Green
Write-Host "Start the API in Development mode and access /coverage"
Write-Host "Credentials: Coverage__Username / Coverage__Password env vars"
