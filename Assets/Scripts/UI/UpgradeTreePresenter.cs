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
        if (!catalog.ById.TryGetValue(id, out var u)) return;

        // Example rule: can select any node, but can only unlock if prereqs met
        state.Select(id);
    }

    void Refresh()
    {
        view.SetSelected(state.SelectedId);

        foreach (var u in catalog.ById.Values)
            view.SetUnlocked(u.Id, state.IsUnlocked(u.Id));
    }
}
