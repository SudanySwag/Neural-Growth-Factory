# Technology Stack

**Analysis Date:** 2026-03-27

## Languages

**Primary:**
- C# - All game logic, UI, and data scripts (`Assets/Scripts/**/*.cs`)

**Secondary:**
- UXML - UI Toolkit layout definitions (`Assets/Scenes/UI/*.uxml`)
- USS - UI Toolkit stylesheets (`Assets/Scenes/UI/*.uss`)
- ShaderLab/HLSL - TextMesh Pro shaders only (`Assets/TextMesh Pro/Shaders/`)

## Runtime

**Engine:**
- Unity 6000.2.5f1 (Unity 6 LTS line)
- Editor Revision: `43d04cd1df69`
- Version file: `ProjectSettings/ProjectVersion.txt`

**Scripting Backend:**
- Mono (default for editor; no IL2CPP override detected in ProjectSettings)

**Color Space:**
- Linear (`m_ActiveColorSpace: 1` in `ProjectSettings/ProjectSettings.asset`)

## Render Pipeline

**Pipeline:** Universal Render Pipeline (URP) 17.2.0
- URP Global Settings: `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`
- PC Renderer: `Assets/Settings/PC_Renderer.asset`
- PC RP Asset: `Assets/Settings/PC_RPAsset.asset`
- Mobile Renderer: `Assets/Settings/Mobile_Renderer.asset`
- Mobile RP Asset: `Assets/Settings/Mobile_RPAsset.asset`
- Volume Profile: `Assets/Settings/DefaultVolumeProfile.asset`
- Sample Scene Profile: `Assets/Settings/SampleSceneProfile.asset`

**Quality Tiers:**
- Mobile (index 0)
- PC (index 1, current default)

## Frameworks & Systems

**UI System:**
- UI Toolkit (primary) - All game UI uses `UIDocument` + UXML/USS
  - UXML files: `Assets/Scenes/UI/Counter.uxml`, `Assets/Scenes/UI/Sidebar.uxml`, `Assets/Scenes/UI/Upgrades.uxml`, `Assets/Scenes/UI/Lineage.uxml`, `Assets/Scenes/UI/CellUpgradePopup.uxml`
  - USS files: `Assets/Scenes/UI/counter.uss`, `Assets/Scenes/UI/sidebar.uss`, `Assets/Scenes/UI/upgrades.uss`, `Assets/Scenes/UI/cell-upgrade-popup.uss`
- uGUI (`com.unity.ugui` 2.0.0) - Present as dependency but not actively used in scripts
- TextMesh Pro - Font rendering (`Assets/TextMesh Pro/`)

**Input System:**
- New Input System (`com.unity.inputsystem` 1.14.2)
- Input Actions Asset: `Assets/InputSystem_Actions.inputactions`
- Used directly via `Mouse.current` in `Assets/Scripts/Core/Camera.cs` (no PlayerInput component pattern)

**Physics:**
- 3D Physics (`com.unity.modules.physics`) - SphereColliders on cells, raycasting for click detection

**AI Navigation:**
- `com.unity.ai.navigation` 2.0.9 - Present in manifest, not yet used in scripts

## Key Dependencies

**Direct (from `Packages/manifest.json`):**

| Package | Version | Purpose |
|---------|---------|---------|
| `com.unity.render-pipelines.universal` | 17.2.0 | URP rendering |
| `com.unity.inputsystem` | 1.14.2 | New Input System |
| `com.unity.ai.navigation` | 2.0.9 | AI NavMesh (unused in code) |
| `com.unity.ugui` | 2.0.0 | Legacy UI (TextMesh Pro dependency) |
| `com.unity.recorder` | 5.1.5 | Video/image recording |
| `com.unity.timeline` | 1.8.9 | Timeline sequences |
| `com.unity.visualscripting` | 1.9.9 | Visual scripting (unused in code) |
| `com.unity.test-framework` | 1.5.1 | NUnit test runner |
| `com.unity.collab-proxy` | 2.11.2 | Unity Collaborate |
| `com.unity.ide.visualstudio` | 2.0.23 | VS IDE integration |
| `com.spoiledcat.git` | 1.0.62 | Git integration for Unity |
| `com.unity.multiplayer.center` | 1.0.0 | Multiplayer hub (unused) |

**Transitive (resolved in `Packages/packages-lock.json`):**

| Package | Version | Pulled By |
|---------|---------|-----------|
| `com.unity.burst` | 1.8.24 | URP / Collections |
| `com.unity.collections` | 2.5.7 | URP |
| `com.unity.mathematics` | 1.3.2 | Burst / Collections |

**Scoped Registries:**
- `spoiledcat` registry at `https://registry.spoiledcat.com` (for `com.spoiledcat.git`)

## Build Configuration

**Scenes in Build:**
- `Assets/Scenes/SampleScene.unity` (configured in `ProjectSettings/EditorBuildSettings.asset`)
- Note: Active development scene is `Assets/Scenes/Neurosphere.unity` but it is NOT in the build settings

**Application Info:**
- Product Name: "My project" (default, not renamed)
- Bundle Version: 0.1.0
- Company: DefaultCompany
- App ID (Standalone): `com.Unity-Technologies.com.unity.template.urp-blank` (template default)

**Default Resolution:**
- 1024x768, native resolution enabled
- Fullscreen Mode: 1 (Fullscreen Window)

## Platform Requirements

**Development:**
- Unity 6000.2.5f1 editor
- Windows 10+ (current dev platform)

**Target Platforms:**
- Standalone (PC) - Primary target based on settings and input handling (Mouse-based)
- Mobile renderer assets exist suggesting future Android/iOS target
- No platform-specific `#if` directives found in scripts

## Assembly Structure

- No `.asmdef` files - All scripts compile into the default `Assembly-CSharp`
- Editor scripts directory exists at `Assets/Editor/` (currently empty)

---

*Stack analysis: 2026-03-27*
