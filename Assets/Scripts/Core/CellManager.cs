using System;
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
    private Dictionary<CellType, int> counts = ((CellType[])System.Enum.GetValues(typeof(CellType)))
    .ToDictionary(ct => ct, ct => 0);
        [System.Serializable]
    public class CellCountEntry
    {
        public CellType cellType;
        public int count;
    }

    [SerializeField]
    private List<CellCountEntry> countsDisplay = new List<CellCountEntry>();


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
        foreach (CellType ct in Enum.GetValues(typeof(CellType)))   countsDisplay.Add(new CellCountEntry { cellType = ct, count = 0 });

        
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

    public void RegisterCell(CellType cellType)
    {
        counts[cellType]++;
        UpdateDisplayCount(cellType);
    }

    public void UnregisterCell(CellType cellType)
    {
        if (counts[cellType] > 0)
        {
            counts[cellType]--;
            UpdateDisplayCount(cellType);
        }
    }

    private void UpdateDisplayCount(CellType cellType)
    {
        var entry = countsDisplay.Find(e => e.cellType == cellType);
        if (entry != null)
        {
            entry.count = counts[cellType];
        }
    }
}
