# Session Process

The sequence for taking a task from receipt to done. **Each step starts only after the previous one finishes.**
Details for each step live in the linked documents.

```
0. Resume check  →  1. Intake  →  2. Scope  →  3. Investigate
                                                      ↓
9. Handoff  ←  8. Record  ←  7. Report  ←  6. Verify  ←  4~5. Implement
```

---

## 0. Resume — when a session picks back up after a break

**Read actual state, not memory.** Side effects already executed do not roll back when a response gets cut off.
The most dangerous move on resume is re-running something because "it probably didn't finish."

```bash
git status --porcelain      # what's left in the working tree
git log --oneline -3        # did the commit actually happen
git status -sb | head -1    # was it pushed (ahead/behind)
```

If a file was mid-edit, **read it first to confirm the intended content already landed** before editing again.

| Action | Re-run safety | On resume |
|---|---|---|
| Read file · grep · diff | Safe | Just do it again |
| Write file · Edit | **Caution** | Read first, confirm it's not already applied |
| `git commit` | **Risky** | Check `git log`. If done, amend; if not, commit |
| `git push` | **Risky** | Check the ahead count in `git status -sb` |
| Subagent | **Risky** | Keeps running in the background. Don't relaunch — wait for the notification |

**Report what's done and what isn't before continuing.** Don't hide the interruption and quietly retry.
**An approval given inside a truncated response is not an approval.** Confirm it again.

If a `HandOff.md` exists, read it too. If it disagrees with actual state, report the discrepancy first.

---

## 1. Intake — what was actually asked

### 1-1. Question or instruction

| Form | Example | Action |
|---|---|---|
| Question | "Can you...?", "Is there a way to...?", "Why does this...?" | **Answer only.** Explain the approach and stop |
| Instruction | "Do it", "Let's do this", "Go ahead", "Add it" | Execute |
| Ambiguous | "Wouldn't it be better to fix this?" | Answer, then **end with "should I?"** Don't fix it first |

**Investigation is not extra work.** Reading files, grepping, digging through logs to answer accurately is part of answering.
Better than answering without evidence. The boundary is **read vs. write** — read freely, write only when instructed.

If answering a question turns into "I'd need to write code to really answer this," write the approach instead of code and **ask "want me to write it?"**

### 1-2. Right model for the job

Check the task against the criteria in the model-selection rules. If it's mismatched, request a switch and wait
**before starting the work.** Not every turn — only when the task changes. → [06-model-and-prompting.md](06-model-and-prompting.md)

### 1-3. If the instruction is ambiguous, ask and wait

- Ambiguity where **the outcome would materially differ** depending on interpretation
- Missing essential context: target file unclear, where it attaches to existing code unclear, success criteria unclear

Below this bar, don't ask. Pick the reasonable option for variable names, defaults, choosing between equivalent approaches,
and state it in one line. Batch questions into a single round, and disclose any assumptions filled in by guessing.

---

## 2. Scope — how far to reach

**Respect ticket (work-unit) scope.** → [02-git.md](02-git.md) §1

1. `git status` to **check existing changes.** Another session's/task's changes may already be present.
2. If my edits and someone else's land in the same file, **report that explicitly.**
3. Don't touch code outside the requested scope even if it looks improvable. **Report, don't fix.**

**Cleanup/init tasks only add and tidy.** Deletion happens only when explicitly requested.

---

## 3. Investigate — docs before code

1. Check project docs first (`docs/`, `advise/`, spec docs). Faster than re-reading the codebase.
2. **Don't mistake a confirmed policy decision for a bug.** A spot that looks like "why wasn't this done?" may be
   intentional. Check memory/specs first. → [05-memory.md](05-memory.md)
3. Then read the code.

---

## 4. Before implementing — look for what already exists

**Before adding code, check whether code with the same behavior already exists.**
If it does, **ask the user which direction to take** — add a new method, or reuse the existing one.

Skipping this check and writing near-duplicate code is the most common waste.

---

## 5. Implement

Project-specific coding conventions live in `CLAUDE.md`'s "When writing code" section. → [01-rules.md](01-rules.md)

---

## 6. Verify — don't just say "fixed" and stop

**State whether each step was actually done.**

| # | Step | When |
|---|---|---|
| 1 | `git diff` static review | **Always.** Confirm only the intended files changed, no side changes |
| 2 | Compile · static check | Always, after touching code. **State what this step cannot cover** |
| 3 | Real target build | When step 2 doesn't compile the touched code (platform-conditional blocks, etc.) |
| 4 | Runtime check | Things only a human can judge. **List what to look at** |
| 5 | Show the change in-session | File-by-file diff and the reason for each change. **Only publish an Artifact when asked** |

### The principle to keep

**Don't claim a verification you didn't run.** If only step 1 was done, say "static review only, compile not checked."
In a project where failures surface at runtime, **an unfounded completion report is the most expensive mistake.**

---

## 7. Report

- Conclusion first, then evidence.
- **State what wasn't done first** — unverified items, out-of-scope things left untouched, anything filled in by guessing.
- Problems found outside scope go here instead of being fixed.

---

## 8. Record

### Commit

Split by ticket. **Don't add commit trailers.** Push only on explicit request.
→ [02-git.md](02-git.md)

### Docs

- Spec/investigation docs go **in the repo as markdown.** No Artifact publishing.
- New docs follow the **existing format** (filename, header structure) of the same folder.
- New docs stay **untracked**; whether to commit is the user's call.
- A change log records not just the change but **the intent behind the user's request.**

→ [03-docs.md](03-docs.md), [04-templates.md](04-templates.md)

### Memory

Save when given an instruction about working style. Don't save what the repo already records.
→ [05-memory.md](05-memory.md)

---

## 9. Handoff

Update `HandOff.md` when ending a session. **Don't write what reading the code would tell you.**
Write only what's not left anywhere else — why it was done that way, what was tried and dropped, what's still unverified.

Format: [03-docs.md](03-docs.md) §3.
