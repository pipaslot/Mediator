<#
.SYNOPSIS
    Prepares a Pipaslot.Mediator release by closing out the "Unreleased" sections
    in the changelog and (if applicable) the analyzer release-tracking files.

.DESCRIPTION
    Part of the release runbook (see CONTRIBUTING.md), covering its steps 1-2. Run
    this on main; it only edits files - it never commits, tags, or pushes. Review
    the diff and commit it yourself, then continue with the rest of the runbook
    (tag vX.Y.Z and push).

    1. Renames "## Unreleased" to "## Version X.Y.Z" in
       docs/wiki/Release-notes-and-breaking-changes.md, and adds a fresh empty
       "## Unreleased" section above it.
    2. If Pipaslot.Mediator.Analyzers/AnalyzerReleases.Unshipped.md has any rule
       entries (new/changed/removed), moves them into AnalyzerReleases.Shipped.md
       under a matching "## Release X.Y.Z" section, then clears Unshipped.md back
       to its bare template. Skipped (not an error) when there's nothing to move -
       not every release touches the analyzer.

    Fails before writing anything if "## Unreleased" has no bullets while step 2
    has rule entries to ship - an analyzer rule change always needs its own
    changelog bullet (see the "### Roslyn analyzer changes" subsection convention
    in the roslyn-analyzers skill), so an empty changelog at that point means one
    was forgotten, not that there's nothing to say.

.PARAMETER Version
    The release version, e.g. "9.0.0". Must match the git tag (vX.Y.Z) you'll push afterwards.

.EXAMPLE
    ./scripts/release.ps1 -Version 9.0.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

# Table header is whitespace-sensitive (RS2007) - reuse the exact line rather than reformatting it.
$analyzerTableHeader = 'Rule ID | Category | Severity | Notes'
$analyzerTableSeparator = '--------|----------|----------|-------'
$analyzerSectionNames = @('New Rules', 'Changed Rules', 'Removed Rules')
$unshippedPath = Join-Path $repoRoot 'Pipaslot.Mediator.Analyzers\AnalyzerReleases.Unshipped.md'
$shippedPath = Join-Path $repoRoot 'Pipaslot.Mediator.Analyzers\AnalyzerReleases.Shipped.md'
$changelogPath = Join-Path $repoRoot 'docs\wiki\Release-notes-and-breaking-changes.md'

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

function Get-ChangelogUnreleasedBody([string[]]$lines) {
    $nextHeaderIndex = $lines.Count
    for ($i = 1; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^## ') { $nextHeaderIndex = $i; break }
    }
    if ($nextHeaderIndex -le 1) { return @() }
    return $lines[1..($nextHeaderIndex - 1)] | Where-Object { $_.Trim() -ne '' }
}

# --- Validate everything up front so a failure never leaves a half-updated release. ---

$unshippedLines = Get-Content -LiteralPath $unshippedPath
$analyzerSections = Get-UnshippedSections $unshippedLines
$nonEmptyAnalyzerSections = $analyzerSectionNames | Where-Object { $analyzerSections.ContainsKey($_) -and $analyzerSections[$_].Count -gt 0 }

$changelogLines = Get-Content -LiteralPath $changelogPath
if ($changelogLines[0] -ne '## Unreleased') {
    Write-Error "$changelogPath does not start with '## Unreleased' - check it wasn't already released."
}
if ($changelogLines -match "^## Version $([regex]::Escape($Version))(\s|$)") {
    Write-Error "$changelogPath already has a '## Version $Version' section - refusing to duplicate it."
}
$unreleasedBody = Get-ChangelogUnreleasedBody $changelogLines
if (-not $unreleasedBody) {
    if ($nonEmptyAnalyzerSections) {
        Write-Error "'## Unreleased' in $changelogPath has no bullets, but $unshippedPath has pending $($nonEmptyAnalyzerSections -join ', ') entries - add a changelog bullet for them (see the '### Roslyn analyzer changes' subsection) before releasing."
    }
    Write-Warning "'## Unreleased' has no bullets - releasing $Version with an empty changelog section."
}

if ($nonEmptyAnalyzerSections) {
    $shippedLines = Get-Content -LiteralPath $shippedPath
    if ($shippedLines -match "^## Release $([regex]::Escape($Version))(\s|$)") {
        Write-Error "$shippedPath already has a '## Release $Version' section - refusing to duplicate it."
    }
}

# --- All checks passed - now actually write the files. ---

$changelogLines[0] = "## Version $Version"
Set-Content -LiteralPath $changelogPath -Value (@('## Unreleased', '') + $changelogLines)
Write-Host "Renamed '## Unreleased' to '## Version $Version' in $changelogPath"

if (-not $nonEmptyAnalyzerSections) {
    Write-Host "$unshippedPath has no rule entries - skipping the analyzer release step."
} else {
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

    # Newest release goes first, right after the file's leading comment header, matching
    # this repo's changelog convention (docs/wiki/Release-notes-and-breaking-changes.md).
    $shippedLines = Get-Content -LiteralPath $shippedPath
    $insertAt = ($shippedLines | Select-String -Pattern '^## Release ' | Select-Object -First 1).LineNumber
    $before = if ($insertAt) { $shippedLines[0..($insertAt - 2)] } else { $shippedLines }
    $after = if ($insertAt) { $shippedLines[($insertAt - 1)..($shippedLines.Count - 1)] } else { @() }
    while ($before.Count -gt 0 -and $before[-1] -eq '') { $before = $before[0..($before.Count - 2)] }

    Set-Content -LiteralPath $shippedPath -Value ($before + '' + $block + $after)

    # Reset Unshipped.md back to its bare template (header comment only, no leftover tables).
    $headerLines = $unshippedLines | Select-Object -First (($unshippedLines | Select-String -Pattern '^### ' | Select-Object -First 1).LineNumber - 1)
    while ($headerLines.Count -gt 0 -and $headerLines[-1] -eq '') { $headerLines = $headerLines[0..($headerLines.Count - 2)] }
    Set-Content -LiteralPath $unshippedPath -Value ($headerLines + '')

    Write-Host "Moved $($nonEmptyAnalyzerSections -join ', ') into '## Release $Version' in $shippedPath"
    Write-Host "Cleared $unshippedPath"
}

Write-Host ''
Write-Host "Review the diff, then commit, tag vX.Y.Z, and push per CONTRIBUTING.md."
