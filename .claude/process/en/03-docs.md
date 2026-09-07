# Documentation System

Documentation splits into **two axes.** They serve different purposes — merging them ruins both.

| Axis | Folder | What it holds | Lifespan |
|---|---|---|---|
| **Codebase docs** | `docs/` · `advise/` | How this code works now / how it should change | Updated continuously alongside the code |
| **Work-spec docs** | `WorkFlow/<feature>/` | When, why, and how this piece of work was done | Written at the time, referenced afterward |

`docs/architecture.md` is "the structure now"; `WorkFlow/.../작업내역_*.md` is "what happened that day." They don't replace each other.

## Shared principles

- **Docs live in the repo as markdown.** Don't publish spec/investigation docs as Artifacts.
  If the team already uses in-repo doc folders, an Artifact link sits outside that flow and doesn't persist in the repo.
- **A new doc follows the existing format** (filename, header structure) of its folder. Check before writing.
- **New docs stay untracked;** whether to commit is the user's call (same reasoning as ticket-scope rules).
- **One source of truth + everything else is a delta.** Don't write the same information in two docs.
  When multiple docs cover the same topic, state at the top of each which doc/commit is authoritative.

---

## 1. Codebase docs — `docs/` · `advise/`

**`docs/` = how it works now. `advise/` = how it should change.** Don't duplicate between them.
Once a proposal is applied and becomes "how it works now," move it into `docs/` and leave only a resolved-marker in `advise/`.

### `docs/`

| File | Content |
|---|---|
| `README.md` | Folder map, boot sequence, **"where to look" table** |
| `architecture.md` | Init order, layer structure, shared patterns |
| `CHANGELOG.md` | Chronological change history |
| `<domain>.md` | Domain-specific detail |
| `reference/` | External-source analysis. **Kept regardless of whether it was applied** |

`reference/` holds **grounds for judgment**, not proposals. Doesn't count toward the `advise/` 10-doc limit.

The core of `docs/README.md` is the **"where to look" table** — mapping task type directly to doc. With it, you don't re-scan the codebase every session.

### `advise/`

- Filename `NNN-topic.md` — 3-digit serial number, **never reused**
- Each item follows **symptom → evidence (`file:line`) → impact → recommendation**
- **No item without evidence**
- Resolved items aren't deleted — mark with `~~strikethrough~~` + `✅ resolved (commit hash)` — keeps the judgment trail
- Always update the listing table in `README.md` alongside

| Grade | Meaning |
|---|---|
| **P0** | Build/run is broken right now |
| **P1** | Works, but produces wrong results under some conditions |
| **P2** | Fine now, but cost explodes as scale grows |

Handle P0 first. Layering a refactor on top of a broken build makes root-cause isolation impossible.

**10-doc rule** — reorganize the whole folder once it passes 10 docs (count excludes `README.md`).
Keep the current count (`N / 10`) in `README.md` and update it every time. On reorganizing:

- Move resolved docs to `advise/archive/` — keeps history, clears the active list
- Unify classification into either "by layer" or "by priority"
- Promote content that became "how it works now" into `docs/`
- Keep a single entry point — `README.md` — no matter how many docs there are

### CHANGELOG format

Chronological, newest on top. One entry doesn't have to map to one commit — group by **unit of judgment.**

```markdown
## YYYY-MM-DD — <what was done> (advise/NNN-N)

<why it was done. what symptom existed.>

### Scope of change
| Target | Change |

### Where the judgment call was made
**<chose option A.>** <why the alternatives don't work.>

### What was left out
<what was considered but not added, and why. saves the next person from repeating the same review.>

### Verification
| Step | Result |
| <visual-check item> | ❌ Not verified — <why not> |
```

**Don't drop unverified items from the table.** Dropping them reads as "confirmed" to the next person.

---

## 2. Work-spec docs — `WorkFlow/`

Create a folder per feature (content), then split by document nature underneath.

```
WorkFlow/
└── <feature>/
    ├── 명세서/                        # source of truth: implementation spec, migration spec
    │   ├── <feature>_구현명세서.md
    │   ├── 시스템/시스템N_<topic>.md   # large features split by system
    │   └── 서브/서브N_<topic>.md
    ├── YYYY-MM-DD/                    # date-keyed session logs
    │   ├── 작업내역_<feature>_<summary>.md
    │   └── 남은작업_<feature>_순차.md
    └── Change_log/
        └── <project>/YYYY-MM-DD_<topic>_Change_log.md
```

The 6 doc templates: [04-templates.md](04-templates.md).

### Common header

Put metadata in a blockquote at the top. Pick fields per doc type.

```markdown
# <feature> — <doc nature> (<date or subtitle>)

> Written: YYYY-MM-DD / Author: <name> (or session/worktree name)
> Branch: <working branch> / Base: <base commit SHA or point in time>
> Source: <original branch tip SHA being ported/referenced>   ← for migration/porting docs
> Design: <design-doc link>
> Precedes: <preceding commit/doc>                              ← when there's a dependency
> Nature: <one line on this doc's role>
```

### Notation conventions

- **Status emoji**: ✅ done · ⚠️ partial/caution · ❌ failed/not started. Used in status columns.
- **Trap markers**: sections with recurring mistakes get `★trap` in the heading.
- **Resolved items aren't deleted** — `~~strikethrough~~` + `(resolved, date)`.
- **Warnings** get a ⚠️ prefix inline.
- **Defects/issues in a numbered table**: `| # | file | defect | fix |` — later docs can reference `§2-2 #15`.
- Code symbols, paths, SHAs wrapped in backticks.

### Change log

- Save location follows the project's rule (e.g. `C:\Git\Change_log\{ProjectName}\`).
- **Record not just the change but the intent behind the user's explanation.** The change alone loses the judgment basis later.

### Recording a policy decision

When recording a confirmed policy decision from design/owner, include **all** of:

- The decision (what will/won't be done)
- **Intentional exclusion** — why it was excluded, which path/user is affected
- The boundary of that exclusion — so a later bug report can be cross-checked against this decision

**Don't mistake a confirmed policy decision for a bug.** A "why wasn't this done?" spot may be intentional.

---

## 3. `HandOff.md` — handoff

The doc for picking up work after a session breaks off. Lives in the project root.

**Don't write what reading the code would tell you.** Write only what's not left anywhere else —
why it was done that way, what was tried and dropped, what's still unverified.

```markdown
# HandOff

Last updated: <YYYY-MM-DD> / <remote sync state>

## Current state
**Working tree <clean/dirty>, <N> unpushed commits, <a/no> process running.**

### Reached so far
| Item | Status |          # ✅ resolved + how it was verified / ⚠️ code done, verification incomplete

## Next up
**Look at this first** — <usually verification the last session didn't finish>
### Open items
| Item | Doc | Note |   # why it's not done yet, what you need to know before touching it

## Environment info needed to resume
### Tool versions        # required version + actual install path + why not to open with a different version
### Build · check         # commands + output-path caveats + rule for cleaning up temp scripts
### Runtime log           # path + which tags tell you what
### Debug controls        # keys/commands. note if mapping depends on code order

## Traps repeatedly hit in this project
<only things actually hit. no generalities.>
<a misdiagnosis history — "read symptom X as cause Y, actually Z" — is especially valuable.>

## First action on resume
git status --porcelain / git log --oneline -3 / git status -sb | head -1
If it disagrees with this doc, report the discrepancy first, then proceed.
```
