# Codebase Concerns

**Analysis Date:** 2026-03-27

---

## Critical

### Exponential Cell Growth with No Cap

- Issue: `NeuralStemCell.Update()` calls `Split()` every frame when `auto-divide` is unlocked, and each split spawns a new cell that also runs `Update()`. `GanglionMotherCell.Update()` similarly auto-divides. There is no population cap, spawn rate limiter, or cooldown. Cell count grows exponentially until performance collapses.
- Files: `Assets/Scripts/Cells/NeuralStemCell.cs` (lines 12-16), `Assets/Scripts/Cells/GanglionMotherCell.cs` (lines 25-35)
- Impact: Once `auto-divide` is purchased, the game will freeze within seconds as thousands of GameObjects spawn, each running coroutines and Update loops.
- Fix approach: Add a population cap in `CellManager`, a cooldown timer on division, or a rate limiter. At minimum, add `yield return new WaitForSeconds(interval)` or a timer check before allowing the next division.

### Double Registration in GanglionMotherCell

- Issue: `GanglionMotherCell.Start()` explicitly calls `CellManager.Instance.RegisterCell(this)` (line 22), but the base `Cell.Start()` also calls `RegisterCell(this)` (line 31). Since `GanglionMotherCell` does not call `base.Start()`, only the child `Start()` runs -- but this is fragile and depends on Unity's implicit behavior. If someone adds `base.Start()` or the base `Start()` were made virtual and called, the cell would be double-registered.
- Files: `Assets/Scripts/Cells/GanglionMotherCell.cs` (line 22), `Assets/Scripts/Cells/Cell.cs` (line 31)
- Impact: Currently works by accident. The base `Cell.Start()` is hidden (not overridden), so only `GanglionMotherCell.Start()` runs. But this is a maintenance trap -- any refactor could cause double-counting in cell lists and `lifetimeCount`.
- Fix approach: Make `Cell.Start()` virtual and have `GanglionMotherCell` override it with `base.Start()`, removing the duplicate `RegisterCell` call. Or move registration to `Awake()` in the base class.

### Material Instance Leaks

- Issue: Accessing `Renderer.material` (not `sharedMaterial`) creates a new Material instance each time. This happens in `Cell.ApoptosisRoutine()` and `GanglionMotherCell.Start()`. These cloned materials are never explicitly destroyed and will leak until the GameObject is collected.
- Files: `Assets/Scripts/Cells/Cell.cs` (lines 169, 178), `Assets/Scripts/Cells/GanglionMotherCell.cs` (line 18)
- Impact: With exponential cell growth, this creates thousands of leaked material instances, increasing GPU memory pressure and causing GC spikes.
- Fix approach: Cache the material reference once and destroy it in `OnDestroy()`, or use `MaterialPropertyBlock` to avoid instancing entirely.

---

## Major

### No Persistence / Save System

- Issue: All upgrade state (`UpgradeDef.Level`) and cell counts exist only in memory. There is no serialization, save/load, or PlayerPrefs usage anywhere in the codebase.
- Files: `Assets/Scripts/Interfaces/UpgradeState.cs`, `Assets/Scripts/Data/MutationData.cs`, `Assets/Scripts/Data/LineageUpgradeData.cs`, `Assets/Scripts/Data/CellUpgradeData.cs`
- Impact: All player progress is lost on application quit or scene reload. For an incremental/idle game, this is a fundamental missing feature.
- Fix approach: Implement a save system using JSON serialization to `Application.persistentDataPath` or `PlayerPrefs`. Serialize upgrade levels and cell counts.

### CameraHandler Event Leak (OnEnable without OnDisable)

- Issue: `CameraHandler.OnEnable()` subscribes to `CellManager.CellBirth` but there is no corresponding `OnDisable()` or `OnDestroy()` to unsubscribe.
- Files: `Assets/Scripts/Core/Camera.cs` (lines 24-27)
- Impact: If the CameraHandler is disabled/re-enabled or destroyed, the event handler accumulates duplicate subscriptions or holds a reference to a destroyed object, causing null reference exceptions.
- Fix approach: Add `void OnDisable() { CellManager.CellBirth -= HandleCellBirth; }`.

### UpgradeTreePresenter Never Unsubscribes from View Events

- Issue: `UpgradeTreePresenter` subscribes to `view.NodeClicked`, `view.UpgradeClicked`, and `view.ViewRestored` in its constructor but never unsubscribes. Unlike `CellUpgradePopupPresenter` which has a `Dispose()` method, `UpgradeTreePresenter` has no cleanup path.
- Files: `Assets/Scripts/UI/UpgradeTreePresenter.cs` (lines 13-16)
- Impact: Memory leak if presenters are re-created (e.g., on scene reload or UI rebuild). Old presenters remain referenced by view events.
- Fix approach: Add a `Dispose()` method that unsubscribes from all view events, and call it from the owning `Upgrades` component's `OnDisable()`.

### Duplicate CanPurchase / ArePrereqsMet Logic

- Issue: `CanPurchase()` and `ArePrereqsMet()` are copy-pasted identically in both `UpgradeTreePresenter` and `CellUpgradePopupPresenter`. Any bug fix or behavior change must be applied in two places.
- Files: `Assets/Scripts/UI/UpgradeTreePresenter.cs` (lines 51-79), `Assets/Scripts/UI/CellUpgradePopupPresenter.cs` (lines 54-79)
- Impact: Violation of DRY principle. Risk of purchase logic diverging between the two upgrade systems.
- Fix approach: Move purchase validation into `UpgradeCatalog` or a shared static utility method.

### Upgrade State Not Checked at Runtime

- Issue: Cell upgrade definitions in `CellUpgradeData.cs` (e.g., `tpc-division-rate`, `nsc-yield`, `rgc-scaffold`, etc.) are defined and purchasable, but no cell code reads these upgrades. The upgrades have no gameplay effect.
- Files: `Assets/Scripts/Data/CellUpgradeData.cs` (all 12 upgrades), `Assets/Scripts/Cells/` (none reference cell upgrade IDs)
- Impact: Players spend RGC on upgrades that do nothing. Only the `MutationData` upgrades (`gmc-basic`, `gmc-enhanced`, `gmc-advanced`, `auto-divide`) have any effect.
- Fix approach: Implement upgrade effects in the respective cell scripts by checking `Upgrades.Instance.GetUpgradeLevel()`.

### Counter.Update() Runs Every Frame

- Issue: `Counter.Update()` calls `SetRGCCount()` every frame, which queries `CellManager.Instance.GetCellCount()` and sets `Label.text`. This runs even when the count has not changed.
- Files: `Assets/Scripts/UI/Counter.cs` (lines 55-58)
- Impact: Unnecessary string allocation and UI rebuild every frame. Minor individually but compounds with other per-frame work.
- Fix approach: Subscribe to `CellManager.CellBirth` to update only when a cell is added/removed, or use a dirty flag.

---

## Minor

### File/Class Name Mismatch

- Issue: `Assets/Scripts/Cells/RadialGlialSpawner.cs` contains a class named `RadialGlialCell`, not `RadialGlialSpawner`. Unity requires the MonoBehaviour class name to match the file name for proper serialization.
- Files: `Assets/Scripts/Cells/RadialGlialSpawner.cs`
- Impact: Unity may fail to find the script when attaching it to GameObjects, or may show warnings in the Inspector. This could silently break prefab references.
- Fix approach: Rename the file to `RadialGlialCell.cs` (and update the `.meta` GUID reference) or rename the class.

### Placeholder Upgrade Descriptions

- Issue: `LineageUpgradeData` contains upgrades with "Placeholder." in their descriptions, indicating incomplete design.
- Files: `Assets/Scripts/Data/LineageUpgradeData.cs` (lines 14, 25)
- Impact: If exposed to players, looks unfinished. The lineage system appears to be partially implemented.
- Fix approach: Write real descriptions once gameplay effects are defined.

### Recovery Directory Contains Old Scene Files

- Issue: `Assets/_Recovery/` contains files named `0.unity` and `0 (1).unity`, which appear to be backup/recovery scenes.
- Files: `Assets/_Recovery/`
- Impact: Adds unnecessary files to the repository. Could cause confusion about which scenes are canonical.
- Fix approach: Remove the `_Recovery` directory or add it to `.gitignore`.

### Commented-Out Code in Cell.cs

- Issue: Line 106 contains a commented-out line for setting parent transforms: `// daughter.transform.SetParent(...)`.
- Files: `Assets/Scripts/Cells/Cell.cs` (line 106)
- Impact: Dead code that adds confusion about intended hierarchy management.
- Fix approach: Remove the commented code or implement the parenting logic if needed.

### Empty Statement in Apoptosis

- Issue: `Cell.Apoptosis()` contains `if (busy) {}` on line 158 -- an empty if-block that does nothing. The method continues regardless of the `busy` state.
- Files: `Assets/Scripts/Cells/Cell.cs` (line 158)
- Impact: Cells can begin apoptosis while mid-division, potentially causing visual glitches or null references if coroutines overlap.
- Fix approach: Either `return` early when busy, or queue the apoptosis to execute after the current operation completes.

### Debug.Log Statements Left in Production Code

- Issue: Multiple `Debug.Log` and `print()` calls scattered throughout the codebase for click handlers and initialization.
- Files: `Assets/Scripts/Cells/Cell.cs` (line 42), `Assets/Scripts/UI/UpgradeTreeView.cs` (line 100), `Assets/Scripts/UI/UpgradeTreePresenter.cs` (line 93), `Assets/Scripts/UI/CellUpgradePopupPresenter.cs` (line 51), `Assets/Scripts/Lineage/CellNodeClickable.cs` (line 14)
- Impact: Console spam during gameplay. Minor performance cost from string formatting.
- Fix approach: Use `#if UNITY_EDITOR` guards, a log level system, or remove after debugging is complete.

### Singleton Pattern Has No DontDestroyOnLoad

- Issue: Both `CellManager` and `Upgrades` use a basic singleton pattern that destroys duplicates but does not call `DontDestroyOnLoad`. If a scene transition occurs, these singletons are destroyed and any code referencing `.Instance` will get a null reference.
- Files: `Assets/Scripts/Core/CellManager.cs` (lines 43-52), `Assets/Scripts/Core/Upgrades.cs` (lines 25-33)
- Impact: Currently the game appears to be single-scene, so this is not an immediate problem. Becomes critical if scene loading is added.
- Fix approach: Add `DontDestroyOnLoad(gameObject)` if cross-scene persistence is needed, or document that these managers are scene-scoped.

### UpgradeTreeView Debug Hierarchy Print in Production

- Issue: `DebugPrintHierarchy()` recursively traverses and logs the entire UI tree. While only called on error, it could produce massive log output on complex UIs.
- Files: `Assets/Scripts/UI/UpgradeTreeView.cs` (lines 187-196)
- Impact: If the error condition is hit, the recursive logging could cause a performance hitch and flood the console.
- Fix approach: Guard with `#if UNITY_EDITOR` or remove once UI structure is stable.

### Three Separate UpgradeCatalogs with No Cross-Referencing

- Issue: `MutationData`, `LineageUpgradeData`, and `CellUpgradeData` each create independent `UpgradeCatalog` instances. The `Upgrades.IsUpgradeUnlocked()` method only searches the neurosphere and lineage catalogs, not the cell upgrade catalog.
- Files: `Assets/Scripts/Core/Upgrades.cs` (lines 64-71), `Assets/Scripts/UI/CellUpgradePopupView.cs` (line 31)
- Impact: Cell upgrades cannot be queried through the global `Upgrades.Instance.IsUpgradeUnlocked()` API. Any system that needs to check cell upgrade state must hold its own catalog reference.
- Fix approach: Register all catalogs with the `Upgrades` singleton, or merge into a unified catalog.

---

## Scaling Limits

### Cell List Management is O(n)

- Issue: `CellManager.UnregisterCell()` uses `List.Remove()` which is O(n) for each removal. `KillCells()` randomly indexes into the list and uses `RemoveAt()`. With thousands of cells, this becomes slow.
- Files: `Assets/Scripts/Core/CellManager.cs` (lines 94-101, 121-143)
- Impact: As cell populations grow (especially with auto-divide), list operations become a bottleneck.
- Fix approach: Use `HashSet<Cell>` for O(1) removal, or swap-and-pop for the list. For `KillCells`, consider batch removal.

### No Object Pooling

- Issue: Cells are created with `Instantiate()` and destroyed with `Destroy()`. No object pooling is used.
- Files: `Assets/Scripts/Cells/Cell.cs` (lines 70, 100, 182)
- Impact: With rapid cell creation/destruction (especially GMC auto-division), this causes GC pressure and frame hitches from allocation/deallocation.
- Fix approach: Implement Unity's built-in `ObjectPool<T>` or a custom pool for each cell prefab type.

---

## Test Coverage Gaps

### No Tests Exist

- What's not tested: The entire codebase. There are no unit tests, integration tests, or play-mode tests.
- Files: No test files found anywhere in the project.
- Risk: Any refactor (especially to the upgrade purchase logic, cell lifecycle, or event subscription patterns) could introduce silent regressions.
- Priority: Medium. The codebase is small enough that manual testing is feasible, but the interconnected event system and upgrade logic would benefit from automated tests.

---

*Concerns audit: 2026-03-27*
