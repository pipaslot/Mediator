# Contributing

This file documents maintainer workflows for this repo. It is not part of the published wiki (`docs/wiki/`), which is user-facing package documentation.

## Release notes

Release notes live in [GitHub Releases](https://github.com/pipaslot/Mediator/releases), one per version — there is no changelog file to keep up to date in each PR. They are written when the release is created (see [Releasing](#releasing)), starting from the list of merged PRs that GitHub generates.

- **The PR title is that list's entry**, so write it for a package consumer: one short English sentence in the imperative saying what changed ("Add HTTP GET allowlist", not "Added…" or "WIP"). No prefix convention is enforced.
- **A breaking change also updates the [Migration guide](docs/wiki/Migration-guide.md) in the same PR** — add the step to the section of the upcoming major version (create it if it doesn't exist yet). The release notes of that version then link there instead of repeating the migration.
- The history up to 9.1.0 is archived in [docs/archive/Release-notes-and-breaking-changes.md](docs/archive/Release-notes-and-breaking-changes.md) and is no longer extended.

### Repository settings

- Settings → Branches → branch protection rule for `main` → "Require status checks to pass before merging": select `build & test`.

## CI quality gate

- `.github/workflows/ci.yml` builds and runs both test suites (Core + Http) on every PR targeting `main` and on every push to `main`.
- This only blocks merging if branch protection on `main` requires it: Settings → Branches → branch protection rule for `main` → "Require status checks to pass before merging" → select the `build & test` check. Without that setting, a failing CI run shows as a red X on the PR but does not stop the merge button.

## `llms.txt`

`llms.txt` in the repo root indexes the wiki for fetch-capable AI tools, following the [llms.txt](https://llmstxt.org/) convention. It is a supplement, never the primary channel — anything a consumer must know belongs in the packaged XML docs or the wiki itself, since a tool behind a corporate firewall may never fetch this file.

- **Links point at `docs/wiki/*.md` on `main` via `raw.githubusercontent.com`, deliberately not at a release tag.** `docs/wiki/` is mirrored by `wiki-sync.yml` to the GitHub Wiki, which has exactly one live version — pinning to a tag would advertise a snapshot that diverges from the wiki every reader is actually looking at, and would buy per-release maintenance for nothing. Raw Markdown rather than the `blob/` view keeps the fetched content free of HTML chrome.
- **Because the links carry no version, releasing needs no `llms.txt` step.** Do not add one without also changing the link scheme above.
- **CI enforces that `llms.txt` and `docs/wiki/` agree** (`Check llms.txt covers docs/wiki` in `ci.yml`): a page with no line, or a line pointing at a missing page, fails the build. `_Sidebar.md` is excluded as GitHub Wiki navigation chrome. Adding a wiki page therefore means adding its line here in the same change.

## Versioning

- `Pipaslot.Mediator` and `Pipaslot.Mediator.Http` always share the same version number. One git tag releases both packages together.
- The version is derived entirely from the git tag `vX.Y.Z` via [MinVer](https://github.com/adamralph/minver) — do not add a `<Version>` back to either `.csproj`. The tag is created together with the GitHub Release.

## Releasing

1. **Analyzer rules (only if `Pipaslot.Mediator.Analyzers/AnalyzerReleases.Unshipped.md` has rule entries)** — move them to `AnalyzerReleases.Shipped.md` under `## Release X.Y.Z` and merge that to `main` first:
   ```bash
   pwsh ./scripts/ship-analyzer-rules.ps1 -Version X.Y.Z
   ```
   The analyzer ships bundled inside `Pipaslot.Mediator` (`dotnet pack` adds it), so this is only its release tracking, not packaging. Not every release touches the analyzer; the script does nothing when there is nothing to ship.
2. **GitHub → Releases → Draft a new release.**
   - *Choose a tag* → type `vX.Y.Z` → *Create new tag on publish*; *Target* `main` (or a `release/*` branch for a fix of an older version line).
   - *Generate release notes* fills in the PRs merged since the previous release. Edit them into the notes for consumers: drop internal-only entries, call out breaking changes and link the [Migration guide](https://github.com/pipaslot/Mediator/wiki/Migration-guide) for a major version.
   - Choose the version by [SemVer](https://semver.org/): breaking change → major, new API → minor, fixes only → patch. *Set as a pre-release* for a `vX.Y.Z-suffix` tag.
3. **Publish release.** The `publish to nuget` workflow (`.github/workflows/publish.yml`) then, for the tagged commit:
   - fails if the tag isn't `vX.Y.Z[-suffix]` or the commit isn't on `main` or a `release/*` branch,
   - warns if analyzer rules were left in `AnalyzerReleases.Unshipped.md` (step 1 then goes into a follow-up PR with the released version),
   - runs the Core, Http and Analyzers test suites,
   - packs both projects (MinVer reads the version from the tag) and fails if the package version differs from the tag,
   - logs in to NuGet.org via OIDC Trusted Publishing and pushes both packages.

If the run fails (e.g. a flaky test), re-run it from the Actions tab. If the fix needs a code change, merge it, delete the GitHub Release together with its tag, and create the release again — the failed version never reached NuGet.

The NuGet Trusted Publishing policy for both packages is bound to `pipaslot/Mediator` and the workflow file `publish.yml` without a GitHub environment — keep that file name and do not add an `environment:` to the workflow.
