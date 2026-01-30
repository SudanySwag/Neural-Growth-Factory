using UnityEngine;
using UnityEngine.UIElements;

public class Upgrades : MonoBehaviour
{
    private const string LINEAGE_HOLDER_NAME = "LineageHolder";
    private const int LINEAGE_UNLOCK_THRESHOLD = 10;

    public UIDocument document;

    public static short GMCLevel = 0;

    private UpgradeTreePresenter upgradePresenter;
    private UpgradeState upgradeState;
    private VisualElement lineageHolder;

    void OnEnable()
    {
        CellManager.CellBirth += HandleCellBirth;
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
        upgradeState = new UpgradeState();
        var catalog = TestUpgradeData.CreateTestCatalog();

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

        upgradePresenter = new UpgradeTreePresenter(view, catalog, upgradeState);
        upgradeState.Unlock("mitochondria");
    }

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
