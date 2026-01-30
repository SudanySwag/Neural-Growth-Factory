public sealed class UpgradeTreePresenter
{
    readonly IUpgradeTreeView view;
    readonly UpgradeCatalog catalog;
    readonly UpgradeState state;

    public UpgradeTreePresenter(IUpgradeTreeView view, UpgradeCatalog catalog, UpgradeState state)
    {
        this.view = view;
        this.catalog = catalog;
        this.state = state;

        view.BuildTree(catalog.ById.Values);

        view.NodeClicked += OnNodeClicked;
        state.Changed += Refresh;

        Refresh();
    }

    void OnNodeClicked(string id)
    {
        if (!catalog.ById.TryGetValue(id, out var upgrade)) return;

        state.Select(id);

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
        foreach (var prereqId in upgrade.Prereqs)
        {
            if (!state.IsUnlocked(prereqId))
                return false;
        }

        // Check if player has sufficient RGC cells
        int cost = upgrade.Cost(upgrade.Level);
        int currentCells = CellManager.Instance.GetCellCount(CellType.RGC);
        if (currentCells < cost)
            return false;

        return true;
    }

    void PurchaseUpgrade(UpgradeDef upgrade)
    {
        // Spend RGC cells
        int cost = upgrade.Cost(upgrade.Level);
        CellManager.Instance.KillCells(CellType.RGC, cost);

        // Increment level
        upgrade.Level++;

        // Unlock the upgrade if this is the first level
        if (upgrade.Level == 1)
        {
            state.Unlock(upgrade.Id);
        }

        // Apply the upgrade's effect
        ApplyUpgradeEffect(upgrade);

        // Refresh the UI
        Refresh();

        UnityEngine.Debug.Log($"Purchased {upgrade.Title} (Level {upgrade.Level})");
    }

    void ApplyUpgradeEffect(UpgradeDef upgrade)
    {
        // Map upgrade IDs to gameplay effects
        switch (upgrade.Id)
        {
            case "gmc-basic":
                Upgrades.GMCLevel = 1;
                UnityEngine.Debug.Log("GMC Production unlocked - NSC now spawns GMC");
                break;

            case "gmc-enhanced":
                Upgrades.GMCLevel = 2;
                UnityEngine.Debug.Log("GMC Enhancement unlocked - GMC divides 2 times");
                break;

            case "gmc-advanced":
                Upgrades.GMCLevel = 3;
                UnityEngine.Debug.Log("GMC Amplification unlocked - GMC divides 3 times");
                break;

            // Add other upgrade effects here as needed
            default:
                // No special effect for this upgrade
                break;
        }
    }

    void Refresh()
    {
        view.SetSelected(state.SelectedId);

        foreach (var u in catalog.ById.Values)
            view.SetUnlocked(u.Id, state.IsUnlocked(u.Id));
    }
}
