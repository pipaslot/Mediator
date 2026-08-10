# Contributing

This file documents maintainer workflows for this repo. It is not part of the published wiki (`docs/wiki/`), which is user-facing package documentation.

## Changelog entries ("Unreleased" convention)

`docs/wiki/Release-notes-and-breaking-changes.md` follows a [Keep a Changelog](https://keepachangelog.com/)-style convention:

- Any PR that changes user-observable behavior adds a bullet under the `## Unreleased` section at the top of the file. Do not invent a version number for it — that happens later, at release time.
- `## Unreleased` stays empty between releases; it's fine for it to have no bullets for a while.

## CI quality gate

- `.github/workflows/ci.yml` builds and runs both test suites (Core + Http) on every PR targeting `main` and on every push to `main`.
- This only blocks merging if branch protection on `main` requires it: Settings → Branches → branch protection rule for `main` → "Require status checks to pass before merging" → select the `build & test` check. Without that setting, a failing CI run shows as a red X on the PR but does not stop the merge button.

## `llms.txt`

`llms.txt` in the repo root indexes the wiki for fetch-capable AI tools, following the [llms.txt](https://llmstxt.org/) convention. It is a supplement, never the primary channel — anything a consumer must know belongs in the packaged XML docs or the wiki itself, since a tool behind a corporate firewall may never fetch this file.

- **Links point at `docs/wiki/*.md` on `main` via `raw.githubusercontent.com`, deliberately not at a release tag.** `docs/wiki/` is mirrored by `wiki-sync.yml` to the GitHub Wiki, which has exactly one live version — pinning to a tag would advertise a snapshot that diverges from the wiki every reader is actually looking at, and would buy per-release maintenance for nothing. Raw Markdown rather than the `blob/` view keeps the fetched content free of HTML chrome.
- **Because the links carry no version, the release runbook needs no `llms.txt` step.** Do not add one without also changing the link scheme above.
- **CI enforces that `llms.txt` and `docs/wiki/` agree** (`Check llms.txt covers docs/wiki` in `ci.yml`): a page with no line, or a line pointing at a missing page, fails the build. `_Sidebar.md` is excluded as GitHub Wiki navigation chrome. Adding a wiki page therefore means adding its line here in the same change.

## Versioning

- `Pipaslot.Mediator` and `Pipaslot.Mediator.Http` always share the same version number. One git tag releases both packages together.
- The version is derived entirely from the git tag via [MinVer](https://github.com/adamralph/minver) — do not add a `<Version>` back to either `.csproj`.

## Release runbook

1. On `main`, run:
   ```bash
   pwsh ./scripts/release.ps1 -Version X.Y.Z
   ```
   It only edits files, never commits/tags/pushes:
   - renames `## Unreleased` to `## Version X.Y.Z` in `docs/wiki/Release-notes-and-breaking-changes.md`, and adds a fresh empty `## Unreleased` section above it (so the next PR has somewhere to add a bullet without having to remember this step);
   - if `Pipaslot.Mediator.Analyzers/AnalyzerReleases.Unshipped.md` has any rule entries (new/changed/removed since the last release), moves them into `AnalyzerReleases.Shipped.md` under a matching `## Release X.Y.Z` section — the analyzer ships bundled inside `Pipaslot.Mediator`, so it doesn't get its own version number. Skipped automatically (not an error) when `Unshipped.md` is empty — not every release touches the analyzer.
2. Review the diff, then commit/PR it to `main`.
3. Tag and push:
   ```bash
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```
4. Watch the `publish to nuget` GitHub Actions run. It will, in order:
   - fail fast if `## Version X.Y.Z` isn't found in the changelog (the previous step was skipped or the version doesn't match the tag),
   - run the Core, Http and Analyzers test suites,
   - pack both projects (version comes from the tag via MinVer),
   - log in to NuGet.org via OIDC Trusted Publishing and push both packages,
   - create a GitHub Release for the tag using the extracted changelog section as its notes.
5. Verify both packages show up on nuget.org and the GitHub Release was created with the expected notes.
