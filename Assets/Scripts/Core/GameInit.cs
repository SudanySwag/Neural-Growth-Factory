using UnityEngine;

public static class GameInit {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() {
        CursorDefinition.Gameplay.Activate();
    }
}
