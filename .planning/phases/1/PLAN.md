---
phase: 01-intro-cutscene
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - Assets/Scripts/Core/IntroCutscene.cs
  - Assets/Resources/UI/IntroCutscene.uxml
  - Assets/Resources/UI/intro-cutscene.uss
autonomous: false
requirements:
  - INTRO-01
  - INTRO-02
  - INTRO-03
  - INTRO-04
  - INTRO-05

must_haves:
  truths:
    - "Game starts with black screen, no cells visible, no gameplay UI visible"
    - "Custom cursor fades in after a few seconds of void"
    - "System cursor is hidden throughout the cutscene"
    - "Caption 'Look into the void' appears when cursor moves near screen center"
    - "Caption disappears when cursor leaves center zone"
    - "Clicking while caption is visible transitions to normal gameplay"
    - "Pressing any key or clicking during the void/wait phases skips directly to gameplay transition"
    - "After transition: initial TPC cell is active, CameraHandler processes input, system cursor restored"
  artifacts:
    - path: "Assets/Scripts/Core/IntroCutscene.cs"
      provides: "Self-bootstrapping coroutine state machine that sequences the entire intro"
      min_lines: 100
    - path: "Assets/Resources/UI/IntroCutscene.uxml"
      provides: "Full-screen overlay with cursor element and caption label"
    - path: "Assets/Resources/UI/intro-cutscene.uss"
      provides: "Styles for overlay, cursor fade, caption fade, and transition-out"
  key_links:
    - from: "IntroCutscene.AutoInit()"
      to: "TotipotentStemCell GameObject"
      via: "FindAnyObjectByType then SetActive(false)"
      pattern: "FindAnyObjectByType.*SetActive.*false"
    - from: "IntroCutscene.AutoInit()"
      to: "CameraHandler component"
      via: "FindAnyObjectByType then enabled = false"
      pattern: "cameraHandler.*enabled.*false"
    - from: "IntroCutscene.Update()"
      to: "Mouse.current.position"
      via: "ReadValue() mapped to UI Toolkit coordinates"
      pattern: "Screen\\.height.*pos\\.y"
    - from: "IntroCutscene.TransitionToGameplay()"
      to: "initialCell and cameraHandler"
      via: "Re-enable cell GameObject and CameraHandler component"
      pattern: "SetActive.*true.*enabled.*true"
---

<objective>
Add an intro cutscene that plays on game start: empty void background for a few seconds, fade in a custom mouse cursor, show "Look into the void" when cursor passes screen center, advance to gameplay on click. Pressing any key or clicking during the void/cursor-fade phases skips directly to gameplay.

Purpose: Create an atmospheric first-impression moment before the player begins interacting with cells.
Output: A self-contained, self-bootstrapping IntroCutscene system (MonoBehaviour + UXML + USS) that suppresses gameplay during the intro and cleanly hands off to normal play. No manual scene editing required.
</objective>

<execution_context>
@.planning/phases/1/CONTEXT.md
@.planning/phases/1/RESEARCH.md
@.planning/codebase/ARCHITECTURE.md
@.planning/codebase/CONVENTIONS.md
@.planning/codebase/STRUCTURE.md
</execution_context>

<context>
@Assets/Scripts/Core/Camera.cs
@Assets/Scripts/Core/CellManager.cs

<interfaces>
<!-- Key types and contracts the executor needs -->

From Assets/Scripts/Core/Camera.cs:
```csharp
public class CameraHandler : MonoBehaviour
{
    public static event System.Action ClickedNothing;
    // Processes clicks, pan, zoom every Update() -- must be disabled during cutscene
}
```

From Assets/Scripts/Cells/TotipotentStemCell.cs:
```csharp
public class TotipotentStemCell : Cell
{
    // Initial cell placed in scene hierarchy as prefab instance
    // Its Start() calls CellManager.Instance.RegisterCell(this) which fires CellBirth
    // Must disable its GameObject in Awake() before Start() runs
}
```

From Assets/Scripts/Core/CellManager.cs:
```csharp
public class CellManager : MonoBehaviour
{
    public static CellManager Instance { get; private set; }
    public delegate void OnCellBirth(CellType cellType);
    public static event OnCellBirth CellBirth;
    // Singleton -- safe to leave running, only provides prefabs
}
```
</interfaces>

<notes>
**Why Upgrades/Sidebar/Counter don't need explicit suppression:**
These UI systems are hidden by default until gameplay thresholds are reached. SidebarController.Start() sets its panel to `display: none` and only reveals it after enough CellBirth events. Counter starts off-screen (top: -150%) and slides in on first CellBirth. Upgrades UI is hidden until cell selection. Since the initial TPC cell is deactivated (preventing CellBirth from firing), none of these systems will show anything during the cutscene. The opaque black overlay at sorting order 100 provides an additional safety layer covering all existing UIDocuments (sorting orders -1 to 1).
</notes>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Create cutscene UI assets and IntroCutscene MonoBehaviour with self-bootstrap</name>
  <files>
    Assets/Resources/UI/IntroCutscene.uxml
    Assets/Resources/UI/intro-cutscene.uss
    Assets/Scripts/Core/IntroCutscene.cs
  </files>
  <action>
Create all three files that compose the intro cutscene system. All files are new -- no modifications to existing code. The UXML and USS go in `Assets/Resources/UI/` (not `Assets/Scenes/UI/`) so the bootstrap can load them via `Resources.Load`.

Create the `Assets/Resources/UI/` directory if it does not exist.

**1. Create `Assets/Resources/UI/intro-cutscene.uss`:**

```css
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
    border-radius: 16px;
    background-color: rgba(255, 255, 255, 0.9);
    opacity: 0;
    transition-property: opacity;
    transition-duration: 1.5s;
    transition-timing-function: ease-in;
}

.custom-cursor.visible {
    opacity: 1;
}

.caption {
    position: absolute;
    font-size: 24px;
    color: rgba(200, 200, 200, 0.9);
    opacity: 0;
    -unity-text-align: middle-center;
    transition-property: opacity;
    transition-duration: 0.8s;
    transition-timing-function: ease-in;
}

.caption.visible {
    opacity: 1;
}
```

Notes on the cursor element: Use a simple white circle via border-radius (no external texture needed). If the user later wants a custom texture, swap `background-color` for `background-image: url(...)`.

**2. Create `Assets/Resources/UI/IntroCutscene.uxml`:**

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="project://database/Assets/Resources/UI/intro-cutscene.uss" />
    <ui:VisualElement name="CutsceneOverlay" class="cutscene-overlay">
        <ui:VisualElement name="CustomCursor" class="custom-cursor" />
        <ui:Label name="Caption" text="Look into the void" class="caption" />
    </ui:VisualElement>
</ui:UXML>
```

**3. Create `Assets/Scripts/Core/IntroCutscene.cs`:**

Full self-bootstrapping coroutine-based state machine MonoBehaviour. Key implementation details:

- **Static bootstrap method** with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`:
  ```csharp
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void AutoInit()
  {
      // Skip if one already exists in scene (manual placement path)
      if (FindAnyObjectByType<IntroCutscene>() != null) return;

      // Suppress gameplay IMMEDIATELY (AfterSceneLoad fires after all Awake() but before any Start())
      var cell = FindAnyObjectByType<TotipotentStemCell>(FindObjectsInactive.Include);
      if (cell != null) cell.gameObject.SetActive(false);

      var cam = FindAnyObjectByType<CameraHandler>();
      if (cam != null) cam.enabled = false;

      Cursor.visible = false;

      // Create cutscene GameObject
      GameObject go = new GameObject("IntroCutscene");
      IntroCutscene cutscene = go.AddComponent<IntroCutscene>();
      cutscene.initialCell = cell;
      cutscene.cameraHandler = cam;
      cutscene.suppressionDone = true;

      // Create and configure UIDocument
      UIDocument doc = go.AddComponent<UIDocument>();
      doc.sortingOrder = 100; // Above all gameplay UI (existing uses -1 to 1)
      doc.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/IntroCutscene");
      cutscene.overlayDocument = doc;
  }
  ```

- **Awake():** Only runs when manually placed in scene (not via bootstrap). Guard with `if (!suppressionDone)`:
  - Find TotipotentStemCell via `FindAnyObjectByType<TotipotentStemCell>(FindObjectsInactive.Include)` and call `SetActive(false)` on its GameObject
  - Find CameraHandler via `FindAnyObjectByType<CameraHandler>()` and set `enabled = false`
  - Set `Cursor.visible = false`
  - Store references in `initialCell` and `cameraHandler` fields
  - Set `suppressionDone = true`

- **Start():** Query UI elements from the UIDocument:
  - `overlayRoot = overlayDocument.rootVisualElement`
  - `cursorElement = overlayRoot.Q<VisualElement>("CustomCursor")`
  - `captionElement = overlayRoot.Q<Label>("Caption")`
  - Start the coroutine: `StartCoroutine(CutsceneSequence())`

- **Update():** Track mouse position each frame and update cursor element position:
  - Read `Mouse.current.position.ReadValue()`
  - Flip Y for UI Toolkit: `float uiTop = Screen.height - pos.y`
  - Set `cursorElement.style.left = pos.x - 16f` and `cursorElement.style.top = uiTop - 16f` (half of 32px cursor size)

- **OnDisable():** Safety restore `Cursor.visible = true` (prevents cursor disappearing in editor)

- **CheckSkipInput() helper:** Returns true if any key is pressed OR left mouse button was pressed this frame. Used during the void and cursor-fade phases to allow skipping:
  ```csharp
  private bool CheckSkipInput()
  {
      return Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame
          || Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
  }
  ```

- **CutsceneSequence() coroutine** with these phases:
  1. **Void phase:** Loop frame-by-frame for `initialDelay` seconds. Each frame check `CheckSkipInput()` -- if true, break and call `TransitionToGameplay()` immediately (yield break after). Use elapsed time tracking instead of `WaitForSeconds` so skip input can be checked each frame.
  2. **Cursor fade-in:** `yield return null` (wait one frame so USS transition fires), then `cursorElement.AddToClassList("visible")`. Loop frame-by-frame for `cursorFadeDuration` seconds, checking `CheckSkipInput()` each frame -- if true, break and call `TransitionToGameplay()` immediately.
  3. **Center detection loop:** Each frame check `IsCursorNearCenter()`. When true and caption not shown, call `captionElement.AddToClassList("visible")` and position caption below cursor. When false and caption shown, call `captionElement.RemoveFromClassList("visible")`. When caption is visible and `Mouse.current.leftButton.wasPressedThisFrame`, break out of loop. Use `yield return null` each iteration. (No skip-any-key in this phase -- the player must complete the intended interaction of finding center and clicking.)
  4. **Transition:** Call `TransitionToGameplay()` coroutine via `yield return`.

- **IsCursorNearCenter():** Compare `Vector2.Distance(mousePos, screenCenter)` against `centerThresholdPixels` (default 80f). screenCenter = `new Vector2(Screen.width / 2f, Screen.height / 2f)`.

- **TransitionToGameplay() coroutine:**
  - `overlayRoot.AddToClassList("fade-out")`
  - `yield return new WaitForSeconds(1f)` to match USS transition duration
  - Re-enable initial cell: `initialCell.gameObject.SetActive(true)`
  - Re-enable camera: `cameraHandler.enabled = true`
  - Restore system cursor: `Cursor.visible = true`
  - Disable overlay: `overlayDocument.gameObject.SetActive(false)`
  - Disable self: `enabled = false`

- **Field declarations:**
  - `[Header("Timing")] [SerializeField] private float initialDelay = 3f;`
  - `[SerializeField] private float cursorFadeDuration = 1.5f;`
  - `[Header("Detection")] [SerializeField] private float centerThresholdPixels = 80f;`
  - `internal UIDocument overlayDocument;` (internal so bootstrap can assign it)
  - `internal TotipotentStemCell initialCell;`
  - `internal CameraHandler cameraHandler;`
  - `internal bool suppressionDone;`
  - `private VisualElement overlayRoot;`
  - `private VisualElement cursorElement;`
  - `private Label captionElement;`

- **Required usings:** `System.Collections`, `UnityEngine`, `UnityEngine.InputSystem`, `UnityEngine.UIElements`

- **No namespace.** Follow project convention (global namespace).

- **Allman brace style, 4-space indent.** Match existing code in Camera.cs and CellManager.cs.

- Position caption below cursor dynamically: when showing caption, set `captionElement.style.left` and `captionElement.style.top` relative to cursor position (cursor X centered, cursor Y + 26f for spacing below the 32px cursor).
  </action>
  <verify>
    <automated>cd "I:/Projects/Unity/Neural Growth Factory" && test -f Assets/Resources/UI/IntroCutscene.uxml && test -f Assets/Resources/UI/intro-cutscene.uss && test -f Assets/Scripts/Core/IntroCutscene.cs && grep -q "RuntimeInitializeOnLoadMethod" Assets/Scripts/Core/IntroCutscene.cs && grep -q "CheckSkipInput" Assets/Scripts/Core/IntroCutscene.cs && grep -q "Resources/UI/intro-cutscene.uss" Assets/Resources/UI/IntroCutscene.uxml && echo "All files exist with key symbols"</automated>
  </verify>
  <done>
    - IntroCutscene.cs exists with AutoInit() bootstrap, Awake(), Start(), Update(), OnDisable(), CutsceneSequence(), CheckSkipInput(), IsCursorNearCenter(), TransitionToGameplay()
    - AutoInit has [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] and creates GameObject, UIDocument, suppresses gameplay
    - CheckSkipInput() checks Keyboard.current.anyKey and Mouse.current.leftButton
    - Void phase and cursor-fade phase both check CheckSkipInput() each frame to allow skipping
    - IntroCutscene.uxml exists in Assets/Resources/UI/ with CutsceneOverlay, CustomCursor, and Caption elements
    - intro-cutscene.uss exists in Assets/Resources/UI/ with .cutscene-overlay, .custom-cursor, .caption classes and their .visible/.fade-out transition states
    - UXML Style src points to Assets/Resources/UI/intro-cutscene.uss
    - No existing files modified
  </done>
</task>

<task type="checkpoint:human-verify" gate="blocking">
  <what-built>
Complete self-bootstrapping intro cutscene system: empty void on game start, custom cursor fades in after 3 seconds, "Look into the void" caption appears near screen center, click advances to gameplay. Skip support: pressing any key or clicking during the void or cursor-fade phases jumps straight to gameplay transition. No manual scene setup needed -- the system creates itself via RuntimeInitializeOnLoadMethod.
  </what-built>
  <how-to-verify>
1. Open the project in Unity 6 and enter Play mode in the Neurosphere scene
2. Verify: Screen shows only the black/dark background for ~3 seconds (no cells, no sidebar, no counter, no upgrade UI -- these are all hidden by default until cell thresholds, and the TPC cell is deactivated preventing CellBirth)
3. Verify: A white circle cursor fades in smoothly over ~1.5 seconds. The system mouse cursor should be hidden.
4. Move the custom cursor around -- it should track the mouse smoothly
5. Move the cursor toward the center of the screen. When within roughly 80px of center, "Look into the void" should fade in below the cursor
6. Move cursor away from center -- caption should fade out
7. Move cursor back to center (caption visible) and click
8. Verify: The overlay fades out, the initial TPC cell appears, the camera responds to pan/zoom/click, and the system cursor is restored
9. Stop Play mode and re-enter Play mode. This time, during the void wait (first 3 seconds), press any key (e.g., Space or Escape). Verify the cutscene skips directly to gameplay transition.
10. Stop Play mode -- verify the system cursor is visible in the editor (OnDisable safety)
  </how-to-verify>
  <resume-signal>Type "approved" or describe any issues with timing, positioning, skip behavior, or visual appearance</resume-signal>
</task>

</tasks>

<verification>
- Enter Play mode: black void for 3 seconds, then cursor fades in
- During void or cursor-fade: pressing any key or clicking skips to gameplay
- Move cursor to center: caption appears
- Click while caption visible: transitions to normal gameplay
- All gameplay systems functional after transition (click cells, pan, zoom)
- Stop Play mode: system cursor visible in editor
- No manual scene setup was required -- fully self-bootstrapping
</verification>

<success_criteria>
- Game starts with atmospheric void (no cells, no UI) for a configurable delay
- Custom cursor element fades in with USS transition
- System cursor hidden during cutscene
- "Look into the void" appears/disappears based on cursor proximity to screen center
- Click during caption triggers smooth transition to gameplay
- Pressing any key or clicking during void/cursor-fade phases skips the cutscene to gameplay
- All gameplay systems (CameraHandler, initial TPC cell) re-enabled after cutscene
- No modifications to existing scripts -- suppression via enable/disable only
- Self-bootstrapping via RuntimeInitializeOnLoadMethod (no manual scene setup)
- All files in Assets/Resources/UI/ for Resources.Load compatibility
</success_criteria>

<output>
After completion, create `.planning/phases/1/01-intro-cutscene-01-SUMMARY.md`
</output>
