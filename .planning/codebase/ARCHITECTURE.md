# Architecture

**Analysis Date:** 2026-03-27

## Pattern Overview

**Overall:** MonoBehaviour Singleton + MVP (Model-View-Presenter) hybrid

**Key Characteristics:**
- Core systems (`CellManager`, `Upgrades`) use the Singleton pattern via `static Instance` properties
- UI follows a View/Presenter split for upgrade trees and popups (MVP pattern)
- Cell behaviors use inheritance from an abstract `Cell` base class
- Inter-system communication relies on C# events/delegates (not Unity Events)
- Upgrade data is defined in static factory classes that produce `UpgradeCatalog` instances
- No dependency injection, no ScriptableObject-based architecture, no ECS

## Layers

**Core (Singletons):**
- Purpose: Central game state and system management
- Location: `Assets/Scripts/Core/`
- Contains: `CellManager.cs`, `Upgrades.cs`, `Camera.cs`
- Depends on: Data layer for upgrade catalogs, UI layer for views
- Used by: All other layers query singletons directly

**Cells (Game Logic):**
- Purpose: Individual cell behaviors, division/differentiation logic
- Location: `Assets/Scripts/Cells/`
- Contains: Abstract `Cell` base class and concrete cell type implementations
- Depends on: `CellManager` singleton for prefab lookup and registration
- Used by: Instantiated at runtime via `Cell.Divide()` and `Cell.Differentiate()`

**Data (Static Definitions):**
- Purpose: Upgrade definitions and catalogs
- Location: `Assets/Scripts/Data/`
- Contains: `MutationData.cs`, `LineageUpgradeData.cs`, `CellUpgradeData.cs`
- Depends on: `UpgradeDef` and `UpgradeCatalog` types from `Assets/Scripts/Interfaces/UpgradeState.cs`
- Used by: `Upgrades` singleton, presenters

**UI (Presentation):**
- Purpose: UI Toolkit views and their presenters
- Location: `Assets/Scripts/UI/`
- Contains: Views (`UpgradeTreeView`, `CellUpgradePopupView`, `Counter`, `SidebarController`) and Presenters (`UpgradeTreePresenter`, `CellUpgradePopupPresenter`)
- Depends on: Core singletons, Data layer, UI Toolkit (UXML/USS)
- Used by: Bound to UIDocument components in the scene

**Interfaces:**
- Purpose: Shared contracts and data types
- Location: `Assets/Scripts/Interfaces/`
- Contains: `IClickable.cs` (click interface), `UpgradeState.cs` (defines `UpgradeDef` and `UpgradeCatalog`)
- Depends on: Nothing
- Used by: Cells, Lineage, UI, Core

**Lineage:**
- Purpose: Clickable cell nodes for the lineage/evolution view
- Location: `Assets/Scripts/Lineage/`
- Contains: `CellNodeClickable.cs`
- Depends on: `IClickable` interface, static events
- Used by: `CellUpgradePopupPresenter` listens for `CellNodeClickable.Clicked`

## Data Flow

**Cell Lifecycle (Division/Differentiation):**

1. Player clicks a cell in the scene -> `CameraHandler.Update()` raycasts and calls `IClickable.Click()`
2. The cell's `OnClick()` calls `Divide()` or `Differentiate()` with a prefab from `CellManager.Instance.GetPrefab()`
3. `Cell.Divide()` runs a coroutine: squash animation -> `Instantiate()` daughter cell -> split animation -> settle
4. New cell's `Start()` calls `CellManager.Instance.RegisterCell(this)` which fires `CellBirth` event
5. `CellBirth` listeners update UI counters, check unlock thresholds, adjust camera zoom

**Upgrade Purchase Flow:**

1. Player clicks an upgrade node in the tree UI -> `UpgradeTreeView.NodeClicked` event fires
2. `UpgradeTreePresenter.OnNodeClicked()` selects the node and refreshes detail panel
3. Player clicks "Upgrade" button -> `UpgradeTreeView.UpgradeClicked` event fires
4. `UpgradeTreePresenter.OnUpgradeClicked()` checks `CanPurchase()` (prereqs met + enough RGC cells)
5. If affordable: `CellManager.Instance.KillCells(CellType.RGC, cost)` destroys RGC cells as payment
6. `upgrade.Level++` is set directly on the `UpgradeDef` object
7. Cell behaviors query `Upgrades.Instance.IsUpgradeUnlocked("id")` each frame to gate features

**Cell Upgrade Popup Flow:**

1. Player clicks a cell node in the lineage view -> `CellNodeClickable.Clicked` static event fires
2. `CellUpgradePopupPresenter.OnCellNodeClicked()` filters upgrades by cell type prefix (e.g., "tpc-", "nsc-")
3. `CellUpgradePopupView.Show()` builds rows dynamically and positions popup at the clicked world position
4. Purchase follows same pattern: kill RGC cells, increment level, refresh UI

**Progressive Unlocking:**

1. `SidebarController` listens for `CellBirth` and shows sidebar when RGC count reaches 5
2. `Upgrades` listens for `CellBirth` and enables the lineage upgrade tree when RGC count reaches 10
3. `Counter` slides down when RGC count reaches 100

**State Management:**
- Game state is ephemeral (no save/load system detected)
- Upgrade levels are stored as mutable `short Level` fields on `UpgradeDef` instances in memory
- Cell counts are tracked by `CellManager.cells` dictionary (maps `CellType` to `CellInfo` with live list and lifetime count)
- Three separate `UpgradeCatalog` instances exist: `neurosphereCatalog` (mutations), `lineageCatalog` (lineage upgrades), and a cell-upgrade catalog (in `CellUpgradePopupView`)
- No persistence layer -- all state resets on play mode restart

## Key Abstractions

**Cell (Abstract Base Class):**
- Purpose: Shared division/differentiation/death animation logic for all cell types
- Examples: `Assets/Scripts/Cells/Cell.cs`
- Pattern: Template Method -- subclasses override `OnClick()` and set `cellType`, base class handles animation coroutines
- All cells require `SphereCollider`, implement `IClickable`

**CellType (Enum):**
- Purpose: Identifies cell species: TPC, NSC, GMC, RGC, NB, N
- Defined in: `Assets/Scripts/Core/CellManager.cs`
- Used for: Prefab lookup, cell counting, upgrade filtering by prefix

**UpgradeDef / UpgradeCatalog:**
- Purpose: Data-driven upgrade definitions with cost functions, prerequisites, and positioning
- Examples: `Assets/Scripts/Interfaces/UpgradeState.cs`
- Pattern: Plain C# objects (not ScriptableObjects). Created by static factory methods in Data classes.
- `Cost` is a `Func<short, int>` allowing level-scaling cost formulas

**MVP Pattern (View + Presenter):**
- Purpose: Separates UI rendering from game logic for upgrade trees
- View examples: `Assets/Scripts/UI/UpgradeTreeView.cs`, `Assets/Scripts/UI/CellUpgradePopupView.cs`
- Presenter examples: `Assets/Scripts/UI/UpgradeTreePresenter.cs`, `Assets/Scripts/UI/CellUpgradePopupPresenter.cs`
- Pattern: View fires events -> Presenter handles logic -> Presenter calls View methods to update display

## Entry Points

**Main Scene:**
- Location: `Assets/Scenes/Neurosphere.unity`
- Triggers: Loaded on play
- Responsibilities: Contains all game objects -- camera rig, cells, UI documents, environment hierarchy

**CellManager (Singleton):**
- Location: `Assets/Scripts/Core/CellManager.cs`
- Triggers: `Awake()` -- initializes singleton, loads all cell prefabs from `Resources/Cells/`
- Responsibilities: Central registry for all live cells, prefab provider, cell counting, cell destruction

**Upgrades (Singleton):**
- Location: `Assets/Scripts/Core/Upgrades.cs`
- Triggers: `Awake()` -- initializes singleton, creates upgrade catalogs, sets up UI
- Responsibilities: Facade for querying upgrade state (`IsUpgradeUnlocked`, `GetUpgradeLevel`), manages two upgrade tree presenters, progressive UI unlocking

**CameraHandler:**
- Location: `Assets/Scripts/Core/Camera.cs`
- Triggers: Every `Update()` frame
- Responsibilities: Raycast click detection (dispatches to `IClickable`), orthographic pan/zoom, fires `ClickedNothing` event for popup dismissal

## Error Handling

**Strategy:** Minimal -- relies on Unity's null checks and `Debug.Log` / `Debug.LogError`

**Patterns:**
- Null-conditional access for optional references: `clickable?.Click()`
- Guard clauses with `if (busy) return` to prevent re-entrant coroutines
- `Debug.LogError` for missing prefabs or UI elements (development-time diagnostics)
- No try/catch blocks, no error recovery system

## Cross-Cutting Concerns

**Logging:** `Debug.Log` / `Debug.LogError` / `Debug.LogWarning` used directly (no abstraction layer)

**Validation:** Compile-time via `[RequireComponent]` attribute on `Cell` and `CellNodeClickable`. Runtime checks in `CellManager.GetPrefab()` for missing prefabs.

**Authentication:** Not applicable (single-player offline game)

**Event System:** C# delegates and events used for decoupled communication:
- `CellManager.CellBirth` (delegate `OnCellBirth`) -- broadcast when any cell registers
- `CameraHandler.ClickedNothing` (static Action) -- broadcast when player clicks empty space
- `CellNodeClickable.Clicked` (static Action) -- broadcast when a lineage node is clicked
- `UpgradeTreeView.NodeClicked` / `UpgradeClicked` (instance events) -- UI node interactions
- `UpgradeTreeView.ViewRestored` (instance event) -- re-initialization after scene changes

**Resource Loading:** `Resources.Load<GameObject>()` used in `CellManager.Awake()` for cell prefabs. All prefabs stored in `Assets/Resources/Cells/`.

---

*Architecture analysis: 2026-03-27*
