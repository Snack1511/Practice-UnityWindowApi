# Appendix — Unity-Specific Rules

Delete this file entirely if the project isn't Unity.

## 1. MonoBehaviour lifecycle

**`Awake()` is binding-only.** Serialized-field caching and internal reference wiring, nothing else.

**Subscriptions belong in `Start()`.** Initial registration of delegates, C# events, `UnityEvent.AddListener`,
and message-bus listeners all go there. Call `base.Start()` first when overriding.

**Unsubscribe symmetrically in `OnDisable()` / `OnDestroy()`.**

**Why:** at `Awake()` time there's no guarantee other objects have finished initializing. The subscription target might not exist yet.

## 2. Field declarations

**Don't add explicit default initializers (`= null`, `= 0`, `= false`) on reference/value-type fields.** Rely on C# defaults.

**Exception — collections must be initialized.** `List<T>`, `Dictionary<K,V>`, `HashSet<T>`, `Queue<T>`, etc. need `= new()`.
Skipping it means `NullReferenceException`.

This rule applies **only to field declarations** — not properties, locals, or parameter defaults.

## 3. Unity editor vs. git operations

- **Rebasing/checking out a checkout with the editor open** can have Unity regenerate files mid-operation, aborting the rebase.
  - Workaround: discard the churn files Unity created, then `git rebase --continue`.
  - Close the editor for that checkout before rebasing when possible.
- A worktree project and the main checkout project can each be open in Unity separately.
  **Watch for confusion about which editor an MCP connection is pointed at** — confirm the target project before working.

## 4. Editor version

**Opening the project with an editor version different from `ProjectSettings/ProjectVersion.txt`
triggers asset-database and `.meta` reserialization that's hard to undo.**

If the version can't be confirmed, don't launch it — tell the user. Never open with a different version arbitrarily.

## 5. Editing prefab/scene YAML directly

When editing prefab/scene YAML without the editor (swapping script GUIDs, deleting nodes, etc.):

- Swapping the root `m_Script` GUID while **preserving field names** keeps serialized references intact.
- **A stripped RectTransform's children don't appear in `m_Children`.**
  Deleting a node in a nested prefab can orphan an override child, so **a scan for orphaned nodes after deletion is mandatory.**
- After a swap, clean up leftover fields and do a full pass checking internal fileID reference integrity.
- Verify by editor log — confirm compile success (`error CS` count 0) and prefab import success, then finish with a play-mode QA pass.

> A batch-mode editor script is safer than YAML surgery for node deletion/prefab creation.
> Running `EditorSceneManager.OpenScene` → `DestroyImmediate` → `SaveScene` through `-executeMethod`
> lets Unity keep child references and SceneRoots consistent on its own. Delete the temp `.cs` and its `.meta` right after running.

## 6. Conditional-compilation blocks

The inside of `#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` **is not verified by the editor.**
Batch-mode compile checks don't touch this block either. **A real target-platform build is the only way to verify it.**

If touched, always run the real-build verification step (step 3). → [00-process.md](00-process.md) §6

## 7. Async-load UI integration pattern (reference)

When merging several popups into one frame + panel prefabs:

- Open policy: `SetInfo` sets the root CanvasGroup alpha to 0 → parallel-loads all panels (`UniTask.WhenAll`) → restores alpha and plays the open animation
- Panel instances are created once per popup pool and cached; release Addressables handles in `OnDestroy`
- If an A/B test variant (`_B`) exists, even the frame layout (close-button position, etc.) may differ — **the frame prefab may need a per-variant version too**
