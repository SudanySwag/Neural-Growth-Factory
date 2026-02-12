using UnityEngine;
using UnityEngine.UIElements;

public class Upgrades : MonoBehaviour
{
    private const string LINEAGE_HOLDER_NAME = "LineageHolder";
    private const int LINEAGE_UNLOCK_THRESHOLD = 10;

    public static Upgrades Instance { get; private set; }

    public UIDocument document;

    private UpgradeTreePresenter upgradePresenter;
    private UpgradeCatalog upgradeCatalog;
    private VisualElement lineageHolder;

    void OnEnable()
    {
        Instance = this;
        CellManager.CellBirth += HandleCellBirth;
        upgradeCatalog = UpgradeData.CreateCatalog();
        InitializeUI();
    }

    void OnDisable()
    {
        CellManager.CellBirth -= HandleCellBirth;
    }

    private void InitializeUI()
    {
        var root = document.rootVisualElement;

        // Cache the lineage holder reference
        lineageHolder = root.Q<VisualElement>(LINEAGE_HOLDER_NAME);
        if (lineageHolder != null)
        {
            lineageHolder.style.opacity = 0f;
            lineageHolder.SetEnabled(false);
        }

        // Initialize the upgrade tree
        InitializeUpgradeTree();
    }

    void InitializeUpgradeTree()
    {
        

        var view = GetComponent<UpgradeTreeView>();
        if (view == null)
        {
            Debug.LogError("UpgradeTreeView component not found on this GameObject");
            return;
        }

        // Make sure the view uses the same document as Upgrades
        if (document != null)
        {
            view.SetDocument(document);
        }

        upgradePresenter = new UpgradeTreePresenter(view, upgradeCatalog);
    }

    public bool IsUpgradeUnlocked(string id) => upgradeCatalog.IsUnlocked(id);
    public int GetUpgradeLevel(string id) => upgradeCatalog.GetUpgradeLevel(id);
    
    private void HandleCellBirth(CellType cellType)
    {
        if (cellType == CellType.RGC &&
            CellManager.Instance.GetCellCount(CellType.RGC) >= LINEAGE_UNLOCK_THRESHOLD &&
            lineageHolder != null)
        {
            lineageHolder.style.opacity = 1f;
            lineageHolder.SetEnabled(true);
        }
    }
}
