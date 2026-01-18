using UnityEngine;

public class TotipotentStemCell : Cell
{
    public override CellType cellType => CellType.TPC;

    public override void Click()
    {
        base.Click();

        Differentiate(CellManager.Instance.GetPrefab(CellType.NSC));
    }
}
