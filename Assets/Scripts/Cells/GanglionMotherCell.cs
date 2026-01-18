using UnityEngine;

public class GanglionMotherCell : Cell
{
    public override CellType cellType => CellType.GMC;
    [SerializeField] private short gmcCount = 1;

    void Start() {
        busy = true;
        CellManager.Instance.RegisterCell(this);
        Invoke(nameof(freeCell), 1f);
    }

    void Update()
    {
        if (busy) return;   
        if (gmcCount > 0)
        {
            gmcCount--;
            Divide(CellManager.Instance.GetPrefab(CellType.RGC));
        } else
        {
            Differentiate(CellManager.Instance.GetPrefab(CellType.RGC));
        }
    }

    
}
