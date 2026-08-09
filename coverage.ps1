Remove-Item -Recurse -Force TestResults -ErrorAction SilentlyContinue

$projects = @(
    "tests/Pipaslot.Mediator.Tests"
    "tests/Pipaslot.Mediator.Http.Tests"
    "tests/Pipaslot.Mediator.Analyzers.Tests"
)

foreach ($project in $projects) {
    dotnet test $project --collect:"XPlat Code Coverage" --results-directory TestResults
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:TestResults/CoverageReport -reporttypes:Html

Start-Process "TestResults/CoverageReport/index.html"
