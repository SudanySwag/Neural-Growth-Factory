/// <summary>
/// Central manager for all cell resources
/// </summary>
/*
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    
    private Dictionary<string, CellResource> resources = new Dictionary<string, CellResource>();
    
    public event Action<string, double> OnResourceChanged;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void RegisterResource(CellType cellType, double initialCount = 0)
    {
        if (!resources.ContainsKey(cellType.id))
        {
            resources[cellType.id] = new CellResource
            {
                cellType = cellType,
                count = initialCount,
                productionRate = 0
            };
        }
    }
    
    public CellResource GetResource(string cellTypeId)
    {
        return resources.ContainsKey(cellTypeId) ? resources[cellTypeId] : null;
    }
    
    public void AddResource(string cellTypeId, double amount)
    {
        if (resources.ContainsKey(cellTypeId))
        {
            resources[cellTypeId].AddCells(amount);
            OnResourceChanged?.Invoke(cellTypeId, resources[cellTypeId].count);
        }
    }
    
    public bool TrySpendResource(string cellTypeId, double amount)
    {
        if (resources.ContainsKey(cellTypeId) && resources[cellTypeId].TrySpend(amount))
        {
            OnResourceChanged?.Invoke(cellTypeId, resources[cellTypeId].count);
            return true;
        }
        return false;
    }
    
    public Dictionary<string, CellResource> GetAllResources()
    {
        return new Dictionary<string, CellResource>(resources);
    }
}

*/