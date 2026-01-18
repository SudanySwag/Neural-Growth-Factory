using UnityEngine;

public class RadialGlialCell : Cell
{
    protected override CellType cellType => CellType.RGC;

    public override void Click()
    {
        base.Click();
        // if (spawnOnClick) Divide(CellManager.Instance.GetPrefab(CellType.N));;
    }
}
