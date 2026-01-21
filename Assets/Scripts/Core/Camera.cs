using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public class CameraHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask clickableLayers = ~0; // default: everything

    [Header("Pan")]
    [SerializeField] private float panSpeed = 1.0f;
    [SerializeField] private bool invertPan = false;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2.0f;
    private float cellMultiplier = 1.0f;
    [SerializeField] private float minOrthoSize = 0.5f;
    [SerializeField] private float maxOrthoSize = 50f;

    private Vector2 lastMousePos;
    private bool dragging;

    void OnEnable()
    {
        CellManager.CellBirth += HandleCellBirth;
    }

    void Awake()
    {
        if (!cam) cam = GetComponentInChildren<Camera>();
        if (cam && !cam.orthographic) cam.orthographic = true;
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasReleasedThisFrame && !dragging) {
            Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, clickableLayers))
            {
                var clickable = hit.collider.GetComponentInParent<IClickable>();
                clickable?.Click();
            }
        }

        HandlePan(mouse);
        HandleZoom(mouse);
    }

    void HandlePan(Mouse mouse)
    {
        // Left click + drag
        if (mouse.leftButton.wasPressedThisFrame)
        {
            lastMousePos = mouse.position.ReadValue();
        }
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
        }
        if (!mouse.leftButton.isPressed) return;

        Vector2 pos = mouse.position.ReadValue();
        Vector2 delta = pos - lastMousePos;
        if (delta.magnitude > 10f) dragging = true;

        lastMousePos = pos;

        float sign = invertPan ? 1f : -1f;

        // pixel -> world units (keeps pan consistent at different zoom levels)
        float worldPerPixel = 2f * cam.orthographicSize / Screen.height;


        Vector3 move =
            (cam.transform.right * delta.x + cam.transform.up * delta.y) *
            (worldPerPixel * panSpeed * sign);

        transform.position += move;
    }

    void HandleCellBirth(CellType cellType)
    {
        long cellCount = 0;
        foreach (CellType ct in System.Enum.GetValues(typeof(CellType)))
        {
            cellCount += CellManager.Instance.GetCellCount(ct);
        }
        cellMultiplier = 1.0f + Mathf.Log10(Mathf.Max(1, cellCount)) * 0.5f;
    }
    void HandleZoom(Mouse mouse)
    {
        float scrollY = mouse.scroll.ReadValue().y; // typically +/- 120 per notch
        if (scrollY == 0f) return;

        // Normalize the wheel a bit (feel free to tweak divisor)
        float scroll = scrollY / 20f;

        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize - scroll * zoomSpeed * cellMultiplier,
            minOrthoSize,
            maxOrthoSize
        );
    }
}
