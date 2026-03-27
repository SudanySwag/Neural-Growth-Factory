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

    internal UIDocument overlayDocument;
    internal TotipotentStemCell initialCell;
    internal CameraHandler cameraHandler;
    internal bool suppressionDone;

    private VisualElement overlayRoot;
    private VisualElement cursorElement;
    private Label captionElement;

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

    private void Awake()
    {
        if (!suppressionDone)
        {
            initialCell = FindAnyObjectByType<TotipotentStemCell>(FindObjectsInactive.Include);
            if (initialCell != null) initialCell.gameObject.SetActive(false);

            cameraHandler = FindAnyObjectByType<CameraHandler>();
            if (cameraHandler != null) cameraHandler.enabled = false;

            Cursor.visible = false;
            suppressionDone = true;
        }
    }

    private void Start()
    {
        overlayRoot = overlayDocument.rootVisualElement;
        cursorElement = overlayRoot.Q<VisualElement>("CustomCursor");
        captionElement = overlayRoot.Q<Label>("Caption");
        StartCoroutine(CutsceneSequence());
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        if (cursorElement == null) return;

        Vector2 pos = Mouse.current.position.ReadValue();
        // Flip Y: screen space is bottom-up, UI Toolkit expects top-down
        float uiTop = Screen.height - pos.y;
        cursorElement.style.left = pos.x - 16f;
        cursorElement.style.top = uiTop - 16f;
    }

    private void OnDisable()
    {
        // Safety restore -- prevents cursor disappearing in editor
        Cursor.visible = true;
    }

    private bool CheckSkipInput()
    {
        return Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame
            || Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private bool IsCursorNearCenter()
    {
        if (Mouse.current == null) return false;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        return Vector2.Distance(mousePos, screenCenter) < centerThresholdPixels;
    }

    private IEnumerator CutsceneSequence()
    {
        // 1) Void phase: wait for initialDelay, checking skip each frame
        float elapsed = 0f;
        while (elapsed < initialDelay)
        {
            if (CheckSkipInput())
            {
                yield return StartCoroutine(TransitionToGameplay());
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2) Cursor fade-in: wait one frame so USS transition fires
        yield return null;
        cursorElement.AddToClassList("visible");

        elapsed = 0f;
        while (elapsed < cursorFadeDuration)
        {
            if (CheckSkipInput())
            {
                yield return StartCoroutine(TransitionToGameplay());
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3) Center detection loop
        bool captionShown = false;
        while (true)
        {
            if (IsCursorNearCenter())
            {
                if (!captionShown)
                {
                    // Position caption below cursor
                    Vector2 pos = Mouse.current.position.ReadValue();
                    float uiTop = Screen.height - pos.y;
                    captionElement.style.left = pos.x;
                    captionElement.style.top = uiTop + 26f;
                    captionElement.AddToClassList("visible");
                    captionShown = true;
                }

                // Check for click to advance
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    break;
                }
            }
            else
            {
                if (captionShown)
                {
                    captionElement.RemoveFromClassList("visible");
                    captionShown = false;
                }
            }

            yield return null;
        }

        // 4) Transition to gameplay
        yield return StartCoroutine(TransitionToGameplay());
    }

    private IEnumerator TransitionToGameplay()
    {
        overlayRoot.AddToClassList("fade-out");
        yield return new WaitForSeconds(1f);

        // Re-enable gameplay
        if (initialCell != null) initialCell.gameObject.SetActive(true);
        if (cameraHandler != null) cameraHandler.enabled = true;
        Cursor.visible = true;

        // Clean up overlay
        overlayDocument.gameObject.SetActive(false);
        enabled = false;
    }
}
