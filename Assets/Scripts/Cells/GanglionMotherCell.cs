using UnityEngine;

public class GanglionMotherCell : Cell
{
    protected override CellType cellType => CellType.GMC;
    [SerializeField] private short gmcCount = 1;
    public override void Click()
    {
        base.Click();
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
