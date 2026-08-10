<#
.SYNOPSIS
    Runs the Core, Http and Analyzers test suites with code coverage and opens the HTML report.

.DESCRIPTION
    Resolves all paths relative to the repo root (the parent of this script's folder), so it
    works from any working directory: ./scripts/coverage.ps1 or pwsh scripts/coverage.ps1.
    Requires the dotnet-reportgenerator-globaltool (dotnet tool install -g dotnet-reportgenerator-globaltool).

.EXAMPLE
    ./scripts/coverage.ps1
#>
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$resultsDir = Join-Path $repoRoot 'TestResults'

Remove-Item -Recurse -Force $resultsDir -ErrorAction SilentlyContinue

$projects = @(
    'tests/Pipaslot.Mediator.Tests'
    'tests/Pipaslot.Mediator.Http.Tests'
    'tests/Pipaslot.Mediator.Analyzers.Tests'
)

foreach ($project in $projects) {
    dotnet test (Join-Path $repoRoot $project) --collect:"XPlat Code Coverage" --results-directory $resultsDir
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$reportDir = Join-Path $resultsDir 'CoverageReport'
reportgenerator -reports:"$resultsDir/**/coverage.cobertura.xml" -targetdir:$reportDir -reporttypes:Html

Start-Process (Join-Path $reportDir 'index.html')
