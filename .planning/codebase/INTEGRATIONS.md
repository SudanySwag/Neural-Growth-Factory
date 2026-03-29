# External Integrations

**Analysis Date:** 2026-03-27

## APIs & External Services

**None.** This is a fully offline, single-player Unity game with no external API calls, no network requests, and no backend services. No `UnityWebRequest`, REST client, or HTTP usage found in any script.

## Data Storage

**Databases:**
- None. All game state is held in memory at runtime.

**Persistence:**
- No save/load system detected. No `PlayerPrefs`, `JsonUtility.ToJson`, or file I/O usage in scripts.
- All upgrade progress and cell counts reset on scene reload.

**File Storage:**
- Resources folder for prefab loading: `Assets/Resources/Cells/` (6 cell prefabs)
- Resources folder for upgrades: `Assets/Resources/Upgrades/` (empty)

**Caching:**
- None

## Third-Party Packages

**SpoiledCat Git (`com.spoiledcat.git` 1.0.62):**
- Editor-only Git integration plugin
- Source: Custom scoped registry at `https://registry.spoiledcat.com`
- Purpose: Git workflow within Unity Editor
- No runtime impact

## Unity First-Party Packages (Notable)

**Active in Code:**

| Package | How Used |
|---------|----------|
| `com.unity.inputsystem` 1.14.2 | `Mouse.current` for click/pan/zoom in `Assets/Scripts/Core/Camera.cs` |
| `com.unity.render-pipelines.universal` 17.2.0 | URP rendering with Mobile and PC quality tiers |
| `com.unity.modules.uielements` | UI Toolkit for all game UI (upgrade trees, sidebar, counter, popup) |
| `com.unity.modules.physics` | SphereColliders on cells, raycasting for click detection |

**Present but Unused in Code:**

| Package | Status |
|---------|--------|
| `com.unity.ai.navigation` 2.0.9 | In manifest, no NavMesh usage in scripts |
| `com.unity.visualscripting` 1.9.9 | In manifest, no visual script graphs found |
| `com.unity.recorder` 5.1.5 | Editor tool for recording, no script references |
| `com.unity.timeline` 1.8.9 | In manifest, `TimelineSettings.asset` exists but no Timeline usage in scripts |
| `com.unity.multiplayer.center` 1.0.0 | In manifest, no multiplayer code |
| `com.unity.ugui` 2.0.0 | Dependency for TextMesh Pro, not used directly |

## Authentication & Identity

- Not applicable. Offline single-player game.

## Monitoring & Observability

**Error Tracking:**
- None. Uses `Debug.Log`, `Debug.LogError`, `Debug.LogWarning` for console output only.

**Analytics:**
- Unity Analytics module is included (`com.unity.modules.unityanalytics`) but `submitAnalytics: 1` is the default setting. No custom analytics events in code.

## CI/CD & Deployment

**Hosting:**
- Not deployed. Local development only.

**CI Pipeline:**
- None detected. No `.github/workflows/`, no `Jenkinsfile`, no CI config files.

**Version Control:**
- Git (current branch: `upgrades`, main branch: `master`)
- SpoiledCat Git Unity integration for editor workflow

## Environment Configuration

**Required env vars:**
- None. No `.env` files or environment variable reads in code.

**Secrets:**
- None. No API keys, tokens, or credentials.

## Webhooks & Callbacks

**Incoming:**
- None

**Outgoing:**
- None

## Asset Store Packages

- None detected. No Asset Store package references in manifest or evidence of imported `.unitypackage` assets (besides the bundled TextMesh Pro and TutorialInfo from the URP template).

## Custom Package References

- None. No `file:` or `git:` package references in `Packages/manifest.json`.

## Font Assets

**TextMesh Pro:**
- Full TextMesh Pro examples and extras imported at `Assets/TextMesh Pro/`
- Custom fonts directory at `Assets/Fonts/` (likely contains project-specific fonts)

## Material Assets

**Cell Materials:**
- Location: `Assets/Data/Materials/Cells/`
- One material per cell type: `totipotentStemCell.mat`, `neuralStemCell.mat`, `radialGlialCell.mat`, `ganglionMotherCell.mat`, `neuroblast.mat`
- Shared base material: `Cell.mat`
- Unknown cell material: `unknownCell.mat`

**Other Materials:**
- Lineage background: `Assets/Data/Materials/Lineage Background.mat`

---

*Integration audit: 2026-03-27*
