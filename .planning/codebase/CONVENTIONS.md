# Coding Conventions

**Analysis Date:** 2026-03-27

## Naming Patterns

**Files:**
- One class per file, file name matches class name exactly: `Cell.cs`, `CellManager.cs`, `UpgradeTreeView.cs`
- PascalCase for all file names
- Data definition files use `*Data.cs` suffix: `MutationData.cs`, `LineageUpgradeData.cs`, `CellUpgradeData.cs`

**Classes:**
- PascalCase: `CellManager`, `UpgradeTreePresenter`, `SidebarController`
- MonoBehaviour subclasses named by role: `*Controller`, `*View`, `*Presenter`, `*Handler`
- Non-MonoBehaviour POCOs use `*Def` suffix: `UpgradeDef`
- Static data factories use `*Data` suffix: `MutationData`, `LineageUpgradeData`
- Abstract base classes use plain nouns: `Cell`
- Interfaces use `I` prefix: `IClickable`

**Enums:**
- PascalCase enum name, UPPERCASE abbreviated values: `CellType { TPC, NSC, GMC, RGC, NB, N }`

**Methods:**
- PascalCase for public methods: `RegisterCell()`, `GetPrefab()`, `Differentiate()`
- PascalCase for private methods: `HandleCellBirth()`, `UpdateDisplayCount()`
- Exception: one legacy method uses camelCase: `freeCell()` in `Assets/Scripts/Cells/Cell.cs` line 35
- Event handlers prefixed with `On` or `Handle`: `OnClick()`, `HandleCellBirth()`, `OnNodeClicked()`
- Boolean query methods use `Is`/`Are`/`Can` prefix: `IsUnlocked()`, `ArePrereqsMet()`, `CanPurchase()`

**Fields:**
- Private fields: camelCase, no prefix: `busy`, `lastMousePos`, `dragging`
- SerializeField private fields: camelCase: `prepTime`, `splitTime`, `panSpeed`
- Public properties: PascalCase: `Instance`, `CellType`
- Constants: SCREAMING_SNAKE_CASE: `LINEAGE_HOLDER_NAME`, `SIDEBAR_UNLOCK_THRESHOLD`, `LINEAGE_UNLOCK_THRESHOLD`
- Readonly fields: camelCase: `view`, `catalog`
- Dictionary fields: descriptive camelCase: `nodeById`, `upgradeById`, `rowById`

**Properties:**
- PascalCase with expression bodies for simple getters:
  ```csharp
  public override CellType cellType => CellType.TPC;  // Note: lowercase 'c' - inconsistent
  public static CellManager Instance { get; private set; }
  public CellType CellType => cellType;
  ```
- Note: `cellType` property on `Cell` subclasses uses camelCase (breaking convention). All new properties should use PascalCase.

**Events/Delegates:**
- PascalCase: `CellBirth`, `NodeClicked`, `UpgradeClicked`, `ViewRestored`, `ClickedNothing`
- Delegate types prefixed with `On`: `OnCellBirth`
- Static events for global broadcasting: `CellManager.CellBirth`, `CameraHandler.ClickedNothing`, `CellNodeClickable.Clicked`

## Code Style

**Formatting:**
- No `.editorconfig` or formatting tool configured
- Allman-style braces (opening brace on new line) for classes and most methods
- Exception: some methods use K&R style (opening brace on same line), especially short ones:
  ```csharp
  void Start() {           // K&R in GanglionMotherCell.cs
  void Update () {          // K&R with space before parens in NeuralStemCell.cs
  ```
- Use Allman style (brace on new line) for consistency in new code
- 4-space indentation throughout

**Linting:**
- No linter configured
- No Roslyn analyzers or StyleCop

**Access Modifiers:**
- Explicit `private` on SerializeField members: `[SerializeField] private float prepTime`
- Implicit private on non-serialized fields (no keyword): `bool busy = false;` -- avoid this, always be explicit
- `public static` for singleton Instance properties
- `sealed` on classes that should not be extended: `UpgradeTreeView`, `UpgradeTreePresenter`, `UpgradeDef`, `UpgradeCatalog`
- `abstract` on base classes meant for extension: `Cell`

**Spacing:**
- Inconsistent spacing before parentheses in method declarations: `void Update ()` vs `void Update()`
- Use no space before parentheses in new code

## Import Organization

**Order (observed pattern):**
1. `System` / `System.Collections` / `System.Collections.Generic` / `System.Linq`
2. `UnityEngine`
3. `UnityEngine.UIElements`
4. `UnityEngine.InputSystem`

**No `using` statements** in files that only reference types from the default namespace (project classes): see `Assets/Scripts/UI/UpgradeTreePresenter.cs`

**Path Aliases:**
- None configured (no assembly definitions, no custom namespaces)

## Namespaces

- **No namespaces used.** All classes are in the global namespace.
- No assembly definition files (`.asmdef`) exist.
- When adding new code, do NOT introduce namespaces unless refactoring the entire project.

## Common Design Patterns

**Singleton (MonoBehaviour):**
Used by core managers. Pattern:
```csharp
public static CellManager Instance { get; private set; }

void Awake()
{
    if (Instance == null)
        Instance = this;
    else
    {
        Destroy(gameObject);
        return;
    }
}
```
- Files using this: `Assets/Scripts/Core/CellManager.cs`, `Assets/Scripts/Core/Upgrades.cs`
- Access via `CellManager.Instance`, `Upgrades.Instance`

**MVP (Model-View-Presenter):**
UI follows MVP pattern for upgrade trees and cell upgrade popups:
- **Model:** `UpgradeCatalog` + `UpgradeDef` (in `Assets/Scripts/Interfaces/UpgradeState.cs`)
- **View:** MonoBehaviour managing UI Toolkit elements (`Assets/Scripts/UI/UpgradeTreeView.cs`, `Assets/Scripts/UI/CellUpgradePopupView.cs`)
- **Presenter:** Plain C# class wiring view events to model mutations (`Assets/Scripts/UI/UpgradeTreePresenter.cs`, `Assets/Scripts/UI/CellUpgradePopupPresenter.cs`)
- Presenter is NOT a MonoBehaviour; it is created by the View or a manager in `OnEnable()`

**Static Factory for Data:**
Upgrade definitions live in static classes with `CreateCatalog()` methods:
```csharp
public static class MutationData
{
    public static UpgradeCatalog CreateCatalog() { ... }
}
```
- Files: `Assets/Scripts/Data/MutationData.cs`, `Assets/Scripts/Data/LineageUpgradeData.cs`, `Assets/Scripts/Data/CellUpgradeData.cs`

**Observer / Event System:**
- C# delegates and events for decoupled communication (NOT Unity Events)
- Static events for global broadcasts:
  ```csharp
  public static event OnCellBirth CellBirth;              // CellManager
  public static event System.Action ClickedNothing;       // CameraHandler
  public static event Action<CellNodeClickable> Clicked;  // CellNodeClickable
  ```
- Instance events for local UI communication:
  ```csharp
  public event Action<string> NodeClicked;    // UpgradeTreeView
  public event Action<string> UpgradeClicked; // UpgradeTreeView
  public event Action ViewRestored;           // UpgradeTreeView
  ```
- Always subscribe in `OnEnable()`, unsubscribe in `OnDisable()` (or `OnDestroy()`)

**Template Method (Inheritance):**
- `Cell` is abstract base; subclasses override `OnClick()` and `cellType`
- Files: `Assets/Scripts/Cells/Cell.cs` (base), `Assets/Scripts/Cells/TotipotentStemCell.cs`, `Assets/Scripts/Cells/NeuralStemCell.cs`, `Assets/Scripts/Cells/GanglionMotherCell.cs`, `Assets/Scripts/Cells/RadialGlialSpawner.cs`

**Lazy Initialization:**
- UI views use `EnsureInitialized()` guard pattern to defer setup until first use:
  ```csharp
  void EnsureInitialized()
  {
      if (initialized) return;
      // ... setup ...
      initialized = true;
  }
  ```
- Used in: `Assets/Scripts/UI/UpgradeTreeView.cs`, `Assets/Scripts/UI/CellUpgradePopupView.cs`

## Unity-Specific Conventions

**SerializeField Usage:**
- Private fields exposed to Inspector via `[SerializeField]`:
  ```csharp
  [SerializeField] private float prepTime = 0.25f;
  [SerializeField] UIDocument doc;
  ```
- Grouped with `[Header("...")]` attributes:
  ```csharp
  [Header("Division Timing")]
  [Header("Pan")]
  [Header("Zoom")]
  [Header("Death Animation")]
  ```

**RequireComponent:**
- Applied to classes needing specific components: `[RequireComponent(typeof(SphereCollider))]` on `Cell` and `CellNodeClickable`, `[RequireComponent(typeof(UIDocument))]` on `CellUpgradePopupView`

**Coroutines for Animation:**
- Cell division, differentiation, and death animations use coroutines:
  ```csharp
  public void Divide(GameObject child)
  {
      if (busy) return;
      StartCoroutine(DivideRoutine(child));
  }
  ```
- Busy flag prevents re-entry: `protected bool busy = false;`
- Routine naming convention: `*Routine` suffix for private IEnumerator methods

**Prefab Loading:**
- Via `Resources.Load<GameObject>("Cells/...")` in `CellManager.Awake()`
- Prefab names use camelCase: `totipotentStemCell`, `neuralStemCell`

**UI Toolkit (NOT UGUI):**
- All UI uses Unity UI Toolkit (`UnityEngine.UIElements`)
- UXML files in `Assets/Scenes/UI/`: `Sidebar.uxml`, `Counter.uxml`, `Upgrades.uxml`, `CellUpgradePopup.uxml`
- USS stylesheets in `Assets/Scenes/UI/`: `sidebar.uss`, `counter.uss`, `upgrades.uss`, `cell-upgrade-popup.uss`
- Elements queried by name: `root.Q<VisualElement>("Counter")`, `root.Q<Label>("Count")`
- CSS classes for state: `is-locked`, `is-available`, `is-affordable`, `is-partial`, `is-maxed`, `is-selected`, `visible`
- USS file names use kebab-case: `cell-upgrade-popup.uss`
- UXML file names use PascalCase: `CellUpgradePopup.uxml`

**UIDocument Injection:**
- Views accept UIDocument via `SetDocument(UIDocument)` or `[SerializeField] UIDocument doc`
- Pattern allows sharing a single UIDocument across multiple view components

## Error Handling

**Patterns:**
- Guard clauses with early return:
  ```csharp
  if (busy) return;
  if (popup == null) return;
  if (!child) yield break;
  ```
- `Debug.LogError()` for missing references / initialization failures:
  ```csharp
  Debug.LogError("UpgradeTreeView: No UIDocument found!");
  Debug.LogError($"CellManager: No prefab found for CellType {cellType}");
  ```
- `Debug.LogWarning()` for non-critical issues:
  ```csharp
  Debug.LogWarning($"CellManager: No cells of type {ct} available to kill.");
  ```
- No try/catch blocks used anywhere; errors are handled via null checks and guard clauses
- No custom exception types

## Comments

**When to Comment:**
- Inline comments for non-obvious math or logic:
  ```csharp
  return x * x * (3f - 2f * x); // smoothstep
  ```
- Step-numbered comments for multi-phase routines:
  ```csharp
  // 1) Prep: squash and stretch (like pressure building)
  // 2) Spawn daughter at same position
  // 3) Split: separate both halves outward
  // 4) Settle: both return to normal scale
  ```
- Comments explaining "why" for non-obvious decisions:
  ```csharp
  // pixel -> world units (keeps pan consistent at different zoom levels)
  // Flip Y: screen space is bottom-up, UI Toolkit expects top-down
  ```

**XML Documentation:**
- Minimal usage. Only one `<summary>` found in `Assets/Scripts/Data/MutationData.cs`:
  ```csharp
  /// <summary>
  /// List of upgrades
  /// </summary>
  ```
- Not required for new code, but add XML docs on public APIs of shared utility classes

## String Formatting

- Use string interpolation (`$"..."`) consistently:
  ```csharp
  Debug.Log($"Purchased {upgrade.Title} (Level {upgrade.Level})");
  Debug.LogError($"CellManager: No prefab found for CellType {cellType}");
  ```

## Module Design

**Exports:**
- All classes are public (global namespace, no assembly boundaries)
- No barrel files or index patterns

**File Organization by Role:**
- `Assets/Scripts/Core/` - Singletons and core managers
- `Assets/Scripts/Cells/` - Cell type implementations (MonoBehaviours)
- `Assets/Scripts/Data/` - Static data definition factories
- `Assets/Scripts/UI/` - UI views, presenters, controllers
- `Assets/Scripts/Interfaces/` - Interfaces and shared data types
- `Assets/Scripts/Lineage/` - Lineage-specific gameplay components

---

*Convention analysis: 2026-03-27*
