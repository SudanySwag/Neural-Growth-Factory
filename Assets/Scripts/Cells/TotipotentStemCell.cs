using UnityEngine;

public class TotipotentStemCell : Cell
{
    public override CellType cellType => CellType.TPC;

    protected override void OnClick()
    {
        Differentiate(CellManager.Instance.GetPrefab(CellType.NSC));
    }
}
