using UnityEngine;

public class NeuralStemCell : Cell
{
    public override CellType cellType => CellType.NSC;

    public override void Click()
    {
        base.Click();

        Divide(Lineage.GMCCheck ? CellManager.Instance.GetPrefab(CellType.GMC) : CellManager.Instance.GetPrefab(CellType.RGC));
    }
}
