---
name: wiki-page
description: Conventions for reading and writing pages under docs/wiki/ in this repository - page structure, See also footers, glossary links, anchors, Diátaxis separation, and how to read a wiki page without pulling the whole file into context. Use whenever creating, editing, reviewing, or looking something up in a docs/wiki/ page.
---

# Editing `docs/wiki/`

`docs/wiki/` is the source of truth for the GitHub Wiki. `.github/workflows/wiki-sync.yml` mirrors the
folder verbatim (`rsync --delete`) whenever it changes on `main`, so anything not in `docs/wiki/` is
deleted from the wiki on the next sync.

## Read a section, not a page

Pages range from 3 KB to 25 KB. Reading a whole page to change one paragraph wastes context.

1. `Grep` the page for the heading or term you need, with `-n`.
2. `Read` with `offset`/`limit` around that line.
3. Read the whole page only when the edit is structural (adding a section, reordering, checking the
   footer) or when you need to verify the "no silent duplication" rule below against the full text.

`Glob docs/wiki/*.md` first if you don't know which page owns the topic — the numbered filenames are
descriptive enough to pick from without opening them.

## Find every affected page

Before deciding a change touches one page, `Grep docs/wiki/` for the type and member names the change
renamed, removed, or altered. A page nobody expected often documents the old behavior, and the pages
you thought of first are the ones already fresh in your head — not the ones most likely to be stale.

Where a change removes or replaces behavior, delete the old description rather than adding a note
beside it; the changelog is where the history lives.

Never document behavior you are unsure about. A confidently wrong page is worse than a missing one —
say what you couldn't resolve instead of guessing.

## Page structure

Apply this to every new or edited page under `docs/wiki/`:

- **Footer**: end every page (except `Home.md`, which is itself the nav index, and
  `Release-notes-and-breaking-changes.md`, a plain changelog) with a `## See also` section listing 1-3
  related pages and a short reason each is relevant. Use `## See also` as the only heading name for
  this — don't reintroduce `## Next steps` or other variants; the wiki previously had both, which is
  the inconsistency this rule replaces.
- **Glossary links**: the first time a page uses a term defined in `2.-Core-concepts-and-glossary.md`
  (Mediator, Action, Request, Message, Handler, Pipeline, Middleware, Feature, Context, Response,
  Result, Facade, ...) and the reader could plausibly land on that page directly (e.g. via search)
  without having read the glossary first, link it as `[Term](2.-Core-concepts-and-glossary.md#term-anchor)`.
- **No silent duplication**: if a concept is already explained canonically on another page (e.g.
  `Dispatch`/`Execute`/`DispatchUnhandled`/`ExecuteUnhandled` in `5.-Mediator-API.md`, or a middleware
  documented in `6.1.-Ready-to-use-middlewares.md`), link to it instead of re-explaining or re-copying
  it. This includes example/contract code: the Client-Server quickstart (`4.-`) reuses the same
  `WeatherForecastRequest`/`WeatherForecastResult` shapes as the in-process quickstart (`3.-`) and
  links back to it instead of re-pasting the code block. Duplicated explanations or code drift apart
  over time.
- **Anchors**: link to a specific heading with `<page>.md#<slug>`, where `<slug>` follows GitHub's
  heading-to-slug rule (lowercase, spaces to hyphens, punctuation stripped) — this is the convention
  already used throughout `docs/wiki/`. Grep the target page for the heading to confirm the anchor
  resolves; a slug derived from memory of a heading is a broken link often enough to be worth the check.
- **Diátaxis separation**: keep Tutorial/How-to content (step-by-step recipes) visually
  distinguishable from Reference/Explanation content within the same page (e.g. its own
  heading/subsection) rather than interleaving them under one heading. Where a page legitimately
  covers more than one Diátaxis category in sequence (e.g. `1.-Why-Pipaslot.Mediator.md` moving from
  Explanation into a Reference "Library structure" section, or `2.-Core-concepts-and-glossary.md`
  moving from Explanation into the Reference glossary), add an explicit one-line signpost sentence
  marking the transition — a heading alone is not enough for the reader to notice the category shift.
- **Historical context belongs in the changelog**: don't narrate past/removed behavior (e.g. "in
  version X this was done differently") inline in a Reference or How-to chapter. Keep the chapter
  describing only current behavior, and link to the relevant entry in
  `Release-notes-and-breaking-changes.md` for readers who want the history (a short "Historical note"
  subsection with a link is enough if it's worth flagging at all).
- **Explain non-obvious structural choices**: if a Tutorial/How-to page makes a structural choice that
  isn't self-evident (e.g. splitting a quickstart's code into "Shared" vs "Executable" projects), add
  a short "why" sentence or paragraph before the steps rather than letting the reader infer it — this
  applies even though the surrounding content is otherwise pure Tutorial/How-to.

## Navigation and diagrams

- `Home.md` is the wiki's landing/nav page — add an entry there for any new page.
- Pipeline/call-flow diagrams (component views, in-process calls, HTTP calls, nested calls, custom
  middleware ordering) live directly in `2.-Core-concepts-and-glossary.md`, `5.-Mediator-API.md`,
  `6.-Pipelines-and-Middlewares.md`, and
  `8.-HTTP-transport-and-configuration-for-Client-Server-usage.md`. Update the relevant diagram there
  if you change pipeline ordering or the client/server call flow.
- `docs/archive/version4/` and `docs/archive/version5/` are old wiki snapshots kept for historical
  reference only. Never treat them as current API documentation and never edit them to match a change.
