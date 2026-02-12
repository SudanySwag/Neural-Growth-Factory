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

    [System.Serializable]
    public class CellInfo
    {
        public CellType cellType;
        public GameObject prefab;
        public List<Cell> cellList = new List<Cell>();
        public int lifetimeCount = 0; // Total cells created of this type
    }

    [System.Serializable]
    public class CellCountEntry
    {
        public CellType cellType;
        public int count = 0;
    }

    public static CellManager Instance { get; private set; }
    public delegate void OnCellBirth(CellType cellType);
    public static event OnCellBirth CellBirth;
    private Dictionary<CellType, CellInfo> cells = new Dictionary<CellType, CellInfo>();
    
    [SerializeField]
    private List<CellCountEntry> countsDisplay = new List<CellCountEntry>();


    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize cells dictionary with CellInfo for each type
        foreach (CellType ct in System.Enum.GetValues(typeof(CellType)))
        {
            cells[ct] = new CellInfo { cellType = ct};
            countsDisplay.Add(new CellCountEntry { cellType = ct });
        }

        // Load prefabs from Resources folder
        cells[CellType.TPC].prefab = Resources.Load<GameObject>("Cells/totipotentStemCell");
        cells[CellType.NSC].prefab = Resources.Load<GameObject>("Cells/neuralStemCell");
        cells[CellType.GMC].prefab = Resources.Load<GameObject>("Cells/ganglionMotherCell");
        cells[CellType.RGC].prefab = Resources.Load<GameObject>("Cells/radialGlialCell");
        cells[CellType.NB].prefab = Resources.Load<GameObject>("Cells/neuroblast");
        cells[CellType.N].prefab = Resources.Load<GameObject>("Cells/neuron");
    }

    public GameObject GetPrefab(CellType cellType)
    {
        if (cells.TryGetValue(cellType, out CellInfo cellInfo) && cellInfo.prefab != null)
        {
            return cellInfo.prefab;
        }
        else
        {
            Debug.LogError($"CellManager: No prefab found for CellType {cellType}");
            return null;
        }
    }

    public void RegisterCell(Cell cellInstance)
    {
        if (cells.TryGetValue(cellInstance.cellType, out CellInfo cellInfo))
        {
            cellInfo.cellList.Add(cellInstance);
            cellInfo.lifetimeCount++;
            CellBirth?.Invoke(cellInstance.cellType);
            UpdateDisplayCount(cellInstance.cellType);
        }
    }

    public void UnregisterCell(Cell cellInstance)
    {
        if (cells.TryGetValue(cellInstance.cellType, out CellInfo cellInfo))
        {
            cellInfo.cellList.Remove(cellInstance);
            UpdateDisplayCount(cellInstance.cellType);
        }
    }

    private void UpdateDisplayCount(CellType cellType)
    {
        var entry = countsDisplay.Find(e => e.cellType == cellType);
        if (entry != null && cells.TryGetValue(cellType, out CellInfo cellInfo))
        {
            entry.count = cellInfo.cellList.Count;
        }
    }

    public int GetCellCount(CellType cellType)
    {
        if (cells.TryGetValue(cellType, out CellInfo cellInfo))
        {
            return cellInfo.cellList.Count;
        }
        return 0;
    }

    public void KillCells(CellType ct, int count)
    {
        if (!cells.TryGetValue(ct, out CellInfo cellInfo) || cellInfo.cellList.Count == 0)
        {
            Debug.LogWarning($"CellManager: No cells of type {ct} available to kill.");
            return;
        }
        
        int cellsToKill = Mathf.Min(count, cellInfo.cellList.Count);
        
        if (cellsToKill < count)
        {
            Debug.LogWarning($"CellManager: Attempted to kill {count} cells of type {ct}, but only {cellsToKill} are available.");
        }
        
        for (int i = 0; i < cellsToKill; i++)
        {
            int index = Random.Range(0, cellInfo.cellList.Count);
            Cell cell = cellInfo.cellList[index];
            cellInfo.cellList.RemoveAt(index);
            cell.Apoptosis();
        }
    }
}
