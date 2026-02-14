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

    private bool unlocked = false;

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
            neurosphereView.ViewRestored += () => RefreshHolders(root);
        }

        if (lineageView != null)
        {
            lineageView.SetDocument(document);
            lineagePresenter = new UpgradeTreePresenter(lineageView, lineageCatalog);
            lineageView.ViewRestored += () => RefreshHolders(root);
        }

        RefreshHolders(root);
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

    private void RefreshHolders(VisualElement root)
    {
        lineageHolder = root.Q<VisualElement>(LINEAGE_HOLDER_NAME);
        lineageTreeHolder = root.Q<VisualElement>(LINEAGE_TREE_HOLDER_NAME);
        ApplyHolderState();
    }

    private void ApplyHolderState()
    {
        if (lineageHolder != null)
        {
            lineageHolder.style.opacity = unlocked ? 1f : 0f;
            lineageHolder.SetEnabled(unlocked);
        }
        if (lineageTreeHolder != null)
        {
            lineageTreeHolder.SetEnabled(unlocked);
        }
    }

    private void HandleCellBirth(CellType cellType)
    {
        if (cellType == CellType.RGC &&
            CellManager.Instance.GetCellCount(CellType.RGC) >= LINEAGE_UNLOCK_THRESHOLD)
        {
            unlocked = true;
            ApplyHolderState();
            CellManager.CellBirth -= HandleCellBirth;
        }
    }
}
