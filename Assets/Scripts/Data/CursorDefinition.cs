using System.Collections.Generic;
using UnityEngine;

public enum CursorAlignment {
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    Center,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

[System.Serializable]
public class CursorDefinition {
    [Tooltip("Default cursor texture")]
    public Texture2D texture;

    [Tooltip("Shown when hovering over a clickable object")]
    public Texture2D activeTexture;

    [Tooltip("Shown when the cursor is blocked from clicking")]
    public Texture2D disabledTexture;

    [Tooltip("Shown while mouse button is held down")]
    public Texture2D clickedTexture;

    public CursorAlignment alignment = CursorAlignment.TopLeft;

    private static CursorDefinition current;
    private CursorState currentState = CursorState.Default;

    private enum CursorState { Default, Active, Disabled, Clicked }

    // --- Cursor Registry ---

    private static readonly Dictionary<string, CursorDefinition> registry = new Dictionary<string, CursorDefinition>();

    public static readonly CursorDefinition Intro = Register("Intro", CursorAlignment.Center, 2f);
    public static readonly CursorDefinition Gameplay = Register("Gameplay", CursorAlignment.Center, .5f);

    private static CursorDefinition Register(string name, CursorAlignment align, float size = 1f) {
        int pixels = Mathf.RoundToInt(size * 32);
        var def = new CursorDefinition {
            texture = ResizeIfNeeded(Resources.Load<Texture2D>($"Cursors/{name}Cursor"), pixels),
            activeTexture = ResizeIfNeeded(Resources.Load<Texture2D>($"Cursors/{name}CursorActive"), pixels),
            disabledTexture = ResizeIfNeeded(Resources.Load<Texture2D>($"Cursors/{name}CursorDisabled"), pixels),
            clickedTexture = ResizeIfNeeded(Resources.Load<Texture2D>($"Cursors/{name}CursorClicked"), pixels),
            alignment = align
        };

        if (def.texture == null)
            Debug.LogWarning($"[CursorDefinition] No texture found for '{name}' cursor at Resources/Cursors/{name}Cursor");

        registry[name] = def;
        return def;
    }

    private static Texture2D ResizeIfNeeded(Texture2D source, int size) {
        if (source == null) return null;
        if (source.width == size && source.height == size) return source;

        RenderTexture rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit(source, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D resized = new Texture2D(size, size, TextureFormat.RGBA32, false);
        resized.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        resized.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return resized;
    }

    public static CursorDefinition Get(string name) {
        return registry.TryGetValue(name, out var def) ? def : null;
    }

    // --- Hotspot ---

    public Vector2 Hotspot {
        get {
            if (texture == null) return Vector2.zero;
            float w = texture.width;
            float h = texture.height;

            return alignment switch {
                CursorAlignment.TopLeft => new Vector2(0, 0),
                CursorAlignment.TopCenter => new Vector2(w / 2f, 0),
                CursorAlignment.TopRight => new Vector2(w, 0),
                CursorAlignment.MiddleLeft => new Vector2(0, h / 2f),
                CursorAlignment.Center => new Vector2(w / 2f, h / 2f),
                CursorAlignment.MiddleRight => new Vector2(w, h / 2f),
                CursorAlignment.BottomLeft => new Vector2(0, h),
                CursorAlignment.BottomCenter => new Vector2(w / 2f, h),
                CursorAlignment.BottomRight => new Vector2(w, h),
                _ => Vector2.zero
            };
        }
    }

    // --- State Management ---

    public void Activate() {
        current = this;
        currentState = CursorState.Default;
        ApplyTexture(texture);
    }

    public void SetActive() {
        if (current != this) return;
        if (currentState == CursorState.Active) return;
        currentState = CursorState.Active;
        ApplyTexture(activeTexture != null ? activeTexture : texture);
    }

    public void SetDisabled() {
        if (current != this) return;
        if (currentState == CursorState.Disabled) return;
        currentState = CursorState.Disabled;
        ApplyTexture(disabledTexture != null ? disabledTexture : texture);
    }

    public void SetClicked() {
        if (current != this) return;
        if (currentState == CursorState.Clicked) return;
        currentState = CursorState.Clicked;
        ApplyTexture(clickedTexture != null ? clickedTexture : texture);
    }

    public void SetDefault() {
        if (current != this) return;
        if (currentState == CursorState.Default) return;
        currentState = CursorState.Default;
        ApplyTexture(texture);
    }

    private void ApplyTexture(Texture2D tex) {
        if (tex == null) {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            return;
        }
        UnityEngine.Cursor.SetCursor(tex, Hotspot, CursorMode.ForceSoftware);
        UnityEngine.Cursor.visible = true;
    }

    public static CursorDefinition Current => current;
}
