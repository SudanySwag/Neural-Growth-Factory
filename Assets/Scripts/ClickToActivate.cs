using UnityEngine;
using UnityEngine.InputSystem;

public class ClickToActivate : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask clickableLayers = ~0; // default: everything

    void Reset()
    {
        if (!cam) cam = Camera.main;
    }

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (!mouse.leftButton.wasPressedThisFrame) return;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, clickableLayers))
        {
            hit.collider.GetComponentInParent<NeuralStemCell>()?.Activate();
        }
    }
}
