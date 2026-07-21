# Contributing

This file documents maintainer workflows for this repo. It is not part of the published wiki (`docs/wiki/`), which is user-facing package documentation.

## Changelog entries ("Unreleased" convention)

`docs/wiki/Release-notes-and-breaking-changes.md` follows a [Keep a Changelog](https://keepachangelog.com/)-style convention:

- Any PR that changes user-observable behavior adds a bullet under the `## Unreleased` section at the top of the file. Do not invent a version number for it — that happens later, at release time.
- `## Unreleased` stays empty between releases; it's fine for it to have no bullets for a while.

## CI quality gate

- `.github/workflows/ci.yml` builds and runs both test suites (Core + Http) on every PR targeting `main` and on every push to `main`.
- This only blocks merging if branch protection on `main` requires it: Settings → Branches → branch protection rule for `main` → "Require status checks to pass before merging" → select the `build & test` check. Without that setting, a failing CI run shows as a red X on the PR but does not stop the merge button.

## Versioning

- `Pipaslot.Mediator` and `Pipaslot.Mediator.Http` always share the same version number. One git tag releases both packages together.
- The version is derived entirely from the git tag via [MinVer](https://github.com/adamralph/minver) — do not add a `<Version>` back to either `.csproj`.

## Release runbook

1. On `main`, rename `## Unreleased` to `## Version X.Y.Z` in `docs/wiki/Release-notes-and-breaking-changes.md`, and add a fresh empty `## Unreleased` section above it in the same commit (so the next PR has somewhere to add a bullet without having to remember this step).
2. Commit/PR that rename to `main`.
3. Tag and push:
   ```bash
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```
4. Watch the `publish to nuget` GitHub Actions run. It will, in order:
   - fail fast if `## Version X.Y.Z` isn't found in the changelog (the previous step was skipped or the version doesn't match the tag),
   - run the Core and Http test suites,
   - pack both projects (version comes from the tag via MinVer),
   - log in to NuGet.org via OIDC Trusted Publishing and push both packages,
   - create a GitHub Release for the tag using the extracted changelog section as its notes.
5. Verify both packages show up on nuget.org and the GitHub Release was created with the expected notes.
