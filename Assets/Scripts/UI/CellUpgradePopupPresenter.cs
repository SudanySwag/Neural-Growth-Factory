using System.Collections.Generic;
using UnityEngine;

public sealed class CellUpgradePopupPresenter
{
    readonly CellUpgradePopupView view;
    readonly UpgradeCatalog catalog;
    CellType? currentType;

    public CellUpgradePopupPresenter(CellUpgradePopupView view, UpgradeCatalog catalog)
    {
        this.view = view;
        this.catalog = catalog;

        view.UpgradeClicked += OnUpgradeClicked;
        CellNodeClickable.Clicked += OnCellNodeClicked;
        CameraHandler.ClickedNothing += OnDismiss;
    }

    public void Dispose()
    {
        CellNodeClickable.Clicked -= OnCellNodeClicked;
        CameraHandler.ClickedNothing -= OnDismiss;
    }

    void OnCellNodeClicked(CellNodeClickable node)
    {
        currentType = node.CellType;
        var upgrades = CellUpgradeData.GetUpgradesForType(catalog, node.CellType);
        view.Show(node.CellType, node.transform.position, upgrades);
        RefreshAll();
    }

    void OnDismiss()
    {
        if (!view.IsVisible()) return;
        currentType = null;
        view.Hide();
    }

    void OnUpgradeClicked(string id)
    {
        if (!catalog.ById.TryGetValue(id, out var upgrade)) return;
        if (!CanPurchase(upgrade)) return;

        int cost = upgrade.Cost(upgrade.Level);
        CellManager.Instance.KillCells(CellType.RGC, cost);
        upgrade.Level++;

        RefreshAll();
        Debug.Log($"Purchased {upgrade.Title} (Level {upgrade.Level})");
    }

    bool CanPurchase(UpgradeDef upgrade)
    {
        if (upgrade.Level >= upgrade.maxLevel)
            return false;

        if (!ArePrereqsMet(upgrade))
            return false;

        if (CellManager.Instance == null) return false;
        int cost = upgrade.Cost(upgrade.Level);
        int currentCells = CellManager.Instance.GetCellCount(CellType.RGC);
        if (currentCells < cost)
            return false;

        return true;
    }

    bool ArePrereqsMet(UpgradeDef upgrade)
    {
        foreach (var prereqId in upgrade.Prereqs)
        {
            if (!catalog.IsUnlocked(prereqId))
                return false;
        }
        return true;
    }

    void RefreshAll()
    {
        if (currentType == null) return;

        foreach (var def in CellUpgradeData.GetUpgradesForType(catalog, currentType.Value))
        {
            view.RefreshRow(def.Id, def, CanPurchase(def));
        }
    }
}
