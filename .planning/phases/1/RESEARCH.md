# Phase 1: Intro Cutscene - Research

**Researched:** 2026-03-27
**Domain:** Unity 6 UI Toolkit cutscene / coroutine sequencing
**Confidence:** HIGH

## Summary

The intro cutscene needs to suppress all gameplay systems (cells, UI) during a linear sequence, then hand off to normal gameplay. The project has no existing game state machine -- systems self-initialize in `Awake()`/`Start()`/`OnEnable()` and cells placed in the scene register themselves immediately via `Cell.Start()`. The cleanest integration point is a new `IntroCutscene` MonoBehaviour that (1) disables the initial cell GameObject and all UI documents on `Awake()`, (2) runs a coroutine-based sequence, and (3) re-enables everything when done.

For the custom cursor, `Cursor.SetCursor()` cannot animate opacity -- it is a hardware cursor swap with no blend support. The project should use a UI Toolkit element (a `VisualElement` with the cursor image as background) that tracks `Mouse.current.position` each frame. USS transitions handle the fade-in. The system cursor is hidden via `Cursor.visible = false`.

**Primary recommendation:** Use a single coroutine state machine in a new `IntroCutscene.cs` MonoBehaviour. Use UI Toolkit for the cursor overlay and caption. Do not use Timeline -- it is overkill for this linear 3-step sequence and adds coupling complexity.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Show uninteractable empty background for a few seconds at game start
- Fade in a custom mouse cursor (not system cursor)
- Hide default system cursor during cutscene
- Caption "Look into the void" appears when cursor passes near screen center
- Click while caption visible advances to gameplay
- No cells spawned during intro
- All gameplay UI hidden during cutscene
- Must use UI Toolkit for overlay (project convention)
- Must integrate with existing Neurosphere.unity scene and MonoBehaviour architecture

### Claude's Discretion
- Implementation approach (coroutine vs Timeline vs state machine)
- Exact timing values for delays and fade durations
- Definition of "near center" threshold
- Whether cutscene replays each session or has skip logic
- Cursor art style and size
- Caption styling

### Deferred Ideas (OUT OF SCOPE)
- None specified
</user_constraints>

## How Systems Start Up (Integration Analysis)

Understanding the boot sequence is critical for suppressing gameplay during the cutscene.

### Scene Load Order (Neurosphere.unity)

| Order | System | Lifecycle | What It Does | How to Suppress |
|-------|--------|-----------|-------------|-----------------|
| 1 | `CellManager` | `Awake()` | Singleton init, loads prefabs from Resources | Safe to leave running -- it only provides prefabs |
| 2 | `Upgrades` | `Awake()` | Singleton init, creates catalogs, calls `InitializeUI()` | Disable the UIDocument to hide UI; singleton can live |
| 3 | `CameraHandler` | `Awake()` + `Update()` | Orthographic camera setup, processes mouse input every frame | Disable the component to block pan/zoom/click during cutscene |
| 4 | Initial TPC cell | `Start()` | Calls `CellManager.Instance.RegisterCell(this)` | **Disable the cell GameObject before its `Start()` runs** |
| 5 | `SidebarController` | `Start()` | Queries UIDocument, hides sidebar, subscribes to `CellBirth` | Hidden by default (already sets `display: none`), but disable UIDocument to be safe |
| 6 | `Counter` | `OnEnable()` | Queries UIDocument, slides off-screen initially | Hidden by default (top: -150%), but disable UIDocument |

### Critical Finding: Cell Suppression Strategy

The initial TPC cell is a **prefab instance placed directly in the scene hierarchy** (not spawned by code). Its `Cell.Start()` calls `CellManager.Instance.RegisterCell(this)`, which fires `CellBirth` and makes it visible.

**Approach:** The `IntroCutscene` script must run in `Awake()` (before `Start()`) to find and disable the TPC cell's GameObject. After the cutscene completes, re-enable it so `Start()` fires naturally.

```csharp
// In IntroCutscene.Awake(), before Cell.Start() runs:
initialCell = FindAnyObjectByType<TotipotentStemCell>(FindObjectsInactive.Include);
if (initialCell != null)
    initialCell.gameObject.SetActive(false);
```

### UIDocument Suppression

The scene has **4 UIDocument components** (sorting orders -1, 0, 1, and another -1). Rather than finding and disabling each one, the cutscene overlay can simply be a full-screen opaque black panel at sorting order 100 that covers everything, then fades out when gameplay begins. This avoids needing to track every UIDocument.

However, `CameraHandler` must still be disabled to prevent the player from panning/zooming/clicking during the cutscene:

```csharp
cameraHandler = FindAnyObjectByType<CameraHandler>();
if (cameraHandler != null)
    cameraHandler.enabled = false;
```

## Architecture Patterns

### Recommended Approach: Coroutine State Machine

A single `IntroCutscene` MonoBehaviour with a coroutine that sequences the intro:

```csharp
public class IntroCutscene : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private float cursorFadeDuration = 1.5f;

    [Header("Detection")]
    [SerializeField] private float centerThresholdPixels = 80f;

    [Header("References")]
    [SerializeField] private UIDocument overlayDocument;

    private TotipotentStemCell initialCell;
    private CameraHandler cameraHandler;

    void Awake()
    {
        // Suppress gameplay systems BEFORE Start() runs
        initialCell = FindAnyObjectByType<TotipotentStemCell>(FindObjectsInactive.Include);
        if (initialCell != null)
            initialCell.gameObject.SetActive(false);

        cameraHandler = FindAnyObjectByType<CameraHandler>();
        if (cameraHandler != null)
            cameraHandler.enabled = false;

        Cursor.visible = false;
    }

    void Start()
    {
        StartCoroutine(CutsceneSequence());
    }

    private IEnumerator CutsceneSequence()
    {
        // Phase 1: Empty void (wait)
        yield return new WaitForSeconds(initialDelay);

        // Phase 2: Fade in custom cursor
        // Set cursor element opacity from 0 to 1 over cursorFadeDuration
        yield return FadeCursorIn();

        // Phase 3: Wait for center hover + click
        yield return WaitForCenterClickSequence();

        // Phase 4: Transition to gameplay
        yield return TransitionToGameplay();
    }
}
```

### Why NOT Timeline

Timeline (com.unity.timeline 1.8.9) is installed but inappropriate here because:
1. **No animation clips needed** -- the sequence is logic-driven (detect mouse position, wait for click), not keyframe-driven
2. **Signal/marker overhead** -- Timeline needs SignalReceivers, Playables, and track bindings for simple "wait, then do X"
3. **Mouse interaction** -- Timeline cannot natively wait for "cursor near center" or "user clicked"; you would need custom PlayableTrack or signal-based callbacks that are harder to debug than coroutine `yield` statements
4. **Project pattern** -- the codebase uses coroutines for all sequenced behavior (Cell.DivideRoutine, DifferentiateRoutine, ApoptosisRoutine). A coroutine-based cutscene is consistent.

### Recommended Project Structure

```
Assets/Scripts/Core/
    IntroCutscene.cs          # Coroutine state machine, suppresses systems
Assets/Scenes/UI/
    IntroCutscene.uxml        # Overlay: black panel + cursor element + caption
    intro-cutscene.uss        # Styles: fade transitions, cursor positioning
Assets/Textures/
    cursor.png                # Custom cursor texture (32x32 or 64x64)
```

### Anti-Patterns to Avoid
- **Do not use a separate "intro scene"** -- adds scene loading complexity and breaks the single-scene architecture
- **Do not use Timeline** for logic-driven sequences with input detection
- **Do not use `Cursor.SetCursor()`** for animated cursor -- it cannot fade, and swapping between null and a texture causes a visual pop
- **Do not modify existing scripts** to add `if (cutsceneActive)` checks -- use enable/disable on components and GameObjects instead

## Custom Cursor Implementation

### Approach: UI Toolkit Element Tracking Mouse

Use a `VisualElement` with the cursor texture as `background-image`, positioned absolutely in the overlay panel. Each frame, update its `left`/`top` style properties from `Mouse.current.position`.

**Why this approach over `Cursor.SetCursor()`:**
- `Cursor.SetCursor()` cannot animate opacity (hardware cursor, binary swap)
- UI Toolkit element supports USS `transition-property: opacity` for smooth fade-in
- Full control over size, color tint, and effects
- Consistent with project's UI Toolkit usage

```csharp
// In Update() during cutscene cursor phase:
Vector2 mousePos = Mouse.current.position.ReadValue();

// Screen space is bottom-up, UI Toolkit is top-down
float uiTop = Screen.height - mousePos.y;

cursorElement.style.left = mousePos.x - cursorSize / 2f;
cursorElement.style.top = uiTop - cursorSize / 2f;
```

**Important coordinate conversion:** `Mouse.current.position` uses bottom-left origin. UI Toolkit uses top-left origin. Flip Y: `uiTop = Screen.height - mousePos.y`. The project's `CellUpgradePopupView` already handles this conversion (verified in codebase conventions: "Flip Y: screen space is bottom-up, UI Toolkit expects top-down").

### USS for Cursor Fade-In

```css
#CustomCursor {
    position: absolute;
    width: 32px;
    height: 32px;
    background-image: url("project://database/Assets/Textures/cursor.png");
    opacity: 0;
    transition-property: opacity;
    transition-duration: 1.5s;
    transition-timing-function: ease-in;
}

#CustomCursor.visible {
    opacity: 1;
}
```

Trigger fade by adding class: `cursorElement.AddToClassList("visible");`

## Screen Center Detection

Simple distance check -- no physics or raycasting needed:

```csharp
private bool IsCursorNearCenter()
{
    Vector2 mousePos = Mouse.current.position.ReadValue();
    Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
    return Vector2.Distance(mousePos, screenCenter) < centerThresholdPixels;
}
```

**Threshold recommendation:** 80-100 pixels. This gives a roughly 160-200px diameter "hot zone" at screen center, which feels natural -- large enough to find without being trivially easy.

## UI Toolkit Overlay (UXML)

Following the project's existing UXML patterns (PascalCase file, kebab-case USS):

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="project://database/Assets/Scenes/UI/intro-cutscene.uss" />
    <ui:VisualElement name="CutsceneOverlay" class="cutscene-overlay">
        <ui:VisualElement name="CustomCursor" class="custom-cursor" />
        <ui:Label name="Caption" text="Look into the void" class="caption" />
    </ui:VisualElement>
</ui:UXML>
```

The overlay UIDocument should use **sorting order 100** (above all existing UI, which uses -1 to 1) so it covers everything during the cutscene.

### Caption Styling

```css
.caption {
    position: absolute;
    align-self: center;
    color: rgba(200, 200, 200, 0.9);
    font-size: 24px;
    opacity: 0;
    transition-property: opacity;
    transition-duration: 0.8s;
    transition-timing-function: ease-in;
}

.caption.visible {
    opacity: 1;
}
```

Position the caption below the cursor element dynamically:
```csharp
captionElement.style.left = mousePos.x - captionWidth / 2f;
captionElement.style.top = uiTop + cursorSize / 2f + 10f; // 10px below cursor
```

## Gameplay Transition

When the player clicks while the caption is visible:

1. Fade out the overlay (opacity 0 on the root, with USS transition)
2. Re-enable the initial TPC cell: `initialCell.gameObject.SetActive(true);`
3. Re-enable CameraHandler: `cameraHandler.enabled = true;`
4. Restore system cursor (optional, or keep custom cursor): `Cursor.visible = true;`
5. Destroy or disable the IntroCutscene component: `Destroy(gameObject);` or `enabled = false;`

```csharp
private IEnumerator TransitionToGameplay()
{
    // Fade out overlay
    overlayRoot.AddToClassList("fade-out");
    yield return new WaitForSeconds(1f); // match USS transition duration

    // Re-enable gameplay
    if (initialCell != null)
        initialCell.gameObject.SetActive(true);
    if (cameraHandler != null)
        cameraHandler.enabled = true;

    Cursor.visible = true;
    overlayDocument.gameObject.SetActive(false);
    enabled = false;
}
```

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Opacity fade animation | Manual lerp in Update() | USS `transition-property: opacity` | Already used in project (counter.uss uses transitions); hardware-accelerated, no geometry regen |
| Mouse position tracking | Custom input polling system | `Mouse.current.position.ReadValue()` | Project already uses this pattern in CameraHandler |
| Sequencing | Custom state machine enum | Coroutine `yield return` | Project pattern (Cell division uses coroutines); simpler, readable |
| Full-screen overlay | Canvas + Image (uGUI) | UI Toolkit VisualElement | Project convention is UI Toolkit exclusively |

## Common Pitfalls

### Pitfall 1: Cell.Start() Runs Before Cutscene Suppression
**What goes wrong:** If IntroCutscene uses `Start()` to disable the TPC cell, the cell's own `Start()` may have already run (registration, visibility).
**Why it happens:** Unity's `Start()` execution order between MonoBehaviours on different GameObjects is not guaranteed.
**How to avoid:** Use `Awake()` in IntroCutscene to disable the cell GameObject. `Awake()` always runs before any `Start()`. Alternatively, set Script Execution Order in Project Settings.
**Warning signs:** Cell appears briefly on first frame, then disappears.

### Pitfall 2: UI Toolkit Coordinate Flip
**What goes wrong:** Custom cursor appears at the wrong vertical position (mirrored).
**Why it happens:** `Mouse.current.position` uses bottom-left origin; UI Toolkit uses top-left origin.
**How to avoid:** Always convert: `uiY = Screen.height - mousePos.y`. The project already documents this convention.
**Warning signs:** Cursor moves in the opposite vertical direction from the mouse.

### Pitfall 3: Cursor.visible in Editor
**What goes wrong:** System cursor disappears in the Scene view or becomes unfindable after stopping play mode.
**Why it happens:** `Cursor.visible = false` affects the entire application, including the editor.
**How to avoid:** Always restore `Cursor.visible = true` in `OnDisable()` or `OnDestroy()` as a safety net:
```csharp
void OnDisable()
{
    Cursor.visible = true;
}
```
**Warning signs:** Cannot see cursor after stopping play mode.

### Pitfall 4: USS Transitions Not Firing
**What goes wrong:** Adding a class in the same frame as element creation does not trigger the transition.
**Why it happens:** USS transitions interpolate from the *computed* style to the *new* style. If both are set in the same frame, there is no change to interpolate.
**How to avoid:** Wait one frame (`yield return null`) after creating/showing the element before adding the transition class.
**Warning signs:** Element appears at full opacity instantly instead of fading in.

### Pitfall 5: UIDocument Sorting Order
**What goes wrong:** Cutscene overlay appears behind gameplay UI.
**Why it happens:** Existing UIDocuments use sorting orders -1, 0, 1. If the cutscene uses 0 or 1, it may render below some elements.
**How to avoid:** Use sorting order 100 for the cutscene overlay UIDocument.
**Warning signs:** Gameplay UI elements (counter, sidebar) visible on top of cutscene.

## Code Examples

### Complete IntroCutscene.cs Skeleton
```csharp
// Source: Project architecture analysis + Unity docs
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class IntroCutscene : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private float cursorFadeDuration = 1.5f;

    [Header("Detection")]
    [SerializeField] private float centerThresholdPixels = 80f;

    [Header("UI")]
    [SerializeField] private UIDocument overlayDocument;

    private VisualElement overlayRoot;
    private VisualElement cursorElement;
    private Label captionElement;

    private TotipotentStemCell initialCell;
    private CameraHandler cameraHandler;

    void Awake()
    {
        // Suppress before any Start() calls
        initialCell = FindAnyObjectByType<TotipotentStemCell>(FindObjectsInactive.Include);
        if (initialCell != null)
            initialCell.gameObject.SetActive(false);

        cameraHandler = FindAnyObjectByType<CameraHandler>();
        if (cameraHandler != null)
            cameraHandler.enabled = false;

        Cursor.visible = false;
    }

    void Start()
    {
        overlayRoot = overlayDocument.rootVisualElement;
        cursorElement = overlayRoot.Q<VisualElement>("CustomCursor");
        captionElement = overlayRoot.Q<Label>("Caption");

        StartCoroutine(CutsceneSequence());
    }

    void OnDisable()
    {
        Cursor.visible = true; // Safety restore
    }

    void Update()
    {
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || cursorElement == null) return;

        Vector2 pos = mouse.position.ReadValue();
        float uiTop = Screen.height - pos.y;

        cursorElement.style.left = pos.x - 16f; // half of 32px cursor
        cursorElement.style.top = uiTop - 16f;
    }

    private bool IsCursorNearCenter()
    {
        Vector2 pos = Mouse.current.position.ReadValue();
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        return Vector2.Distance(pos, center) < centerThresholdPixels;
    }

    private IEnumerator CutsceneSequence()
    {
        // Phase 1: Empty void
        yield return new WaitForSeconds(initialDelay);

        // Phase 2: Fade in cursor (wait a frame for USS transition to work)
        yield return null;
        cursorElement.AddToClassList("visible");
        yield return new WaitForSeconds(cursorFadeDuration);

        // Phase 3: Wait for center hover, show caption, wait for click
        bool captionShown = false;
        bool clicked = false;
        while (!clicked)
        {
            bool nearCenter = IsCursorNearCenter();

            if (nearCenter && !captionShown)
            {
                captionElement.AddToClassList("visible");
                captionShown = true;
            }
            else if (!nearCenter && captionShown)
            {
                captionElement.RemoveFromClassList("visible");
                captionShown = false;
            }

            if (captionShown && Mouse.current.leftButton.wasPressedThisFrame)
                clicked = true;

            yield return null;
        }

        // Phase 4: Transition to gameplay
        yield return TransitionToGameplay();
    }

    private IEnumerator TransitionToGameplay()
    {
        overlayRoot.AddToClassList("fade-out");
        yield return new WaitForSeconds(1f);

        if (initialCell != null)
            initialCell.gameObject.SetActive(true);
        if (cameraHandler != null)
            cameraHandler.enabled = true;

        Cursor.visible = true;
        overlayDocument.gameObject.SetActive(false);
        enabled = false;
    }
}
```

### Complete USS (intro-cutscene.uss)
```css
/* Source: Project USS patterns (counter.uss, cell-upgrade-popup.uss) */

.cutscene-overlay {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 1);
    transition-property: opacity;
    transition-duration: 1s;
    transition-timing-function: ease-out;
}

.cutscene-overlay.fade-out {
    opacity: 0;
}

.custom-cursor {
    position: absolute;
    width: 32px;
    height: 32px;
    opacity: 0;
    transition-property: opacity;
    transition-duration: 1.5s;
    transition-timing-function: ease-in;
    /* background-image set to cursor texture */
}

.custom-cursor.visible {
    opacity: 1;
}

.caption {
    position: absolute;
    align-self: center;
    font-size: 24px;
    color: rgba(200, 200, 200, 0.9);
    opacity: 0;
    transition-property: opacity;
    transition-duration: 0.8s;
    transition-timing-function: ease-in;
    -unity-text-align: middle-center;
}

.caption.visible {
    opacity: 1;
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `Cursor.SetCursor()` for custom cursors | UI element tracking mouse position | N/A (both still valid) | UI element approach allows animation, scaling, effects |
| uGUI Canvas overlay | UI Toolkit VisualElement overlay | Unity 2021+ | Project already uses UI Toolkit exclusively |
| `FindObjectOfType<T>()` | `FindAnyObjectByType<T>()` | Unity 2022+ | Non-deterministic but faster; use for singletons |

## Open Questions

1. **Cursor art asset**
   - What we know: Needs to be a PNG texture, 32x32 or 64x64 recommended for crisp rendering
   - What's unclear: No cursor texture exists in the project yet
   - Recommendation: Create a simple circular or crosshair cursor texture, or use a placeholder white circle. Set as `background-image` in USS.

2. **Caption positioning relative to cursor**
   - What we know: Caption should appear "underneath the cursor" per requirements
   - What's unclear: Whether it should follow the cursor or appear at a fixed position near center
   - Recommendation: Position it below the cursor element, updating each frame alongside cursor position. Feels more immersive.

3. **Replay behavior**
   - What we know: CONTEXT.md mentions "can be skipped or only plays once per session"
   - What's unclear: No save system exists; game state resets every play session
   - Recommendation: Always play on scene load (since state resets anyway). No skip button for first implementation -- the interaction IS the skip (just move to center and click).

## Sources

### Primary (HIGH confidence)
- Project source code: CellManager.cs, Camera.cs, SidebarController.cs, Counter.cs, Upgrades.cs, Cell.cs, TotipotentStemCell.cs, NeuralStemCell.cs, GanglionMotherCell.cs
- Project codebase analysis: .planning/codebase/ARCHITECTURE.md, CONVENTIONS.md, STACK.md
- [Unity Cursor.SetCursor docs](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Cursor.SetCursor.html)
- [Unity Cursor.visible docs](https://docs.unity3d.com/ScriptReference/Cursor-visible.html)
- [Unity USS Transitions manual](https://docs.unity3d.com/Manual//UIE-Transitions.html)

### Secondary (MEDIUM confidence)
- [Unity Discussions: Custom cursor scale in Unity 6](https://discussions.unity.com/t/custom-cursor-scale-in-unity-6/1608411) - confirms SetCursor limitations
- [Unity Discussions: USS transition animations](https://discussions.unity.com/t/issue-with-ui-toolkit-transition-animations/1513067) - confirms opacity transition approach

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - using only existing project dependencies (UI Toolkit, Input System, coroutines)
- Architecture: HIGH - coroutine pattern verified against existing codebase conventions
- Pitfalls: HIGH - identified from direct source code analysis of startup order and coordinate systems

**Research date:** 2026-03-27
**Valid until:** 2026-04-27 (stable Unity 6 APIs, project architecture unlikely to change)
