using UnityEngine;

public class NeuralStemCell : Cell
{
    public override CellType cellType => CellType.NSC;

    protected override void OnClick()
    {
        Split();
    }

    void Update () {
        if (busy) return;
        if (Upgrades.Instance.IsUpgradeUnlocked("auto-divide"))
            Split();
    }

    void Split() {
        Divide(Upgrades.Instance.IsUpgradeUnlocked("gmc-basic") ? 
            CellManager.Instance.GetPrefab(CellType.GMC) : 
            CellManager.Instance.GetPrefab(CellType.RGC));
    }
}
