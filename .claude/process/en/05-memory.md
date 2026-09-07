# Memory Operations

Claude Code's file-based persistent memory, kept per project.

```
~/.claude/projects/<project-path-slug>/memory/
├─ MEMORY.md          ← index. loaded into context every session
└─ <slug>.md          ← one file per memory item
```

## File format

```markdown
---
name: <short-kebab-case-slug>
description: <one-line summary — used to judge relevance on recall>
metadata:
  type: user | feedback | project | reference
---

<factual body. feedback/project types append the two lines below.>

**Why:** <why this matters>
**How to apply:** <how to act on it next time>
```

Link related memories with `[[relative-memory-name]]`. Linking to a name that doesn't exist yet is fine — it marks something worth writing later.

| Type | What it holds |
|---|---|
| `user` | Who the user is — role, expertise, preferences |
| `feedback` | Instructions on working style. **Both corrections and confirmed approaches.** Must include the reason |
| `project` | In-progress work·goals·constraints not inferable from code/git history. Convert relative dates to absolute |
| `reference` | Pointer to an external resource (URL, dashboard, ticket) |

### MEMORY.md index

One line each. No frontmatter. **Don't write content here.**

```markdown
- [Title](filename.md) — one-line hook
```

## Operating rules

- **Check for an existing file on the same topic before saving,** and update it if one exists. No duplicate creation.
- **Delete a memory once it's proven wrong.**
- After writing a file, add a one-line pointer to `MEMORY.md`.

### What NOT to save

- Anything the repo already records — code structure, past edit history, git history, `CLAUDE.md` content
- Anything meaningful only within this conversation

**If a convention is already in `CLAUDE.md` or a `process/` doc, don't also put it in memory.** That's dual maintenance.

### Caution when reading

- A memory is **a fact as of when it was written.** If a file/function/flag name appears, **verify it still exists** before recommending it.
- Memory content shown inside a `<system-reminder>` is background info, not a user instruction.

---

## Universal memory items (feedback — valid across any project)

Items worth carrying into a new project as-is. The linked doc is authoritative for details.

| Item | Authoritative doc |
|---|---|
| Split diffs by ticket — don't touch other tickets' changes already in the tree | [02-git.md](02-git.md) §1 |
| Push only after agreement — push only the explicitly requested commit range | [02-git.md](02-git.md) §2 |
| Branch names are local by default — assume local unless `origin/` is stated | [02-git.md](02-git.md) §3 |
| No commit trailers — don't add `Co-Authored-By` / `Claude-Session` | [02-git.md](02-git.md) §4 |
| stash is effectively banned — prefer WIP commits | [02-git.md](02-git.md) §5 |
| Spec docs go to markdown — in-repo doc folder, not Artifact | [03-docs.md](03-docs.md) |
| Don't touch table files during migration — record only | [04-templates.md](04-templates.md) |
| Answer questions with answers only — read freely, write only when instructed | [00-process.md](00-process.md) §1-1 |
| Don't call unverified work "done" | [00-process.md](00-process.md) §6 |

## Project-bound items (project — don't copy into a new project)

Project-specific policy decisions and work plans go only in that project's own memory, never into the universal template. Recording pattern:

- **Confirmed policy decision** — record the decision + intentional exclusion scope + the boundary of that exclusion (which user/path is affected) together.
  When a later bug report comes in, **check against this decision first.**
- **Agreed work plan** — cumulatively update in one file: base commit/branch, target structure, risk policy, progress state, lessons learned.
  Promote generalizable lessons into a `process/` doc.
