# Model Selection and Prompting

## 1. Model selection rules

Pick a model with a slash command to run a task under it. **Without a command, the default model applies.**

| Model | Where used |
|---|---|
| **Default** | Core areas — where failure is expensive. Structural changes, init order, platform-specific code |
| **Top-tier** | Problems the default model has failed on twice or more. Full architectural redesigns. Long autonomous work spanning multiple minutes per turn |
| **Low-cost** | Typos, comments, doc updates, mechanical renames, single-function edits |

### Signals to escalate

Areas where failures show up **as silent runtime failures, not compile errors.** Don't start with a low-cost model.

- Work touching a registration point outside the code (config files, editor settings, build lists)
- Blocks not verified in the dev environment (platform-conditional compilation)
- Changes entangled with init/teardown order
- **A change touching 3+ files at once**

### Signals to de-escalate

- The fix point is already pinpointed and stays within one file
- Only docs are being changed
- A failure would surface immediately as a compile error anyway

### Check before starting

Match the task against the criteria above **in the first response.** Not every turn — only when the task changes.

**Both directions stop and ask.** Ask before starting, not after — announcing it after the fact means the expensive model already read the whole thing.

- **Using a model that's overkill** → **request a switch and wait.**
  One round-trip is cheaper than running the whole task on the expensive model. If the answer is "keep going," proceed with the current model.
  > This task is a doc update only. `<low-cost command> <task>` recommended (cost <N>%).
  > Switch? Reply "keep going" to proceed with <current model>.

- **Using a model that's underpowered** → **stop and confirm.** Failure in this direction won't surface immediately.
  > This task touches <N registration points>. Missing one fails silently at runtime, not as a compile error.
  > `<higher-tier command>` recommended — proceed anyway?

- **After failing the same way twice** → suggest the top-tier model before a third attempt. Don't retry on the same model.

Always include the exact command so the user can copy-paste it.
**Ask only once per task.** Once the answer is "keep going," don't ask again until that task is done.

### Don't offload to a subagent instead

Launching a subagent on a different model breaks the conversation's context (handoff, verification steps, prior judgment calls).
A mode that inherits context ignores the model override; a mode that lets you pick a model starts from a blank slate.
**In a project where failures are silent, a context break is itself a risk. Model switching is the user's call, via slash command.**

### Slash command definition

`~/.claude/commands/<name>.md`:

```yaml
---
description: Run the task on <model name>
model: <model ID>
argument-hint: [task]
---

$ARGUMENTS
```

The `model:` frontmatter applies **only to that turn.** The next turn reverts to the session default.
To keep multiple turns on a model, change the session default with `/model`.

---

## 2. Prompting principles

Apply to every model. If model-specific docs exist, they add only **what deviates from this baseline.**

### 2-1. Give the reason, not just the prohibition

Explaining the reasoning behind a constraint lets the model generalize. A bare rule breaks down outside its exact wording.

- ❌ `Never call <function> directly`
- ✅ `Asset loads go through <wrapper>. Caching and refcounting live there — a direct call causes the same asset to load twice`

### 2-2. State what to do, not what not to do

- ❌ `Don't be verbose` → ✅ `Lead with the conclusion, one sentence. Evidence after`

### 2-3. Say it explicitly if you want it to go beyond the default

- ❌ `Build a save system`
- ✅ `Build a save system — production-ready, including error handling, migration, and corrupted-file recovery`

### 2-4. Give room to say "I don't know"

> If there isn't enough basis to judge, say so instead of guessing.

Reduces hallucination. Especially important in a project where failures don't surface immediately.

### 2-5. Start with one example

The most reliable way to lock in format/tone/structure. Add more only if output still doesn't match. 3–5 is the ceiling.
**Never include an example containing the pattern you want avoided** — the model imitates examples.

### 2-6. What matters less now

Less effective on newer models. Don't reach for these out of habit.

| Technique | Current status |
|---|---|
| XML-tag structuring | Only for very complex prompts. Headers and line breaks usually suffice |
| Role assignment ("You are the world's best...") | Usually unnecessary. Over-specifying can hurt performance |
| "Think step by step" | Superseded by thinking, on by default in newer models |
| Response prefill | Causes API errors on newer models |

### 2-7. Give the full spec on the first turn

Leaking requirements across multiple turns hurts both token efficiency and output quality.

```
Goal: <what should exist when this is done>
Deliverable: <files/code/docs — specifically>
Constraints: <what must not be touched, performance/dependency conditions>
Background: <what problem in the existing code this addresses>
```

Delete fields with nothing to fill — don't leave them blank.

### 2-8. Don't call unverified work "done"

State, per project, what's unverifiable within a session. When something's touched, say
**"fixed, but X still needs a real build to confirm"** — not just "fixed."

### 2-9. Effort level can't be changed by a prompt

`effort` is a session setting (`/config`), not something a doc or prompt raises. A sentence like "reason deeply"
doesn't change behavior. **If reasoning feels shallow, don't rewrite the prompt — raise effort instead.**

---

## 3. When a tool connection drops

If an external tool connection (MCP, etc.) drops mid-task, **try reconnecting before falling back to an alternative approach.**

- Retry policy: up to **5 attempts,** total budget **1 minute**
- If it still fails past the limit, **tell the user and ask how to proceed**

Switching to a different approach on your own changes the outcome without that fact being visible.
