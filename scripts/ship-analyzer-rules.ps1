<#
.SYNOPSIS
    Moves the analyzer rules from AnalyzerReleases.Unshipped.md into AnalyzerReleases.Shipped.md
    under a "## Release X.Y.Z" section.

.DESCRIPTION
    Run by hand before a release whenever Unshipped.md has rule entries, and merge the result to main
    before publishing the GitHub Release (see the Releasing section of CONTRIBUTING.md). The
    `publish to nuget` workflow warns when a release still has unshipped rules. It only edits
    files - it never commits or pushes.

    The rule tables of Unshipped.md (New/Changed/Removed Rules) are inserted as a new
    "## Release X.Y.Z" section before the newest release in Shipped.md, and Unshipped.md is reset
    back to its bare header.

    Safe to run repeatedly:
    - Unshipped.md has no rule entries -> nothing is written (no empty release section), exit 0.
      This also covers a re-run after the rules were already shipped.
    - Shipped.md already has "## Release X.Y.Z" while Unshipped.md still has rule entries -> error,
      nothing is written.

.PARAMETER Version
    The release version, e.g. "9.1.0".

.PARAMETER ShippedPath
    Path to AnalyzerReleases.Shipped.md. Defaults to the file in Pipaslot.Mediator.Analyzers.

.PARAMETER UnshippedPath
    Path to AnalyzerReleases.Unshipped.md. Defaults to the file in Pipaslot.Mediator.Analyzers.

.EXAMPLE
    ./scripts/ship-analyzer-rules.ps1 -Version 9.1.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version,

    [string]$ShippedPath,

    [string]$UnshippedPath
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $ShippedPath) { $ShippedPath = Join-Path $repoRoot 'Pipaslot.Mediator.Analyzers/AnalyzerReleases.Shipped.md' }
if (-not $UnshippedPath) { $UnshippedPath = Join-Path $repoRoot 'Pipaslot.Mediator.Analyzers/AnalyzerReleases.Unshipped.md' }

# Table header is whitespace-sensitive (RS2007) - reuse the exact line rather than reformatting it.
$analyzerTableHeader = 'Rule ID | Category | Severity | Notes'
$analyzerTableSeparator = '--------|----------|----------|-------'
$analyzerSectionNames = @('New Rules', 'Changed Rules', 'Removed Rules')

function Get-UnshippedSections([string[]]$lines) {
    $sections = @{}
    $current = $null
    foreach ($line in $lines) {
        if ($line -match '^### (.+)$') {
            $current = $Matches[1]
            $sections[$current] = New-Object System.Collections.Generic.List[string]
            continue
        }
        if ($null -eq $current) { continue }
        if ($line -eq $analyzerTableHeader -or $line -eq $analyzerTableSeparator -or $line.Trim() -eq '') { continue }
        $sections[$current].Add($line)
    }
    return $sections
}

function Remove-TrailingEmptyLines([string[]]$lines) {
    $count = $lines.Count
    while ($count -gt 0 -and $lines[$count - 1] -eq '') { $count-- }
    if ($count -eq 0) { return , @() }
    return , $lines[0..($count - 1)]
}

# --- Validate everything up front so a failure never leaves a half-updated file. ---

$unshippedLines = @(Get-Content -LiteralPath $UnshippedPath)
$analyzerSections = Get-UnshippedSections $unshippedLines
$nonEmptyAnalyzerSections = @($analyzerSectionNames | Where-Object { $analyzerSections.ContainsKey($_) -and $analyzerSections[$_].Count -gt 0 })

if ($nonEmptyAnalyzerSections.Count -eq 0) {
    Write-Host "$UnshippedPath has no rule entries - nothing to ship."
    exit 0
}

$shippedLines = @(Get-Content -LiteralPath $ShippedPath)
if ($shippedLines -match "^## Release $([regex]::Escape($Version))(\s|$)") {
    Write-Error "$ShippedPath already has a '## Release $Version' section while $UnshippedPath still has rule entries - refusing to duplicate it."
}

# --- All checks passed - now actually write the files. ---

$block = New-Object System.Collections.Generic.List[string]
$block.Add("## Release $Version")
$block.Add('')
foreach ($name in $nonEmptyAnalyzerSections) {
    $block.Add("### $name")
    $block.Add('')
    $block.Add($analyzerTableHeader)
    $block.Add($analyzerTableSeparator)
    foreach ($row in $analyzerSections[$name]) { $block.Add($row) }
    $block.Add('')
}

# Newest release goes first, right after the file's leading comment header.
$insertAt = ($shippedLines | Select-String -Pattern '^## Release ' | Select-Object -First 1).LineNumber
$before = if ($insertAt -gt 1) { $shippedLines[0..($insertAt - 2)] } elseif ($insertAt) { @() } else { $shippedLines }
$after = if ($insertAt) { $shippedLines[($insertAt - 1)..($shippedLines.Count - 1)] } else { @() }
$before = Remove-TrailingEmptyLines $before

Set-Content -LiteralPath $ShippedPath -Value (@($before) + '' + $block + @($after))

# Reset Unshipped.md back to its bare template (header comment only, no leftover tables).
$firstSectionLine = ($unshippedLines | Select-String -Pattern '^### ' | Select-Object -First 1).LineNumber
$headerLines = Remove-TrailingEmptyLines @($unshippedLines | Select-Object -First ($firstSectionLine - 1))
Set-Content -LiteralPath $UnshippedPath -Value (@($headerLines) + '')

Write-Host "Moved $($nonEmptyAnalyzerSections -join ', ') into '## Release $Version' in $ShippedPath"
Write-Host "Cleared $UnshippedPath"
