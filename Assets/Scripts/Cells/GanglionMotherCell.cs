using UnityEngine;

public class GanglionMotherCell : Cell
{
    public override CellType cellType => CellType.GMC;
    [SerializeField] private short gmcCount = 1;

    void Start() {

        gmcCount = 0;
        if (Upgrades.Instance.IsUpgradeUnlocked("gmc-advanced"))
            gmcCount = 3;
        else if (Upgrades.Instance.IsUpgradeUnlocked("gmc-enhanced"))
            gmcCount = 2;
        else if (Upgrades.Instance.IsUpgradeUnlocked("gmc-basic"))
            gmcCount = 1;
        
        float colorValue = Mathf.Lerp(0.2f, 1f, gmcCount / 3f); // 0->1.0, 3->0.5
        GetComponent<Renderer>().material.color *= new Color(colorValue, colorValue, colorValue);

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
