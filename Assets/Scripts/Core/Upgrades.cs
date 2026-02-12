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

    void Awake () {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        upgradeCatalog = UpgradeData.CreateCatalog();
        InitializeUI();
    }

    void OnEnable() => CellManager.CellBirth += HandleCellBirth;
    void OnDisable() => CellManager.CellBirth -= HandleCellBirth;

    private void InitializeUI()
    {
        var root = document.rootVisualElement;
        var view = GetComponent<UpgradeTreeView>();

        if (view == null)
        {
            Debug.LogError("UpgradeTreeView component not found on this GameObject");
            return;
        }
        // Make sure the view uses the same document as Upgrades
        if (document != null) view.SetDocument(document);

        // Cache the lineage holder reference
        lineageHolder = root.Q<VisualElement>(LINEAGE_HOLDER_NAME);
        if (lineageHolder != null)
        {
            lineageHolder.style.opacity = 0f;
            lineageHolder.SetEnabled(false);
        }

        upgradePresenter = new UpgradeTreePresenter(view, upgradeCatalog);
    }

    public bool IsUpgradeUnlocked(string id) => upgradeCatalog.IsUnlocked(id);
    public int GetUpgradeLevel(string id) => upgradeCatalog.GetUpgradeLevel(id);
    
    private void HandleCellBirth(CellType cellType)
    {
        // Reveal the lineage UI when the player has produced enough RGCs
        if (cellType == CellType.RGC &&
            CellManager.Instance.GetCellCount(CellType.RGC) >= LINEAGE_UNLOCK_THRESHOLD &&
            lineageHolder != null)
        {
            lineageHolder.style.opacity = 1f;
            lineageHolder.SetEnabled(true);
            CellManager.CellBirth -= HandleCellBirth; // Unsubscribe after unlocking
        }
    }
}
