using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class CellUpgradePopupView : MonoBehaviour
{
    [SerializeField] Camera worldCamera;

    UIDocument doc;
    VisualElement popup, panel;
    Label titleLabel;
    ScrollView upgradeList;
    bool initialized;
    CellUpgradePopupPresenter presenter;

    readonly Dictionary<string, VisualElement> rowById = new();

    public event Action<string> UpgradeClicked;

    public void SetCamera(Camera cam)
    {
        worldCamera = cam;
    }

    UpgradeCatalog catalog;

    void Awake()
    {
        catalog = CellUpgradeData.CreateCatalog();
    }

    void OnEnable()
    {
        initialized = false;
        presenter = new CellUpgradePopupPresenter(this, catalog);
    }

    void OnDisable()
    {
        presenter?.Dispose();
        presenter = null;
    }

    void EnsureInitialized()
    {
        if (initialized) return;

        doc = GetComponent<UIDocument>();
        if (doc == null) return;

        var root = doc.rootVisualElement;
        if (root == null) return;

        popup = root.Q<VisualElement>("CellUpgradePopup");
        if (popup == null) return;

        panel = popup.Q<VisualElement>("PopupPanel");
        titleLabel = popup.Q<Label>("CellTypeName");
        upgradeList = popup.Q<ScrollView>("UpgradeList");

        // Start hidden
        popup.EnableInClassList("visible", false);

        initialized = true;
    }

    public void Show(CellType cellType, Vector3 worldPos, IEnumerable<UpgradeDef> upgrades)
    {
        EnsureInitialized();
        if (popup == null) return;

        titleLabel.text = cellType.ToString();

        // Build upgrade rows
        upgradeList.Clear();
        rowById.Clear();

        foreach (var def in upgrades)
        {
            var row = BuildRow(def);
            upgradeList.Add(row);
            rowById[def.Id] = row;
        }

        // Position panel near clicked cell
        if (worldCamera != null)
        {
            Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPos);
            // Flip Y: screen space is bottom-up, UI Toolkit expects top-down
            Vector2 screenPoint = new Vector2(screenPos.x, Screen.height - screenPos.y);
            // Convert screen coords to panel coords (handles DPI + scale mode)
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(popup.panel, screenPoint);

            var panelSize = popup.panel.visualTree.layout;
            panelPos.x = Mathf.Clamp(panelPos.x, 10, panelSize.width - 300);
            panelPos.y = Mathf.Clamp(panelPos.y, 10, panelSize.height - 420);

            panel.style.left = panelPos.x;
            panel.style.top = panelPos.y;
        }

        popup.EnableInClassList("visible", true);
    }

    public void Hide()
    {
        EnsureInitialized();
        if (popup == null) return;
        popup.EnableInClassList("visible", false);
    }

    public bool IsVisible()
    {
        EnsureInitialized();
        return popup != null && popup.ClassListContains("visible");
    }

    VisualElement BuildRow(UpgradeDef def)
    {
        var row = new VisualElement();
        row.AddToClassList("upgrade-row");
        row.name = def.Id;

        var header = new VisualElement();
        header.AddToClassList("upgrade-row-header");

        var title = new Label(def.Title);
        title.AddToClassList("upgrade-row-title");
        header.Add(title);

        var level = new Label($"Lv {def.Level}/{def.maxLevel}");
        level.AddToClassList("upgrade-row-level");
        level.name = def.Id + "-level";
        header.Add(level);

        row.Add(header);

        var desc = new Label(def.Description ?? "");
        desc.AddToClassList("upgrade-row-desc");
        row.Add(desc);

        var footer = new VisualElement();
        footer.AddToClassList("upgrade-row-footer");

        var cost = new Label(def.Level >= def.maxLevel ? "MAX" : $"{def.Cost(def.Level)} RGC");
        cost.AddToClassList("upgrade-row-cost");
        cost.name = def.Id + "-cost";
        footer.Add(cost);

        var btn = new Button(() => UpgradeClicked?.Invoke(def.Id));
        btn.text = def.Level >= def.maxLevel ? "Maxed" : "Upgrade";
        btn.AddToClassList("upgrade-row-button");
        btn.name = def.Id + "-btn";
        btn.SetEnabled(def.Level < def.maxLevel);
        footer.Add(btn);

        row.Add(footer);

        return row;
    }

    public void RefreshRow(string id, UpgradeDef def, bool canPurchase)
    {
        EnsureInitialized();
        if (!rowById.TryGetValue(id, out var row)) return;

        var level = popup.Q<Label>(id + "-level");
        if (level != null)
            level.text = $"Lv {def.Level}/{def.maxLevel}";

        var cost = popup.Q<Label>(id + "-cost");
        if (cost != null)
            cost.text = def.Level >= def.maxLevel ? "MAX" : $"{def.Cost(def.Level)} RGC";

        var btn = popup.Q<Button>(id + "-btn");
        if (btn != null)
        {
            btn.text = def.Level >= def.maxLevel ? "Maxed" : "Upgrade";
            btn.SetEnabled(canPurchase);
        }

        row.EnableInClassList("is-maxed", def.Level >= def.maxLevel);
        row.EnableInClassList("is-affordable", canPurchase && def.Level < def.maxLevel);
    }
}
