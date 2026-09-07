# CLAUDE.md body (copy target)

Copy into the project root `CLAUDE.md` and fill in the `<...>` placeholders.
Full process: [00-process.md](00-process.md). Detailed rules live in the `process/` documents.

---

```markdown
# CLAUDE.md

<One-line project description — language/framework/version and what it builds>
The project code is **entirely under `<source path>`, <N> files.** Everything else is third-party.

## Process

Work sequence and conventions live under `process/`. **Read [process/00-process.md](process/00-process.md) before starting work.**

| Doc | When to read it |
|---|---|
| [process/00-process.md](process/00-process.md) | The whole session process. Intake → scope → investigate → implement → verify → record |
| [process/02-git.md](process/02-git.md) | Branch, commit, push, stash |
| [process/03-docs.md](process/03-docs.md) | Where and in what format to write docs |
| [process/06-model-and-prompting.md](process/06-model-and-prompting.md) | Model selection criteria |

## Documentation

| Doc | When to read it |
|---|---|
| `docs/README.md` | Folder map, boot sequence, "where to look" table |
| `docs/architecture.md` | Init order, layer structure, shared patterns |
| `<domain-specific docs>` | <when to read it> |
| `advise/README.md` | Known issues and improvement proposals. **Check before any fix work** |

**`docs/` = how it works now. `advise/` = how it should change.** Don't duplicate between them.

## When writing code

<Conventions that repeat in this project. State the reason, not just the prohibition.>

- **<convention>** — <why>. <what happens if violated>

## Traps you must know

**This is the most important section in this document.** List only failures that don't surface immediately as compile errors.
Don't list mistakes that fail loudly — those get found on their own.

- **<a registration point outside the code>** — config file, editor setting, build list. Missing it fails silently at runtime
- **<code that doesn't compile in the dev environment>** — platform-conditional blocks. Only a real target build verifies it
- **<something depending on path/name conventions>** — e.g. filename must match an identifier
- **<currently unresolved P0 issue>** — link it here if there is one

## Verification steps (project-specific commands)

```bash
# Step 2 — compile · static check
<command>

# Step 3 — real target build
<command>
```

**What step 2 cannot verify:** <state it explicitly>
**What only a human can judge:** <list>

> <If the environment version must match the project exactly, warn here.
> Some tools cause irreversible changes when opened with a mismatched version.>
```

---

## What stays in the body vs. what can be split out

| Item | Where |
|---|---|
| Project-specific traps, coding conventions, verification commands | **Directly in `CLAUDE.md` body** — auto-read every session |
| Session process, git rules, doc format, model rules | Split into `process/` and link — survives a project change |
| Work-spec templates | `process/04-templates.md` — open only when needed |

A long `CLAUDE.md` costs every session. **Keep it as the document with the highest density of project-specific information.**
