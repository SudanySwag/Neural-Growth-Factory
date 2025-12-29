using UnityEngine;

public class NeuralStemCell : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject radialGlialCellPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(1f, 0f, 0f);
    [SerializeField] private bool spawnOnce = true;

    private bool hasSpawned;

    public void Activate()
    {
        if (spawnOnce && hasSpawned) return;

        if (!radialGlialCellPrefab)
        {
            Debug.LogError("NeuralStemCell: radialGlialCellPrefab is not assigned.", this);
            return;
        }

        Instantiate(radialGlialCellPrefab, transform.position + spawnOffset, transform.rotation);
        hasSpawned = true;

        if (spawnOnce)
            Destroy(gameObject);
    }
}
