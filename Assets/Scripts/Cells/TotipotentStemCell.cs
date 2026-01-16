using UnityEngine;

public class TotipotentStemCell : Cell
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject neuralStemCellPrefab;

    private bool hasSpawned;

    void Start()
    {
        cellType = CellType.TPC;
        CellManager.Instance.RegisterCell(cellType);
    }
    public override void Click()
    {
        base.Click();

        if (!neuralStemCellPrefab)
        {
            Debug.LogError("totipotentStemCell: neuralStemCellPrefab is not assigned.", this);
            return;
        }

        Differentiate(neuralStemCellPrefab);
    }
}
