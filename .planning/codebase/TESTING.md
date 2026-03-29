# Testing Patterns

**Analysis Date:** 2026-03-27

## Test Framework

**Runner:**
- No test framework is configured
- No Unity Test Runner assembly definitions (`.asmdef`) exist under `Assets/`
- No `Tests/` directory exists
- `.gitignore` includes `InitTestScene*.unity*` (Unity Test Runner default), suggesting awareness but no active tests

**Assertion Library:**
- Not applicable (no tests)

**Run Commands:**
```bash
# No test commands available
# To add tests, create Assets/Tests/EditMode/ and Assets/Tests/PlayMode/ with .asmdef files
```

## Test File Organization

**Location:**
- No test files exist anywhere in the project

**To add tests (recommended structure):**
```
Assets/
  Tests/
    EditMode/
      EditModeTests.asmdef       # Assembly def referencing NUnit, TestRunner
      UpgradeCatalogTests.cs     # Pure logic tests
      CellUpgradeDataTests.cs
    PlayMode/
      PlayModeTests.asmdef       # Assembly def referencing NUnit, TestRunner, UnityEngine.TestRunner
      CellDivisionTests.cs       # Tests requiring MonoBehaviour lifecycle
      CellManagerTests.cs
```

## Debug / Logging Patterns

**Framework:** `UnityEngine.Debug` (built-in)

**Log Levels Used:**
- `Debug.Log()` - Informational, success confirmations:
  ```csharp
  Debug.Log($"Purchased {upgrade.Title} (Level {upgrade.Level})");
  Debug.Log("UpgradeTreeView: Successfully initialized and found Lineage element");
  ```
- `Debug.LogWarning()` - Non-critical issues, graceful degradation:
  ```csharp
  Debug.LogWarning($"CellManager: No cells of type {ct} available to kill.");
  Debug.LogWarning($"CellManager: Attempted to kill {count} cells of type {ct}, but only {cellsToKill} are available.");
  ```
- `Debug.LogError()` - Missing references, initialization failures:
  ```csharp
  Debug.LogError("UpgradeTreeView: No UIDocument found!");
  Debug.LogError($"CellManager: No prefab found for CellType {cellType}");
  ```
- `print()` (MonoBehaviour shorthand) - Used once in `Assets/Scripts/Cells/Cell.cs` line 41:
  ```csharp
  print($"{this.GetType().FullName} clicked");
  ```
  Prefer `Debug.Log()` over `print()` for consistency.

**Debug Utilities:**
- `DebugPrintHierarchy()` in `Assets/Scripts/UI/UpgradeTreeView.cs` - Recursive UI element tree printer for diagnosing UXML structure issues:
  ```csharp
  void DebugPrintHierarchy(VisualElement element, int depth)
  ```
  This is a development-time debug helper, not stripped in builds.

**Logging Conventions for New Code:**
- Prefix log messages with the class name: `"CellManager: ..."`, `"UpgradeTreeView: ..."`
- Use string interpolation for dynamic values
- Use `Debug.LogError()` when returning early due to missing dependencies
- Use `Debug.LogWarning()` for recoverable edge cases

## Editor Tooling and Custom Inspectors

**Custom Inspectors:**
- None detected in `Assets/Scripts/`
- `Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs` exists (Unity template, not project-specific)

**Custom Editor Windows:**
- None

**Inspector Debugging:**
- `CellManager` exposes `countsDisplay` (a `List<CellCountEntry>`) via `[SerializeField]` for runtime cell count monitoring in the Inspector: `Assets/Scripts/Core/CellManager.cs` line 40
- `[Header("...")]` attributes organize Inspector fields into logical groups

**Gizmos / Handles:**
- None detected

**ScriptableObjects:**
- Not used. Upgrade data lives in static C# classes (`MutationData`, `LineageUpgradeData`, `CellUpgradeData`) rather than ScriptableObject assets.

## Validation Approaches

**Runtime Validation:**
- Null checks on component lookups:
  ```csharp
  if (col != null) col.includeLayers = 1 << gameObject.layer;
  if (uiDoc == null) return;
  if (popup == null) return;
  ```
- Dictionary `TryGetValue()` for safe lookups:
  ```csharp
  if (cells.TryGetValue(cellType, out CellInfo cellInfo) && cellInfo.prefab != null)
  ```
- `RequireComponent` attribute for mandatory component dependencies:
  ```csharp
  [RequireComponent(typeof(SphereCollider))]   // Cell, CellNodeClickable
  [RequireComponent(typeof(UIDocument))]       // CellUpgradePopupView
  ```

**No Compile-Time Validation:**
- No Roslyn analyzers
- No `[DisallowMultipleComponent]` usage
- No `OnValidate()` methods for Inspector-time checks
- No `#if UNITY_EDITOR` conditional compilation blocks

**Input Validation:**
- Guard clauses prevent invalid operations:
  ```csharp
  if (busy) return;                    // Prevent re-entrant coroutines
  if (index < 0 || index >= ...) return;  // Bounds checking
  if (!child) yield break;             // Null prefab check
  ```
- Clamping for numeric ranges:
  ```csharp
  Mathf.Clamp(cam.orthographicSize - ..., minOrthoSize, maxOrthoSize);
  Mathf.Max(0.0001f, duration);  // Prevent division by zero
  ```

## Coverage Gaps

**Untested pure logic (good candidates for EditMode tests):**
- `UpgradeCatalog` - `IsUnlocked()`, `GetUpgradeLevel()` in `Assets/Scripts/Interfaces/UpgradeState.cs`
- `UpgradeTreePresenter` - `CanPurchase()`, `ArePrereqsMet()` in `Assets/Scripts/UI/UpgradeTreePresenter.cs`
- `CellUpgradePopupPresenter` - `CanPurchase()`, `ArePrereqsMet()` in `Assets/Scripts/UI/CellUpgradePopupPresenter.cs`
- `CellUpgradeData.GetUpgradesForType()` in `Assets/Scripts/Data/CellUpgradeData.cs`
- Cost calculation lambdas across all `*Data.cs` files

**Untested MonoBehaviour logic (would need PlayMode tests):**
- `CellManager` registration/unregistration lifecycle
- `Cell` division and differentiation coroutines
- `SidebarController` unlock threshold logic
- Camera pan/zoom behavior in `CameraHandler`

## Recommendations for Adding Tests

1. Create `Assets/Tests/EditMode/` with an assembly definition referencing `Assembly-CSharp`
2. Start with pure logic: `UpgradeCatalog`, presenter purchase logic, cost functions
3. Extract `CanPurchase` and `ArePrereqsMet` into a shared utility (currently duplicated between `UpgradeTreePresenter` and `CellUpgradePopupPresenter`)
4. Add PlayMode tests for `CellManager` singleton and cell lifecycle

---

*Testing analysis: 2026-03-27*
