using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init() {
        var go = new GameObject("CursorManager");
        go.AddComponent<CursorManager>();
        DontDestroyOnLoad(go);
    }

    private void Update() {
        var cursor = CursorDefinition.Current;
        if (cursor == null) return;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            cursor.SetClicked();
        else
            cursor.SetDefault();
    }
}
