using UnityEngine;

public class NeuralStemCell : Cell
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(1f, 0f, 0f);
    protected override CellType cellType => CellType.NSC;


    public override void Click()
    {
        base.Click();

        Divide(CellManager.Instance.GetPrefab(CellType.GMC));
    }
}
