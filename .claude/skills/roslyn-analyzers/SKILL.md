---
name: roslyn-analyzers
description: How to design, implement, test, package, and document a diagnostic rule in Pipaslot.Mediator.Analyzers - the Roslyn analyzer shipped inside the Pipaslot.Mediator NuGet package. Use this whenever adding a new PIPMEDxxx rule, changing an existing one's detection logic, debugging a failing Microsoft.CodeAnalysis.Testing test, or fixing analyzer packaging (why the .dll isn't landing under analyzers/dotnet/cs, why it isn't loading in a consuming project). Also use it when deciding whether a proposed check belongs in the analyzer at all versus the wiki or an editorconfig rule.
---

# Roslyn analyzers for Pipaslot.Mediator

`Pipaslot.Mediator.Analyzers` ships inside the main `Pipaslot.Mediator` NuGet package and gives every
consumer - human or agent - compiler feedback on misuse of the library, with zero install step. This
skill covers project layout, the testing infrastructure's sharp edges, and the packaging plumbing, all
confirmed by building `PIPMED001` (the first shipped rule, in `CatchAllMiddlewareAnalyzer.cs`).

## Does this belong in the analyzer?

Before writing any code, check the candidate against this triage rule: **a rule belongs in the analyzer
only if it's decidable from syntax or the semantic model with a low false-positive rate.** If confirming
the bug requires knowing the built DI container, a middleware pipeline condition (those are delegates -
opaque to static analysis), or cross-project registration state, it stays out of the analyzer and goes in
project documentation/guidance instead.

The corollary that matters day to day: **a false positive is more expensive than a missing rule.** A
noisy analyzer gets disabled wholesale by the consumer, taking every rule down with it, not just the
noisy one. When a check's semantic reasoning has to guess, guess toward not reporting. Every helper in
`CatchAllMiddlewareAnalyzer.cs` that decides whether to suppress (`RethrowsException`,
`CallsAddException`) is written to over-suppress rather than over-report, and says so in its doc comment.

## Before writing detection logic: does a sanctioned alternative already exist?

The single biggest correctness trap in this kind of rule isn't the Roslyn API - it's flagging a pattern
that the library actually documents and recommends. `PIPMED001` originally flagged every non-rethrowing
`catch (Exception)` inside an `IMediatorMiddleware`, which was correct for the deprecated
`ErrorHandlingMiddleware` pattern but also fired on `catch (Exception e) { context.AddException(e); }` -
a pattern the wiki explicitly documents as the *replacement*
([6.2.-Exception-handling.md#fail-an-action-from-a-middleware-while-keeping-the-original-exception](../../../docs/wiki/6.2.-Exception-handling.md)).
Grep `docs/wiki/` for the API surface your rule touches before deciding what counts as a violation - a
page that documents "how to do X deliberately" turns your draft rule from a bug-catcher into a rule that
punishes the correct usage of the library's own escape hatch. This is exactly the kind of thing that
survives a first pass of tests (rethrow-only tests all still pass) and only surfaces when someone tries
the officially documented pattern.

## Project layout

- `Pipaslot.Mediator.Analyzers/` - the analyzer itself, one file per rule (`DiagnosticDescriptors.cs`
  holds every rule's `DiagnosticDescriptor`; keep them in one file so IDs, categories, and severities are
  reviewable at a glance instead of scattered).
- `tests/Pipaslot.Mediator.Analyzers.Tests/` - one `<Rule>Tests.cs` per analyzer, plus the shared
  `CSharpAnalyzerVerifier.cs` helper (see [Testing](#testing) below).
- Rule IDs are `PIPMEDnnn`, sequential, never reused. Category is `Usage` unless a rule genuinely doesn't
  fit (e.g. a future rule about registration lifetimes might be `Reliability`) - don't invent a new
  category per rule.
- Severity is always `DiagnosticSeverity.Warning` at `isEnabledByDefault: true`. Never `Error` - this
  analyzer reaches every consumer of the main package on a minor version bump, and a build-breaking
  change on upgrade is not acceptable. Letting a consumer promote a specific rule to `Error` via their own
  `.editorconfig` is the correct way for that to happen, not a decision this repo makes for them.
- Every `DiagnosticDescriptor.messageFormat` must name a concrete alternative, not just describe the
  problem - "register an IMediatorExceptionHandler<TException> instead", not "avoid catching Exception
  here". Every `helpLinkUri` must resolve to a real wiki anchor (see [Documentation](#documentation)).

## Anatomy of a rule

Resolve every well-known symbol you need **once**, in `RegisterCompilationStartAction`, and bail out of
registering the syntax action entirely if any of them is missing:

```csharp
context.RegisterCompilationStartAction(compilationContext =>
{
    var compilation = compilationContext.Compilation;
    var middlewareInterface = compilation.GetTypeByMetadataName("Pipaslot.Mediator.Middlewares.IMediatorMiddleware");
    var exceptionType = compilation.GetTypeByMetadataName("System.Exception");
    if (middlewareInterface is null || exceptionType is null)
    {
        // Project doesn't reference Pipaslot.Mediator - nothing to analyze, and repeating the lookup
        // per-node would be wasted work on every compilation that doesn't use this library at all.
        return;
    }

    compilationContext.RegisterSyntaxNodeAction(
        nodeContext => AnalyzeCatchClause(nodeContext, middlewareInterface, exceptionType),
        SyntaxKind.CatchClause);
});
```

This is both a performance pattern (symbol lookup is not free, and a syntax node action runs per matching
node in the whole compilation) and a correctness one - a project that doesn't reference `Pipaslot.Mediator`
at all (the analyzer ships to every TFM, some consumers might reference the package for a different
reason entirely, or a partial/incomplete compilation during IDE typing might not resolve the type yet)
should produce zero diagnostics, not a crash.

Match on the **narrowest thing that's true**, not the broadest thing that's convenient. `PIPMED001` only
flags `catch (Exception)` guarding a `try` block that actually calls the `MiddlewareDelegate next`
continuation - not just any broad catch inside a class that happens to implement `IMediatorMiddleware`.
The first version didn't have that scoping and flagged unrelated `catch (Exception)` blocks doing
completely ordinary error handling elsewhere in the same method. When you resolve a delegate invocation,
compare against `MethodKind.DelegateInvoke` + `ContainingType`, not the parameter's name - a consumer can
rename `next` to anything, and `next.Invoke(context)` and `next(context)` must both match.

## Testing

The Microsoft.CodeAnalysis.Testing packages have several traps that aren't obvious from the docs. All of
these were hit building `PIPMED001` - save yourself the rediscovery:

**Package choice.** Use `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` (no suffix) with
`Microsoft.CodeAnalysis.Testing.DefaultVerifier` as the verifier type parameter. The `*.XUnit`-suffixed
packages (`Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.XUnit`, `XUnitVerifier`) are deprecated - they
throw `MissingMethodException` against modern xUnit because they were built against an old xUnit ABI. See
the [roslyn-sdk README's "Obsolete Packages" section](https://github.com/dotnet/roslyn-sdk/tree/main/src/Microsoft.CodeAnalysis.Testing#obsolete-packages)
if a test project already has the `.XUnit` package and needs migrating.

**Version alignment between the analyzer and the test project.** The testing package's own dependencies
pin a very old floor version of `Microsoft.CodeAnalysis.CSharp` (think `1.0.1`) - if your analyzer project
references a current version, you'll get `MSB3277` conflicts and then `CS1705` ("assembly uses a higher
version than referenced assembly"). Fix it by adding an explicit `PackageReference` to
`Microsoft.CodeAnalysis.CSharp.Workspaces` in the test project, pinned to the **same version** the
analyzer project uses for `Microsoft.CodeAnalysis.CSharp`. Check
`Pipaslot.Mediator.Analyzers.csproj` for the current pin and match it exactly - `CSharpAnalyzerVerifier.cs`
and `Pipaslot.Mediator.Analyzers.Tests.csproj` document the reasoning inline as well.

**Corelib version mismatch when referencing `Pipaslot.Mediator` from the test project.** The test project
targets `net10.0` (via `tests/Directory.Build.props`), so a plain `ProjectReference` to
`Pipaslot.Mediator.csproj` picks its `net10.0` build, which references a `System.Runtime` version newer
than whatever `ReferenceAssemblies.Net.NetXX` the analyzer test source compiles against - another
`CS1705`. Pin the reference to a TFM that has a matching `ReferenceAssemblies` set:

```xml
<ProjectReference Include="..\..\Pipaslot.Mediator\Pipaslot.Mediator.csproj" SetTargetFramework="TargetFramework=net8.0"/>
```

and set `TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80` (or whichever TFM you pinned to)
in the verifier. This is purely a compile-time metadata concern - the test process still runs on net10.0.

**Markup syntax for diagnostic locations.** `[|expr|]` (anonymous) auto-registers as an expected
diagnostic location for whichever `DiagnosticResult` in your `expected` array has no location set - if you
*also* add a `DiagnosticResult` explicitly, you get a count mismatch (each anonymous span counts as an
extra implicit expectation). Once you need `.WithArguments(...)` on the expected result (which is nearly
always, since every message here interpolates the containing type name), use **named** markup instead:
`{|#0:expr|}` in the source, `.WithLocation(0)` on the `DiagnosticResult`. See
`CatchAllMiddlewareAnalyzerTests.cs` for working examples of both a positive case (named markup +
`.WithLocation(0).WithArguments(...)`) and a negative case (no markup, `VerifyAnalyzerAsync(source)` with
no expected diagnostics at all).

**Negative tests carry the real signal.** Aim for at least as many "must not report" tests as "must
report" ones - a false positive is more expensive than a missing rule (see above), so the negative cases
are what actually earn a rule its keep. For `PIPMED001` that means: rethrow, narrower exception type, class that
doesn't implement the middleware interface, catch that doesn't guard `next`, and the `AddException`
escape hatch - each one is a distinct reason the rule could false-positive, and each got its own test
rather than being folded into one big scenario. Follow the repo-wide test conventions from the root
`CLAUDE.md` (`Method_Condition_ExpectedOutcome` naming, blank line between arrange/act/assert, a class
doc comment stating what the rule does and does not flag) - these test classes are ordinary xUnit test
classes and follow the same rules as the rest of the test suite.

## Wiring a new rule into the shipped package

Two things need to exist for a rule to actually reach a consumer, beyond the analyzer class compiling:

**Dogfooding.** `Pipaslot.Mediator.csproj` references the analyzer project as an `Analyzer`-only item so
the library's own code is checked by its own rules on every build:

```xml
<ProjectReference Include="..\Pipaslot.Mediator.Analyzers\Pipaslot.Mediator.Analyzers.csproj"
                   OutputItemType="Analyzer"
                   ReferenceOutputAssembly="false"/>
```

If a new rule fires anywhere inside `Pipaslot.Mediator` or `Pipaslot.Mediator.Http` itself, that's a
strong signal to double-check the rule's scoping before assuming the library's own code is wrong.

**Packaging.** The analyzer assembly has to land at `analyzers/dotnet/cs/` inside the `.nupkg` - a plain
`PackageReference`/`ProjectReference` does not do this by default. `Pipaslot.Mediator.csproj` has a
`BeforeTargets="_GetPackageFiles"` target that copies the built `netstandard2.0` DLL into the pack
manifest with `PackagePath="analyzers/dotnet/cs"`. You shouldn't need to touch this for a new rule (it
packages the whole `Pipaslot.Mediator.Analyzers.dll`, not per-rule), but if a new rule isn't reaching a
consumer, this is the first place to check - confirm with:

```bash
dotnet pack Pipaslot.Mediator/Pipaslot.Mediator.csproj -c Debug -p:Version=0.0.1-local
unzip -l Pipaslot.Mediator/bin/Debug/Pipaslot.Mediator.*.nupkg | grep analyzers
```

**Release tracking files.** `AnalyzerReleases.Shipped.md`/`AnalyzerReleases.Unshipped.md` are required by
`EnforceExtendedAnalyzerRules` (RS2008/RS2007) - every `DiagnosticDescriptor` must appear in one of them
with its ID, category, and severity, or the build warns. Add new rules to the `### New Rules` table under
`AnalyzerReleases.Unshipped.md`. The table header is whitespace-sensitive - copy the exact three-column
header (`Rule ID | Category | Severity | Notes` / `--------|----------|----------|-------`, no extra
padding spaces) rather than reformatting it for alignment, or the parser rejects it with RS2007.

## Verification checklist

Run all of these before considering a rule done - each one catches a different failure mode, and skipping
one is how a rule that "works" in isolation turns out to break something else:

1. `dotnet build Pipaslot.Mediator.Analyzers/Pipaslot.Mediator.Analyzers.csproj` - the rule compiles clean
   (0 warnings; RS2007/RS2008 point at the release-tracking files above).
2. `dotnet test tests/Pipaslot.Mediator.Analyzers.Tests/...` - the rule's own tests pass, positive and
   negative.
3. `dotnet build Pipaslot.Mediator.slnx` - **the whole solution**, including `Demo/` and both existing
   test projects. This is the real false-positive check: zero noise there is the bar every rule has to
   clear, and it's the only way to catch a rule that fires on a pattern the library's own code (or the
   Demo app) legitimately uses.
4. `dotnet test tests/Pipaslot.Mediator.Tests/...` (and `Pipaslot.Mediator.Http.Tests` if the change
   touches anything HTTP-adjacent) - confirms the dogfooding `ProjectReference` didn't change build
   behavior for the existing suite.
5. `dotnet pack` + inspect the `.nupkg` contents (see [Packaging](#wiring-a-new-rule-into-the-shipped-package)
   above) if you touched anything under `Pipaslot.Mediator.Analyzers/` or the packaging target itself.

## Documentation

Every rule needs its explanation to live in `docs/wiki/`, not only in the `DiagnosticDescriptor.description`
string - the `HelpLinkUri` must point at a real, existing anchor. Prefer extending an existing page's
relevant section over creating a new "analyzer rules" page per rule: `PIPMED001`'s explanation lives in
the exception-handling wiki page's existing migration section, with one added paragraph naming the rule
ID and its `AddException` exemption, rather than a duplicate explanation on a separate page. Use the
[wiki-page](../wiki-page/SKILL.md) skill for the mechanics of editing `docs/wiki/` (anchor conventions,
`## See also` footers, avoiding duplication).

Add a changelog bullet under `## Unreleased` in `Release-notes-and-breaking-changes.md`, in the
`### Roslyn analyzer changes` subsection (create it if it doesn't exist yet) rather than mixed in with
unrelated API changes - one line, rule ID, what it flags, link to the wiki anchor. Follow the same
"one terse sentence, no rationale" style as the rest of that file.

## See also

- [wiki-page skill](../wiki-page/SKILL.md) - conventions for the `docs/wiki/` edit every rule needs.
