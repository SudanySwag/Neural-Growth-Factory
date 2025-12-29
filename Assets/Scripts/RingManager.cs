using UnityEngine;

public class RingManager : MonoBehaviour
{
    [Header("Ring Geometry")]
    [SerializeField] private Transform ringCenter;          // set to your core's transform
    [SerializeField] private float baseRadius = 1.5f;       // starting ring radius
    [SerializeField] private float growthPerCell = 0.03f;   // how fast it expands
    [SerializeField] private float ringThickness = 0.25f;   // band thickness where neurons can spawn

    [Header("Distribution")]
    [SerializeField] private bool useGoldenAngle = true;    // nice even distribution without tracking slots

    private int totalCells;     // or just neurons, depending on your rule
    private int neuronIndex;    // used to place neurons around the ring

    public float Radius { get; private set; }

    void Awake()
    {
        if (!ringCenter) ringCenter = transform;
        RecomputeRadius();
    }

    public void RegisterNewCell(int count = 1)
    {
        totalCells += count;
        RecomputeRadius();
    }

    public Vector3 GetNextNeuronSpawnPosition()
    {
        // choose angle
        float angle;
        if (useGoldenAngle)
        {
            // golden angle in radians for even packing
            const float goldenAngle = 2.39996323f;
            angle = neuronIndex * goldenAngle;
        }
        else
        {
            angle = Random.value * Mathf.PI * 2f;
        }

        neuronIndex++;

        // spawn somewhere in a thin band around the radius
        float r = Radius + Random.Range(-ringThickness * 0.5f, ringThickness * 0.5f);

        Vector3 center = ringCenter.position;
        Vector3 offset = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
        return center + offset;
    }

    public Vector3 GetInnerAttachmentPointNear(Vector3 neuronPos)
    {
        // Projects toward center and clamps to inner edge of the ring
        Vector3 center = ringCenter.position;
        Vector3 dir = (neuronPos - center);
        dir.z = 0f;

        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.right;

        dir.Normalize();

        float innerR = Mathf.Max(0.01f, Radius - ringThickness * 0.5f);
        return center + dir * innerR;
    }

    private void RecomputeRadius()
    {
        // Pick your growth curve:
        // Linear: base + totalCells * growthPerCell
        // Slightly gentler: base + sqrt(totalCells) * k
        Radius = baseRadius + totalCells * growthPerCell;
    }
}
