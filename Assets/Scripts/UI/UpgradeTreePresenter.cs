public sealed class UpgradeTreePresenter
{
    readonly UpgradeTreeView view;
    readonly UpgradeCatalog catalog;
    private string selectedId;

    public UpgradeTreePresenter(UpgradeTreeView view, UpgradeCatalog catalog)
    {
        this.view = view;
        this.catalog = catalog;

        view.BuildTree(catalog.ById.Values);

        view.NodeClicked += OnNodeClicked;
        view.UpgradeClicked += OnUpgradeClicked;
        view.ViewRestored += OnViewRestored;

        Refresh();
    }

    void OnViewRestored()
    {
        view.BuildTree(catalog.ById.Values);
        Refresh();
    }

    void Select(string id)
    {
        selectedId = id;
        Refresh();
    }

    void OnNodeClicked(string id)
    {
        if (!catalog.ById.ContainsKey(id)) return;

        Select(id);
    }

    void OnUpgradeClicked(string id)
    {
        if (!catalog.ById.TryGetValue(id, out var upgrade)) return;

        // Attempt to purchase if possible
        if (CanPurchase(upgrade))
        {
            PurchaseUpgrade(upgrade);
        }
    }

    bool CanPurchase(UpgradeDef upgrade)
    {
        // Check if already at max level
        if (upgrade.Level >= upgrade.maxLevel)
            return false;

        // Check if all prerequisites are unlocked
        if (!ArePrereqsMet(upgrade))
            return false;

        // Check if player has sufficient RGC cells
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

    void PurchaseUpgrade(UpgradeDef upgrade)
    {
        // Spend RGC cells
        int cost = upgrade.Cost(upgrade.Level);
        CellManager.Instance.KillCells(CellType.RGC, cost);

        // Increment level
        upgrade.Level++;

        // Refresh the UI
        Refresh();

        UnityEngine.Debug.Log($"Purchased {upgrade.Title} (Level {upgrade.Level})");
    }

    void Refresh()
    {
        view.SetSelected(selectedId);

        foreach (var u in catalog.ById.Values)
        {
            view.SetUnlocked(u.Id, catalog.IsUnlocked(u.Id));
            view.SetUpgradeLevel(u.Id, u.Level, u.maxLevel, ArePrereqsMet(u), CanPurchase(u));
        }
    }
}
