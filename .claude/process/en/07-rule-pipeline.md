# Rule-Consolidation Pipeline

**How these documents themselves get created and updated.**
If [00-process.md](00-process.md) is "how to do one piece of code work," this is "how the rules themselves accumulate."

```
1. User requests a memory update
        ↓
2. Apply to memory
        ↓
3. Verify memory
        ↓
4. First-pass consolidation · conflict resolution   ← extract project-specific → universal
        ↓
5. Add to the Claude\MD store
        ↓
6. Second-pass consolidation · update the final version  ← merge inside the store
        ↓
7. Review the final version
        ↓
8. Use in the project
```

**Don't do it all at once.** Steps 1–3 run continuously during work; 4–8 run separately once rules have accumulated.

---

## 1. User requests a memory update

Starts the moment the user gives an instruction about **working style.** Distinct from a code request.

| Signal | Example |
|---|---|
| Correction | "Don't do that", "Do it this way" |
| Confirmation | "Let's go with this approach", "Do it this way from now on" |
| Explicit request | "Remember this", "Make this a rule" |

**A correction is a request too.** Even without the user saying "remember this," an instruction about working style goes all the way to step 3.

## 2. Apply to memory

**Decide where it goes first.** The two destinations serve different purposes.

| Target | Where | Criterion |
|---|---|---|
| Valid only for this project | Project `CLAUDE.md` | Tangled with this project's structure/traps |
| Valid across any project | Memory file (`feedback` type) | Holds even when the stack changes |

**Don't write it in both.** If it's already in `CLAUDE.md`, don't also add it to memory — dual maintenance.
→ [05-memory.md](05-memory.md)

**Record the reason alongside it.** A bare rule breaks down outside its exact wording. `feedback` type requires `Why:` / `How to apply:`.

## 3. Verify memory

**Verify it right after applying it, by reading it back.** Don't stop at writing.

- Does it **duplicate** an existing item? — if so, update instead of creating new
- Does it **conflict** with an existing convention? — if so, **report to the user instead of silently overwriting**
- Did a one-liner land in the `MEMORY.md` index?

Resolve any conflict found here immediately. Dragging it to step 4 makes tracing the cause harder.

## 4. First-pass consolidation — extracting the universal from the project-specific

Once project conventions have piled up enough, **split out the universal part.**

### Extraction criteria

| Extract | Keep |
|---|---|
| Holds even when language/engine changes | Tangled with a specific API/filename/folder structure |
| Judgment procedure, reporting rules, doc system | This project's list of traps |
| Git · memory · model operations | Build commands, version paths |

Don't delete project-specific content — **replace it with a `<...>` placeholder** and quote the original example right below it.
The way to fill it in must be visible for the next project to use it.

### Conflict resolution — the core of this step

**List out where the extracted version disagrees with the existing stored docs.** Don't silently pick a side.

Recording format — **record which side won and why, not which side changed.**

```markdown
| Issue | Decision | Basis |
|---|---|---|
| <what disagreed> | <which way it was resolved> | <which doc, which section, why> |
```

**A conflict that can't be resolved goes into its own "unresolved — needs user decision" list.** Don't decide arbitrarily and move on.

## 5. Add to the `Claude\MD` store

Deposit into the store (`Desktop/Claude/MD/`). **The output at this point is still a draft** — just placed alongside existing docs.

Why not merge immediately: placing them side by side is what reveals what overlaps.

## 6. Second-pass consolidation — merging inside the store

Read the whole store and **build the final version in a new folder.** Don't edit existing files in place —
keeping the originals means you can roll back if the consolidation turns out wrong.

### Merge criteria

| Situation | Handling |
|---|---|
| Same topic, near-identical content | **Merge into one** |
| Same topic, content disagrees | **Conflict** — record the decision and basis, same format as step 4 |
| Looks like the same topic but **different role** | **Keep separate** + have the parent doc distinguish when to use which |
| One file mixes several topics | **Decompose by topic** into separate docs |

The third case is the easy mistake. Merging because names look similar makes both unusable.

### Output structure

- **A single entry-point `README.md`** — what's inside, reading order, how to apply, **decisions made while consolidating**, **unresolved items**
- Process docs numbered in order — the reading order is the work order
- Stack-specific rules **split into an appendix** — deletable wholesale for a different stack

## 7. Review the final version

Check before dropping it into a project.

- **Is every original item present somewhere?** — cross-check against the originals for anything silently dropped
- **Do cross-doc links point to real files?**
- **Is the same content in two docs?** — one source of truth + links elsewhere
- **Does the decision table in `README.md` match the actual body?**
- **Are unresolved items still listed in `README.md`?** — don't delete them as if resolved

## 8. Use in the project

1. Copy the final version into the project's `process/`.
2. **Link `process/00-process.md`** from the top of the project's `CLAUDE.md`. Unlinked, it won't get read.
3. Delete the appendix if the stack differs.
4. **Check for conflicts with the project's existing conventions.** If work already done (commits, etc.) followed the old rule, **ask the user whether to apply retroactively.**

### What stays in the project's `CLAUDE.md`

Don't duplicate what's been split into `process/`. `CLAUDE.md` keeps **only what's true solely for this project.**

- Project intro and code size
- Coding conventions (project-specific)
- **Traps you must know** — the most important part
- Verification commands and what each step can't cover

A long `CLAUDE.md` costs every session. **Keep it as the document with the highest density of project-specific information.**

---

## Maintaining a bilingual version

If the same doc set is kept in two languages (e.g. `process/en/` for agent use, `process/ko/` for user review):

- **Update both together.** A change to one that doesn't land in the other creates silent drift — the agent and the user end up reading different rules.
- **Keep filenames and section numbers identical** across languages so a diff shows what changed at a glance.
- When a step in this pipeline updates a doc, the last sub-step is: **apply the same change to the other language's file.**

---

## What this pipeline prevents

| Step | Skipping it causes |
|---|---|
| 3. Verify memory | The same rule piles up worded differently, several times over |
| 4. Conflict resolution | Nobody knows which side won, and the next decision goes the other way |
| 5. Store deposit | The convention vanishes with the project when it ends |
| 6. Role-separation judgment | Docs with different natures get merged, making both unusable |
| 7. Cross-check against originals | Rules quietly disappear during consolidation |
| 8. Conflict check | Existing output that disagrees with the new convention stays as-is |
