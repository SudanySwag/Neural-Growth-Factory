using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
    {
        TPC,
        NSC,
        GMC,
        RGC,
        NB,
        N
    }

public class CellManager : MonoBehaviour
{
    

    public static CellManager Instance { get; private set; }
    private Dictionary<CellType, GameObject> prefabs = new Dictionary<CellType, GameObject>();
    
        [System.Serializable]
    public class CellCountEntry
    {
        public CellType cellType;
        public int count;
    }

    [SerializeField]
    private List<CellCountEntry> countsDisplay = new List<CellCountEntry>();
    private Dictionary<CellType, List<Cell>> cells = new Dictionary<CellType, List<Cell>>();


    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize counts display
        foreach (CellType ct in System.Enum.GetValues(typeof(CellType)))  {
            countsDisplay.Add(new CellCountEntry { cellType = ct, count = 0 });
            cells[ct] = new List<Cell>();
        }


        
        // Load prefabs from Resources folder
        GameObject tpcPrefab = Resources.Load<GameObject>("Cells/totipotentStemCell");
        GameObject nscPrefab = Resources.Load<GameObject>("Cells/neuralStemCell");
        GameObject gmcPrefab = Resources.Load<GameObject>("Cells/ganglionMotherCell");
        GameObject rgcPrefab = Resources.Load<GameObject>("Cells/radialGlialCell");
        GameObject nbPrefab = Resources.Load<GameObject>("Cells/neuroblast");
        GameObject nPrefab = Resources.Load<GameObject>("Cells/neuron");
        
        prefabs[CellType.TPC] = tpcPrefab;
        prefabs[CellType.NSC] = nscPrefab;
        prefabs[CellType.GMC] = gmcPrefab;
        prefabs[CellType.RGC] = rgcPrefab;
        prefabs[CellType.NB] = nbPrefab;
        prefabs[CellType.N] = nPrefab;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject GetPrefab(CellType cellType)
    {
        if (prefabs.TryGetValue(cellType, out GameObject prefab))
        {
            return prefab;
        }
        else
        {
            Debug.LogError($"CellManager: No prefab found for CellType {cellType}");
            return null;
        }
    }

    public void RegisterCell(Cell cellInstance)
    {
        cells[cellInstance.cellType].Add(cellInstance);
        
        UpdateDisplayCount(cellInstance.cellType);
    }

    public void UnregisterCell(Cell cellInstance)
    {
        cells[cellInstance.cellType].Remove(cellInstance);
        UpdateDisplayCount(cellInstance.cellType);
    }

    private void UpdateDisplayCount(CellType cellType)
    {
        var entry = countsDisplay.Find(e => e.cellType == cellType);
        if (entry != null)
        {
            entry.count = cells[cellType].Count;
        }
    }

    public int getCellCount(CellType cellType)
    {
        return cells[cellType].Count;
    }

    public void KillCells(CellType ct, int count)
    {
        if (!cells.ContainsKey(ct) || cells[ct].Count == 0)
        {
            Debug.LogWarning($"CellManager: No cells of type {ct} available to kill.");
            return;
        }
        
        int cellsToKill = Mathf.Min(count, cells[ct].Count);
        
        if (cellsToKill < count)
        {
            Debug.LogWarning($"CellManager: Attempted to kill {count} cells of type {ct}, but only {cellsToKill} are available.");
        }
        
        for (int i = 0; i < cellsToKill; i++)
        {
            int index = Random.Range(0, cells[ct].Count);
            Cell cell = cells[ct][index];
            cells[ct].RemoveAt(index);
            Destroy(cell.gameObject);
        }
    }
}
