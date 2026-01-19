using UnityEngine;

public class GanglionMotherCell : Cell
{
    public override CellType cellType => CellType.GMC;
    [SerializeField] private short gmcCount = 1;

    void Start() {
        gmcCount = Lineage.GMCLevel;

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
