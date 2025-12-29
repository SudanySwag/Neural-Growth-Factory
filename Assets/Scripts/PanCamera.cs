using UnityEngine;
using UnityEngine.InputSystem;

public class PanCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    // [SerializeField] private ViewportHover viewportHover;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 1.0f;
    [SerializeField] private bool invertPan = false;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2.0f;
    [SerializeField] private float minOrthoSize = 0.5f;
    [SerializeField] private float maxOrthoSize = 50f;

    private Vector2 lastMousePos;
    private bool dragging;

    void Reset()
    {
        cam = GetComponentInChildren<Camera>();
    }

    void Awake()
    {
        if (!cam) cam = GetComponentInChildren<Camera>();
        if (cam && !cam.orthographic) cam.orthographic = true;
    }

    void Update()
    {
        // if (!cam || viewportHover == null) return;
        // if (!viewportHover.IsHovering) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        HandlePan(mouse);
        HandleZoom(mouse);
    }

    void HandlePan(Mouse mouse)
    {
        // Left click + drag
        if (mouse.leftButton.wasPressedThisFrame)
        {
            dragging = true;
            lastMousePos = mouse.position.ReadValue();
        }
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
        }
        if (!dragging) return;

        Vector2 pos = mouse.position.ReadValue();
        Vector2 delta = pos - lastMousePos;
        lastMousePos = pos;

        float sign = invertPan ? 1f : -1f;

        // pixel -> world units (keeps pan consistent at different zoom levels)
        float worldPerPixel = (2f * cam.orthographicSize) / Screen.height;

        Vector3 move =
            (cam.transform.right * delta.x + cam.transform.up * delta.y) *
            (worldPerPixel * panSpeed * sign);

        transform.position += move;
    }

    void HandleZoom(Mouse mouse)
    {
        float scrollY = mouse.scroll.ReadValue().y; // typically +/- 120 per notch
        if (scrollY == 0f) return;

        // Normalize the wheel a bit (feel free to tweak divisor)
        float scroll = scrollY / 120f;

        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize - scroll * zoomSpeed,
            minOrthoSize,
            maxOrthoSize
        );
    }
}
