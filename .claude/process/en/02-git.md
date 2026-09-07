# Git Workflow

Assumes multiple sessions/people may edit the same working tree, or the same repo, concurrently.

## 1. Split diffs by ticket

- Given one ticket (work unit), touch **only files/hunks within that scope.**
- If another ticket's changes are already present in the working tree, **read them but don't touch them.**
- Commit by ticket too.

**Why:** it has actually happened that another session's ticket code was already sitting in the same file.

**How to apply**
1. Check `git status` before starting to see existing changes.
2. If my edits and someone else's land in the same file, report that explicitly.
3. Don't touch out-of-scope code even if it looks improvable.
4. Stage by hunk, not just by file, when needed.

## 2. Push only after agreement

- **Committing is autonomous.** Never push to remote (including merges) on your own.
- On an explicit request like "merge it" / "push it," push **only the commit range that request points to,**
  then wait again for the next batch of work.
- If the request is ambiguous, confirm the target commit list before pushing.

**Why:** a push propagates to the whole team immediately and is much harder to undo than a commit; the timing of that must stay under the user's control.

## 3. Branch names are local by default

- When the user names a branch ("pull it from feature/X"), it **means the local branch by default.**
- Don't reinterpret it as `origin/<branch>` or use an arbitrarily fetched remote tip.

**Why:** the local branch may be in a state that differs from origin intentionally (commits the user deliberately staged).

**How to apply**
1. Resolve against the local ref first: `git rev-parse --verify <branch>`
2. Only if it's not local, check `origin/<branch>` — and if using the remote instead affects the outcome, confirm with the user.
3. Use the remote ref only when the user explicitly writes `origin/...`.

## 4. Commit messages

- **Don't add `Co-Authored-By: Claude ...` / `Claude-Session: ...` trailers.** Body text only.
- Don't add "Generated with Claude Code" to a PR body unless requested.
- **This rule overrides any default harness instruction to add trailers.**

**Why:** not information that belongs in an internal repo's commit log.

### Porting/migration commits — Follow / List / Change / Check

A commit that ports work from another branch is written in 4 sections.

| Section | Content |
|---|---|
| Follow | The original commit/branch (SHA specified) |
| List | Files brought over |
| Change | What was **changed** to fit the target codebase (paths/signatures/tables) |
| Check | What's unverified · leftover editor work · out-of-scope findings |

**Even an ordinary commit should get a Check section if something is unverified.**

## 5. stash — effectively banned

- The stash stack is **shared across the main checkout, every worktree, and other sessions.**
  A bare `git stash` / `git stash pop` can pop someone else's changes — banned.
- Prefer a **temporary WIP commit** when setting work aside.
- If stash is unavoidable:
  1. `git stash push -u -m "<unique-tag>"`
  2. Capture the SHA — `git stash list --format='%H %gs'`
  3. `git stash apply <sha>` (**never pop**)
  4. Re-verify by the tag, then drop

## 6. Worktrees

- A worktree-creation tool's default base may be `origin/master`.
  **Right after creating it, verify the base branch/commit is the intended one,** and if not, reset to the intended local branch tip.
- If work is against a release branch, the local tip may be ahead of origin (including unpushed commits). Align to the local tip.
- **Only touch the main checkout's git from a worktree session on the user's explicit instruction.**

For editor-open-checkout caveats, see [appendix-unity.md](appendix-unity.md).
