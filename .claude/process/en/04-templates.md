# Work-Spec Document Templates

6 templates for docs under `WorkFlow/<feature>/`. Folder structure, headers, and notation: [03-docs.md](03-docs.md) §2.

| Template | When to use | File location |
|---|---|---|
| [A. Implementation spec](#a-implementation-spec) | Source-of-truth doc for one feature | `명세서/<feature>_구현명세서.md` |
| [B. Migration spec](#b-migrationporting-spec) | One instance of porting a feature between branches/versions | `명세서/<feature>_이관명세서.md` |
| [C. Rollup doc](#c-consolidated-rollup-doc) | Status board across multiple similar-nature tasks | `명세서/<work-group>_통합명세서.md` |
| [D. Session log](#d-session-log) | A day's session record | `YYYY-MM-DD/작업내역_*.md` |
| [E. Change log](#e-change-log) | Record of a batch of QA-ticket fixes | `Change_log/YYYY-MM-DD_*.md` |
| [F. Remaining work](#f-remaining-work) | Leftover-work summary at a stop/handoff point | `YYYY-MM-DD/남은작업_*.md` |

---

## A. Implementation spec

The **source-of-truth** doc holding a feature's full structure. Splits into an index doc + per-system docs when it grows.

```markdown
# <feature> — Implementation Spec (index · overview · data)

## Document set          # list of split docs and what each covers
## Update history        # YYYY-MM-DD — one line per update, cumulative
## Notation baseline      # symbol/terminology defs used across this doc family

## 1. Overview
### 1.1 System definition
### 1.2 Entry conditions
### 1.3 Screen composition

## 2. Data / table spec
### 2.N <table name> — columns, meaning, references (★ for core tables)
### 2.end referenced external tables

## 3. Code structure map    # class/file ↔ role mapping, local-storage key list

## 4. Shared conventions    # what to follow when working on this feature. ★trap for traps
### 4.N <convention topic>  # data pipeline, UI rules, asset-editing caution, module boundaries

## 5. Unresolved items      # cross-system open problems / decisions pending
```

## B. Migration/porting spec

Record of one instance of porting a feature between branches/versions.

```markdown
# <feature> — <source> → <target> Migration Spec

> Written: YYYY-MM-DD / <session·worktree name>
> Source: `<original branch>` tip `<SHA>`
> Design: <link> / Precedes: <preceding commit·precedent doc>

## 1. Migration history      # | time | commit | content | — cumulative across 1st pass/rollback/re-migration
## 2. What changed this time
### 2-1. Alignment with prior work
### 2-2. Defect fixes         # | # | file | defect | fix | — exhaustive, by compile/verification
### 2-3. Verification result  # what was audited, how, and the result

## 3. Data status             # record only — table-file no-touch principle
                             # divided into: brought over / caution (needs design confirmation) / absent
## 4. Remaining checks (editor/QA)  # numbered list — things to confirm outside code
## 5. Related docs           # source-of-truth commit, rollup doc, methodology precedent
## 6. Post-migration <target> work  # additional work fitting the target environment after porting
```

## C. Consolidated rollup doc

Tracks several similar-nature tasks (a migration series, etc.) in one place. **Each individual doc stays authoritative.**

```markdown
# <task-group name> — Consolidated Spec

> Nature: cumulative-item rollup. Each folder's doc is the source of truth; this doc
> collects only **what was brought over when and how, and what's left.**

## 0. Shared conventions
### 0-1. Methodology        # why this approach (e.g. why cherry-pick doesn't work + exception precedent)
### 0-2. Commit message format  # Follow/List/Change/Check → 02-git.md §4
### 0-3. Shared risks       # structural risks to check for every item

## 1. Item status           # | # | item | representative commit | status | link to detailed spec |
## 2. Per-item notes        # item-specific quirks/confirmed conventions that don't fit the table
## 3. Actual incidents — regression-prevention checklist
                          # per rolled-back case: what / why / how to prevent recurrence
```

## D. Session log

`YYYY-MM-DD/작업내역_<feature>_<summary>.md`. Split into multiple files for the same day, cross-link them.

```markdown
# <feature> — Session Log (YYYY-MM-DD, <topic>)

<one paragraph: design-doc grounds (down to §number) + summary of what was done and how>
Target: `<key files>`

## Requirements          # requirement bullets for this task (quoted from design-doc clauses)
## 1..N. <work done>     # observed state → judgment → implementation
                        # record "no change needed" verdicts with their basis too

## Files changed/added    # path — one line per file on what was done
## Follow-up / checks     # recommended visual checks, what to hand off to the next session
```

## E. Change log

Record of handling several QA tickets in one pass. Per ticket: **intent / cause / change** in three parts.

```markdown
# <feature> — Change Log

- **Project**: <project name>
- **Work date**: YYYY-MM-DD
- **Target content**: <content identifier>
- **Author**: <name>

## N. <ticket number> — <symptom, one line>
**Intent/purpose**: what problem is being solved and why
**Cause**: the code-level root cause (down to the condition/formula/init timing)
**Change**
- `<file>.<method>` — what changed and how
```

> **Never leave the intent/purpose field blank.** Infer and write the goal behind the user's explanation.
> Recording only the change loses the judgment basis later.

## F. Remaining work

At a stop/handoff point, present leftover work **grouped by dependency** to suggest a start order.

```markdown
# <feature> — Remaining Work (sequenced)

> Compiled: YYYY-MM-DD / Author: <name>
> Branch: <branch> / Base: <point in time>. <one-line overall status>
> Spec: <path to the source-of-truth doc>

Grouped in start-able order. **A → B → C** recommended progression.

## A. Standalone (no dependency — start immediately)
## B. Needs <other role> first, then wrap up   # e.g. designer assets first
## C. Depends on <external team>               # e.g. server-team dependency

Each item: number. **Title** — status · prerequisite · completion criteria.
Resolved items stay listed with ~~strikethrough~~ + (resolved, date).
```

---

## Migration rules

For porting a feature from one branch/version to another.

**Migration scope is code and assets.** Data table files (CSV, xlsm, `Resources/Tables`-family) stay untouched during migration.

- If a table file's overlap/conflict/mismatch is found, **record it, don't fix it** (in the commit's Check section, spec §3).
- Table-data alignment (value corrections, row imports, overlap resolution) happens in a separate later phase.
- **Boundary case** — when code disagrees with a table value, **aligning the code side is in scope** (e.g. aligning an enum to an already-imported table value). Editing the table file itself is out of scope.

**Why:** tables belong to the data team/a later phase; mixing them into a migration commit erodes the boundary between code/asset migration and data import responsibility.

Format for recording unresolved findings: **where it was found** (file/sheet/key) → **what disagrees** (compare both values) → **why it's not fixed now.**

Same spirit as [02-git.md](02-git.md) §1 "split diffs by ticket" — a migration commit carries only migration-scope changes; out-of-scope findings get reported, not fixed.

---

## Appendix. Recurring operating principles

1. **One source of truth + everything else is delta** — don't write the same information in two docs. Rollups get only links and status.
2. **Record "what wasn't done" too** — don't skip unverified items (Check), intentional exclusions, design-confirmation-pending items.
3. **Defects/verification go in exhaustive tables** — the audit method (what was checked how) must be written for it to be reproducible/trustworthy.
4. **Promote rolled-back cases to a checklist** — generalize an actual incident into a regression-prevention item folded into the shared conventions.
5. **Group remaining work by dependency** — grouping by role/team (A/B/C) naturally determines the start order.
6. **Record the basis for a verdict** — even a "no change needed" conclusion needs its check path and evidence recorded.
