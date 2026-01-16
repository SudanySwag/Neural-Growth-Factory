using UnityEngine;

public class RadialGlialCell : Cell
{
    [SerializeField] private RingManager ring;
    [SerializeField] private GameObject neuronPrefab;

    [Header("Spawn Control")]
    [SerializeField] private bool spawnOnClick = true;
    [SerializeField] private int cellsCountForGrowth = 1; // how many "cells" each neuron adds for radius

    void Start()
    {
        cellType = CellType.RGC;
        CellManager.Instance.RegisterCell(cellType);
    }
    public void SpawnNeuron()
    {
        if (!ring || !neuronPrefab) return;

        Vector3 pos = ring.GetNextNeuronSpawnPosition();
        Divide(neuronPrefab);

        // Expand ring based on your rule
        ring.RegisterNewCell(cellsCountForGrowth);
    }

    public override void Click()
    {
        base.Click();
        if (spawnOnClick) SpawnNeuron();
    }
}
