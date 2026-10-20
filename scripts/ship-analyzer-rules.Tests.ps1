<#
.SYNOPSIS
    Self-test for scripts/ship-analyzer-rules.ps1 - plain PowerShell, no Pester.

.DESCRIPTION
    Runs ship-analyzer-rules.ps1 in a child pwsh process against copies of the release tracking
    files in a temp directory and throws on the first mismatch. Run by .github/workflows/ci.yml.

.EXAMPLE
    pwsh -NoProfile -File scripts/ship-analyzer-rules.Tests.ps1
#>
$ErrorActionPreference = 'Stop'
$script = Join-Path $PSScriptRoot 'ship-analyzer-rules.ps1'
$pwsh = (Get-Process -Id $PID).Path

$header = @(
    '; Shipped analyzer releases'
    '; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md'
)
$unshippedHeader = @(
    '; Unshipped analyzer release'
    '; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md'
)
$release900 = @(
    '## Release 9.0.0'
    ''
    '### New Rules'
    ''
    'Rule ID | Category | Severity | Notes'
    '--------|----------|----------|-------'
    'PIPMED001 | Usage | Warning | CatchAllMiddlewareAnalyzer'
)
$rule006 = 'PIPMED006 | Usage | Warning | SomeNewAnalyzer'
$fixtureDirs = New-Object System.Collections.Generic.List[string]

function New-Fixture([string[]]$shipped, [string[]]$unshipped) {
    $dir = Join-Path ([IO.Path]::GetTempPath()) ("ship-analyzer-rules-" + [guid]::NewGuid())
    New-Item -ItemType Directory -Path $dir | Out-Null
    $fixtureDirs.Add($dir)
    $fixture = [pscustomobject]@{
        Dir       = $dir
        Shipped   = Join-Path $dir 'AnalyzerReleases.Shipped.md'
        Unshipped = Join-Path $dir 'AnalyzerReleases.Unshipped.md'
    }
    Set-Content -LiteralPath $fixture.Shipped -Value $shipped
    Set-Content -LiteralPath $fixture.Unshipped -Value $unshipped
    return $fixture
}

function Invoke-Ship($fixture, [string]$version) {
    $output = & $pwsh -NoProfile -File $script -Version $version -ShippedPath $fixture.Shipped -UnshippedPath $fixture.Unshipped 2>&1
    return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output -join "`n") }
}

function Get-Snapshot($fixture) {
    return [pscustomobject]@{
        Shipped   = [IO.File]::ReadAllBytes($fixture.Shipped)
        Unshipped = [IO.File]::ReadAllBytes($fixture.Unshipped)
    }
}

function Assert-Equal($expected, $actual, [string]$message) {
    $expectedText = ($expected | ForEach-Object { "$_" }) -join "`n"
    $actualText = ($actual | ForEach-Object { "$_" }) -join "`n"
    if ($expectedText -cne $actualText) {
        throw "$message`n--- expected ---`n$expectedText`n--- actual ---`n$actualText"
    }
}

function Assert-Unchanged($before, $fixture, [string]$message) {
    $after = Get-Snapshot $fixture
    if ([Convert]::ToBase64String($before.Shipped) -ne [Convert]::ToBase64String($after.Shipped)) { throw "$message - Shipped.md changed" }
    if ([Convert]::ToBase64String($before.Unshipped) -ne [Convert]::ToBase64String($after.Unshipped)) { throw "$message - Unshipped.md changed" }
}

$tests = [ordered]@{
    'Ship_UnshippedHasNewRule_MovesRuleIntoNewReleaseAboveOlderRelease' = {
        $fixture = New-Fixture ($header + '' + $release900) ($unshippedHeader + '' + '### New Rules' + '' + 'Rule ID | Category | Severity | Notes' + '--------|----------|----------|-------' + $rule006)

        $result = Invoke-Ship $fixture '9.1.0'

        Assert-Equal 0 $result.ExitCode "exit code ($($result.Output))"
        $expectedShipped = $header + '' + '## Release 9.1.0' + '' + '### New Rules' + '' + 'Rule ID | Category | Severity | Notes' + '--------|----------|----------|-------' + $rule006 + '' + $release900
        Assert-Equal $expectedShipped (Get-Content -LiteralPath $fixture.Shipped) 'Shipped.md content'
        Assert-Equal ($unshippedHeader + '') (Get-Content -LiteralPath $fixture.Unshipped) 'Unshipped.md content'
    }

    'Ship_UnshippedHasOnlyHeader_LeavesBothFilesUnchanged' = {
        $fixture = New-Fixture ($header + '' + $release900) ($unshippedHeader + '')
        $before = Get-Snapshot $fixture

        $result = Invoke-Ship $fixture '9.0.1'

        Assert-Equal 0 $result.ExitCode "exit code ($($result.Output))"
        Assert-Unchanged $before $fixture 'empty Unshipped.md'
    }

    'Ship_RunTwiceForSameVersion_SecondRunIsNoOp' = {
        $fixture = New-Fixture ($header + '' + $release900) ($unshippedHeader + '' + '### New Rules' + '' + 'Rule ID | Category | Severity | Notes' + '--------|----------|----------|-------' + $rule006)
        Invoke-Ship $fixture '9.1.0' | Out-Null
        $before = Get-Snapshot $fixture

        $result = Invoke-Ship $fixture '9.1.0'

        Assert-Equal 0 $result.ExitCode "exit code ($($result.Output))"
        Assert-Unchanged $before $fixture 'second run'
    }

    'Ship_ReleaseSectionExistsAndUnshippedHasRules_FailsWithoutWriting' = {
        $fixture = New-Fixture ($header + '' + ($release900 -replace '9\.0\.0', '9.1.0')) ($unshippedHeader + '' + '### New Rules' + '' + 'Rule ID | Category | Severity | Notes' + '--------|----------|----------|-------' + $rule006)
        $before = Get-Snapshot $fixture

        $result = Invoke-Ship $fixture '9.1.0'

        if ($result.ExitCode -eq 0) { throw "expected a non-zero exit code, output: $($result.Output)" }
        if ($result.Output -notmatch "already has a '## Release 9\.1\.0' section") { throw "unexpected error: $($result.Output)" }
        Assert-Unchanged $before $fixture 'duplicate release section'
    }

    'Ship_VersionIsNotXYZ_FailsWithoutWriting' = {
        $fixture = New-Fixture ($header + '' + $release900) ($unshippedHeader + '' + '### New Rules' + '' + 'Rule ID | Category | Severity | Notes' + '--------|----------|----------|-------' + $rule006)
        $before = Get-Snapshot $fixture

        $result = Invoke-Ship $fixture 'v9.1'

        if ($result.ExitCode -eq 0) { throw "expected a non-zero exit code, output: $($result.Output)" }
        Assert-Unchanged $before $fixture 'invalid version'
    }
}

$failed = 0
foreach ($name in $tests.Keys) {
    try {
        & $tests[$name]
        Write-Host "PASS $name"
    } catch {
        $failed++
        Write-Host "FAIL $name`n$($_.Exception.Message)"
    }
}

foreach ($dir in $fixtureDirs) { Remove-Item -LiteralPath $dir -Recurse -Force }

if ($failed -gt 0) { throw "$failed of $($tests.Count) test(s) failed" }
Write-Host "All $($tests.Count) tests passed"
