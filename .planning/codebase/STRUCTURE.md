# Codebase Structure

**Analysis Date:** 2026-03-27

## Directory Layout

```
Assets/
├── Data/                    # Art/visual assets (materials)
│   └── Materials/           # All materials
│       ├── Cells/           # Per-cell-type materials
│       ├── Background.mat
│       ├── Lineage Background.mat
│       └── Neurosphere Background.mat
├── Editor/                  # Editor-only scripts (currently empty)
├── Fonts/                   # Custom fonts
├── Resources/               # Runtime-loadable assets (Resources.Load)
│   ├── Cells/               # Cell prefabs (loaded by CellManager)
│   └── Upgrades/            # Upgrade assets (currently empty)
├── Scenes/                  # Game scenes
│   ├── Neurosphere.unity    # Main (and only) game scene
│   └── UI/                  # UI Toolkit documents and stylesheets
├── Scripts/                 # All game scripts
│   ├── Cells/               # Cell behavior MonoBehaviours
│   ├── Core/                # Singleton managers
│   ├── Data/                # Static upgrade definition factories
│   ├── Interfaces/          # Shared types and interfaces
│   ├── Lineage/             # Lineage-view interaction scripts
│   └── UI/                  # UI views, presenters, controllers
├── Settings/                # URP render pipeline settings
├── TextMesh Pro/            # TMP package assets (third-party)
├── TutorialInfo/            # Unity template tutorial (unused)
├── UI Toolkit/              # UI Toolkit theme files
├── _Recovery/               # Scene backup files
└── InputSystem_Actions.inputactions  # New Input System config
Builds/                      # Build output (standalone)
Packages/                    # Unity package manifest
ProjectSettings/             # Unity project configuration
```

## Directory Purposes

**`Assets/Scripts/Core/`:**
- Purpose: Central game systems that other scripts depend on
- Contains: Singleton MonoBehaviours
- Key files:
  - `CellManager.cs`: Cell registry, prefab provider, cell counting, defines `CellType` enum
  - `Upgrades.cs`: Upgrade system facade, manages two upgrade tree presenters, progressive unlocking
  - `Camera.cs`: Click detection via raycast, orthographic pan/zoom, `ClickedNothing` event

**`Assets/Scripts/Cells/`:**
- Purpose: Cell type implementations inheriting from abstract `Cell`
- Contains: One script per cell type
- Key files:
  - `Cell.cs`: Abstract base class with division/differentiation/death animation coroutines
  - `TotipotentStemCell.cs`: Initial cell, differentiates into NSC on click
  - `NeuralStemCell.cs`: Divides into RGC or GMC, supports auto-divide upgrade
  - `GanglionMotherCell.cs`: Divides multiple times based on upgrade tier, then differentiates into RGC
  - `RadialGlialSpawner.cs`: Terminal cell type (filename misleading -- class is `RadialGlialCell`), has no behavior

**`Assets/Scripts/Data/`:**
- Purpose: Static factory classes that define upgrade trees as code
- Contains: No ScriptableObjects -- pure C# static methods returning `UpgradeCatalog`
- Key files:
  - `MutationData.cs`: Neurosphere upgrade definitions (gmc-basic, gmc-enhanced, gmc-advanced, auto-divide)
  - `LineageUpgradeData.cs`: Lineage upgrade definitions (lineage-alpha, lineage-beta -- placeholder)
  - `CellUpgradeData.cs`: Per-cell-type upgrade definitions (12 upgrades across all 6 cell types)

**`Assets/Scripts/Interfaces/`:**
- Purpose: Shared types used across multiple systems
- Contains: Interface and data class definitions
- Key files:
  - `IClickable.cs`: Single-method interface `Click()` implemented by `Cell` and `CellNodeClickable`
  - `UpgradeState.cs`: Defines `UpgradeDef` (upgrade definition with id, title, cost function, prereqs, position) and `UpgradeCatalog` (dictionary wrapper with `IsUnlocked`/`GetUpgradeLevel`)

**`Assets/Scripts/UI/`:**
- Purpose: UI Toolkit views and their presenters
- Contains: MonoBehaviour views and plain C# presenters
- Key files:
  - `UpgradeTreeView.cs`: Renders upgrade nodes as absolutely-positioned buttons in a pannable container, manages selection/detail panel
  - `UpgradeTreePresenter.cs`: Handles node click -> purchase logic for tree upgrades
  - `CellUpgradePopupView.cs`: World-positioned popup showing upgrades for a specific cell type
  - `CellUpgradePopupPresenter.cs`: Handles cell node click -> popup display -> purchase logic
  - `Counter.cs`: RGC count display, slides in at 100 RGC threshold
  - `SidebarController.cs`: Tab-based environment switcher, unlocks at 5 RGC

**`Assets/Scripts/Lineage/`:**
- Purpose: Interactive elements for the lineage/evolution tree environment
- Contains: Clickable 3D nodes representing cell types
- Key files:
  - `CellNodeClickable.cs`: MonoBehaviour + IClickable on 3D objects, fires static `Clicked` event

**`Assets/Scenes/UI/`:**
- Purpose: UI Toolkit layout (UXML) and style (USS) files
- Contains: Paired .uxml + .uss files for each UI panel
- Key files:
  - `Upgrades.uxml` / `upgrades.uss`: Upgrade tree panel layout and styling
  - `Sidebar.uxml` / `sidebar.uss`: Tab sidebar for environment switching
  - `Counter.uxml` / `counter.uss`: RGC counter display
  - `Lineage.uxml`: Lineage tree panel layout
  - `CellUpgradePopup.uxml` / `cell-upgrade-popup.uss`: Per-cell upgrade popup

**`Assets/Resources/Cells/`:**
- Purpose: Cell prefabs loaded at runtime via `Resources.Load<GameObject>()`
- Contains: One prefab per cell type
- Key files: `totipotentStemCell.prefab`, `neuralStemCell.prefab`, `ganglionMotherCell.prefab`, `radialGlialCell.prefab`, `neuroblast.prefab`, `neuron.prefab`

**`Assets/Data/Materials/`:**
- Purpose: Visual materials for cells and backgrounds
- Contains: Material assets
- Key files:
  - `Cells/`: One material per cell type + `Cell.mat` (base) + `unknownCell.mat`
  - `Background.mat`, `Neurosphere Background.mat`, `Lineage Background.mat`: Environment backgrounds

**`Assets/Settings/`:**
- Purpose: URP rendering configuration
- Contains: Render pipeline assets for Mobile and PC quality tiers
- Key files: `PC_RPAsset.asset`, `PC_Renderer.asset`, `Mobile_RPAsset.asset`, `Mobile_Renderer.asset`, `DefaultVolumeProfile.asset`

## Key File Locations

**Entry Points:**
- `Assets/Scenes/Neurosphere.unity`: Only game scene, contains entire game hierarchy

**Configuration:**
- `Assets/InputSystem_Actions.inputactions`: New Input System action definitions (Player, UI maps)
- `Assets/Settings/PC_RPAsset.asset`: PC render pipeline settings
- `Assets/Settings/Mobile_RPAsset.asset`: Mobile render pipeline settings
- `ProjectSettings/ProjectSettings.asset`: Unity project-level settings

**Core Logic:**
- `Assets/Scripts/Core/CellManager.cs`: Cell lifecycle management
- `Assets/Scripts/Core/Upgrades.cs`: Upgrade system coordination
- `Assets/Scripts/Cells/Cell.cs`: Base cell behavior (division, differentiation, death)

**Testing:**
- No test files detected in the project

## Naming Conventions

**Files:**
- Scripts: PascalCase matching class name (`CellManager.cs`, `UpgradeTreeView.cs`)
- Prefabs: camelCase (`neuralStemCell.prefab`, `ganglionMotherCell.prefab`)
- Materials: camelCase for cells (`neuralStemCell.mat`), Title Case for backgrounds (`Neurosphere Background.mat`)
- UXML: PascalCase (`Upgrades.uxml`, `Counter.uxml`, `CellUpgradePopup.uxml`)
- USS: kebab-case or camelCase (`upgrades.uss`, `cell-upgrade-popup.uss`, `sidebar.uss`)

**Directories:**
- PascalCase for top-level (`Scripts/`, `Scenes/`, `Data/`)
- PascalCase for sub-categories (`Scripts/Core/`, `Scripts/Cells/`, `Scripts/UI/`)

## Where to Add New Code

**New Cell Type:**
- Create cell script: `Assets/Scripts/Cells/{CellName}.cs` (extend `Cell`, set `cellType`, override `OnClick()`)
- Create prefab: `Assets/Resources/Cells/{camelCaseName}.prefab`
- Create material: `Assets/Data/Materials/Cells/{camelCaseName}.mat`
- Add enum value to `CellType` in `Assets/Scripts/Core/CellManager.cs`
- Add `Resources.Load` line in `CellManager.Awake()`
- Add cell-specific upgrades to `Assets/Scripts/Data/CellUpgradeData.cs` with id prefix matching lowercase cell type enum

**New Upgrade (Neurosphere Tree):**
- Add `UpgradeDef` entry in `Assets/Scripts/Data/MutationData.cs` -> `CreateCatalog()`
- Set `Position` for tree layout placement (percent-based x,y)
- Reference the upgrade id in cell scripts via `Upgrades.Instance.IsUpgradeUnlocked("id")`

**New Upgrade (Lineage Tree):**
- Add `UpgradeDef` entry in `Assets/Scripts/Data/LineageUpgradeData.cs` -> `CreateCatalog()`

**New UI Panel:**
- Create UXML: `Assets/Scenes/UI/{PanelName}.uxml`
- Create USS: `Assets/Scenes/UI/{panel-name}.uss`
- Create View: `Assets/Scripts/UI/{PanelName}View.cs` (MonoBehaviour, query elements from UIDocument)
- Create Presenter: `Assets/Scripts/UI/{PanelName}Presenter.cs` (plain C# class, receives View in constructor)

**New Core System:**
- Add to `Assets/Scripts/Core/` as a MonoBehaviour singleton
- Follow the pattern: `public static {ClassName} Instance { get; private set; }` set in `Awake()` with duplicate destruction

**Shared Types / Interfaces:**
- Add to `Assets/Scripts/Interfaces/`

## Special Directories

**`Assets/Resources/`:**
- Purpose: Assets loadable at runtime via `Resources.Load()`
- Generated: No (manually populated)
- Committed: Yes
- Note: Only `Cells/` subfolder has content. `Upgrades/` subfolder exists but is empty.

**`Assets/_Recovery/`:**
- Purpose: Scene backup files (old scene versions)
- Generated: Manual backups
- Committed: Yes
- Note: Contains `0.unity` and `0 (1).unity` -- can be cleaned up

**`Assets/TextMesh Pro/`:**
- Purpose: TextMesh Pro package assets (imported via Unity)
- Generated: Package import
- Committed: Yes
- Note: Third-party, do not modify

**`Assets/TutorialInfo/`:**
- Purpose: Unity template boilerplate
- Generated: Unity project template
- Committed: Yes
- Note: Unused, can be removed

**`Builds/`:**
- Purpose: Standalone build output
- Generated: Yes (Unity build pipeline)
- Committed: Yes (should likely be gitignored)

---

*Structure analysis: 2026-03-27*
