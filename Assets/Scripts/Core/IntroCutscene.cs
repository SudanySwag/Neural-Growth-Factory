using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class IntroCutscene : MonoBehaviour {
    [Header("Timing")]
    [SerializeField] private float initialDelay = 3f;

    [Header("Detection")]
    [SerializeField] private float centerThresholdPixels = 80f;

    private UIDocument overlayDocument;
    private VisualElement overlayRoot;
    private Label captionElement;
    private TotipotentStemCell initialCell;

    private void Awake() {
        overlayDocument = GetComponent<UIDocument>();

        initialCell = FindAnyObjectByType<TotipotentStemCell>(FindObjectsInactive.Include);
        if (initialCell != null) initialCell.gameObject.SetActive(false);

        foreach (var cam in FindObjectsByType<CameraHandler>(FindObjectsSortMode.None))
            cam.enabled = false;

        UnityEngine.Cursor.visible = false;
    }

    private void Start() {
        overlayRoot = overlayDocument.rootVisualElement.Q("CutsceneOverlay");
        captionElement = overlayRoot.Q<Label>("Caption");
        StartCoroutine(CutsceneSequence());
    }

    private void OnDisable() {
        UnityEngine.Cursor.visible = true;
    }

    private bool CheckSkipInput() {
        return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
    }

    private void PositionCaptionBelowCursor() {
        if (Mouse.current == null) return;
        Vector2 pos = Mouse.current.position.ReadValue();
        float uiTop = Screen.height - pos.y;
        captionElement.style.left = pos.x - captionElement.resolvedStyle.width / 2f;
        captionElement.style.top = uiTop + 26f;
    }

    private bool IsCursorNearCenter() {
        if (Mouse.current == null) return false;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        return Vector2.Distance(mousePos, screenCenter) < centerThresholdPixels;
    }

    private IEnumerator CutsceneSequence() {
        // 1) Void phase: black screen, cursor hidden
        float elapsed = 0f;
        while (elapsed < initialDelay) {
            if (CheckSkipInput()) {
                yield return StartCoroutine(TransitionToGameplay());
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2) Show intro cursor
        CursorDefinition.Intro.Activate();

        // 3) Typewriter caption — position tracks cursor each frame
        string fullText = "Look into the void?";
        captionElement.text = "";
        captionElement.style.opacity = 1f;
        for (int c = 0; c < fullText.Length; c++) {
            captionElement.text = fullText.Substring(0, c + 1);
            float waitElapsed = 0f;
            while (waitElapsed < 0.10f) {
                PositionCaptionBelowCursor();
                waitElapsed += Time.deltaTime;
                yield return null;
            }
        }

        // 4) Center detection loop — caption follows cursor
        while (true) {
            PositionCaptionBelowCursor();

            if (IsCursorNearCenter() && Mouse.current.leftButton.wasPressedThisFrame) {
                break;
            }

            yield return null;
        }

        // 5) Transition to gameplay
        yield return StartCoroutine(TransitionToGameplay());
    }

    private IEnumerator TransitionToGameplay() {
        // Fade overlay out
        float fadeDuration = 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            overlayRoot.style.opacity = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        // Re-enable gameplay
        if (initialCell != null) initialCell.gameObject.SetActive(true);
        foreach (var cam in FindObjectsByType<CameraHandler>(FindObjectsSortMode.None))
            cam.enabled = true;

        // Switch to gameplay cursor
        CursorDefinition.Gameplay.Activate();

        // Clean up
        gameObject.SetActive(false);
    }
}
