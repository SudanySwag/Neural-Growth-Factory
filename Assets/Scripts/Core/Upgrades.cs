using UnityEngine;
using UnityEngine.UIElements;

public class Upgrades : MonoBehaviour
{
    private const string LINEAGE_HOLDER_NAME = "LineageHolder";
    private const string LINEAGE_TREE_HOLDER_NAME = "LineageTreeHolder";
    private const int LINEAGE_UNLOCK_THRESHOLD = 10;

    public static Upgrades Instance { get; private set; }

    public UIDocument document;
    [SerializeField] UpgradeTreeView neurosphereView;
    [SerializeField] UpgradeTreeView lineageView;

    private UpgradeTreePresenter neurospherePresenter;
    private UpgradeCatalog neurosphereCatalog;
    private VisualElement lineageHolder;

    private UpgradeTreePresenter lineagePresenter;
    private UpgradeCatalog lineageCatalog;
    private VisualElement lineageTreeHolder;

    void Awake () {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        neurosphereCatalog = MutationData.CreateCatalog();
        lineageCatalog = LineageUpgradeData.CreateCatalog();
        InitializeUI();
    }

    void OnEnable() => CellManager.CellBirth += HandleCellBirth;
    void OnDisable() => CellManager.CellBirth -= HandleCellBirth;

    private void InitializeUI()
    {
        var root = document.rootVisualElement;

        if (neurosphereView != null)
        {
            neurosphereView.SetDocument(document);
            neurospherePresenter = new UpgradeTreePresenter(neurosphereView, neurosphereCatalog);
        }

        if (lineageView != null)
        {
            lineageView.SetDocument(document);
            lineagePresenter = new UpgradeTreePresenter(lineageView, lineageCatalog);
        }

        // Cache holder references
        lineageHolder = root.Q<VisualElement>(LINEAGE_HOLDER_NAME);
        if (lineageHolder != null)
        {
            lineageHolder.style.opacity = 0f;
            lineageHolder.SetEnabled(false);
        }

        lineageTreeHolder = root.Q<VisualElement>(LINEAGE_TREE_HOLDER_NAME);
        if (lineageTreeHolder != null)
        {
            lineageTreeHolder.style.opacity = 0f;
            lineageTreeHolder.SetEnabled(false);
        }
    }

    public bool IsUpgradeUnlocked(string id)
    {
        if (neurosphereCatalog.ById.ContainsKey(id))
            return neurosphereCatalog.IsUnlocked(id);
        if (lineageCatalog.ById.ContainsKey(id))
            return lineageCatalog.IsUnlocked(id);
        return false;
    }

    public int GetUpgradeLevel(string id)
    {
        if (neurosphereCatalog.ById.ContainsKey(id))
            return neurosphereCatalog.GetUpgradeLevel(id);
        if (lineageCatalog.ById.ContainsKey(id))
            return lineageCatalog.GetUpgradeLevel(id);
        return 0;
    }

    private void HandleCellBirth(CellType cellType)
    {
        // Unlock upgrade holders (make them interactive) when the player has enough RGCs
        // SidebarController handles which one is actually visible
        if (cellType == CellType.RGC &&
            CellManager.Instance.GetCellCount(CellType.RGC) >= LINEAGE_UNLOCK_THRESHOLD)
        {
            if (lineageHolder != null)
            {
                lineageHolder.style.opacity = 1f;
                lineageHolder.SetEnabled(true);
            }
            if (lineageTreeHolder != null)
            {
                lineageTreeHolder.SetEnabled(true);
            }
            CellManager.CellBirth -= HandleCellBirth;
        }
    }
}
