using UnityEngine;

public class NeuralStemCell : Cell
{
    public override CellType cellType => CellType.NSC;

    protected override void OnClick()
    {
        Divide(Upgrades.GMCLevel > 0 ? CellManager.Instance.GetPrefab(CellType.GMC) : CellManager.Instance.GetPrefab(CellType.RGC));
    }
}
