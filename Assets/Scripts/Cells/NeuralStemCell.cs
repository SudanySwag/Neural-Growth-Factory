using UnityEngine;

public class NeuralStemCell : Cell
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject radialGlialCellPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(1f, 0f, 0f);
    [SerializeField] private bool spawnOnce = true;

    private bool hasSpawned;

    public override void Click()
    {
        base.Click();

        if (!radialGlialCellPrefab)
        {
            Debug.LogError("NeuralStemCell: radialGlialCellPrefab is not assigned.", this);
            return;
        }

        Divide(radialGlialCellPrefab);
    }
}
