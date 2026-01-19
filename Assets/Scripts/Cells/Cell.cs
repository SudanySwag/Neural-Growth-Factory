using System.Collections;
using UnityEngine;

public abstract class Cell : MonoBehaviour, IClickable
{
    [Header("Division Timing")]
    [SerializeField] private float prepTime = 0.25f;
    [SerializeField] private float splitTime = 0.25f;
    [SerializeField] private float settleTime = 0.15f;

    [Header("Division Shape")]
    [SerializeField] private float squashAmount = 0.20f;     // 0.2 = 20%
    [SerializeField] private float separationDistance = 0.6f;

    protected bool busy = false;
    public abstract CellType cellType { get; }

    void Start()
    {
        CellManager.Instance.RegisterCell(this);
    }

    public void freeCell()
    {
        busy = false;
    }

    virtual public void Click()
    {
        print($"{this.GetType().FullName} clicked");
    }

    public void Differentiate(GameObject newForm)
    {
        if (busy) return;
        StartCoroutine(DifferentiateRoutine(newForm));
    }

    private IEnumerator DifferentiateRoutine(GameObject child)
    {
        if (!child) yield break;
        busy = true;

        Vector3 baseScale = transform.localScale;
        Vector3 targetScale = child.transform.localScale;

        // 1) Prep: set initial squash
        Vector3 squashed = new Vector3(0, 0, 0);

        // 2) Squash
        yield return TweenScale(baseScale, squashed, prepTime * 1.5f);

        // 3) Replace with daughter
        GetComponent<SphereCollider>().enabled = false;

        GameObject daughter = Instantiate(child, transform.position, transform.rotation, transform.parent);
        daughter.transform.localScale = squashed; // match squashed look initially
        yield return TweenScale(squashed, targetScale, settleTime, daughter.transform);
        busy = false;
        Destroy(gameObject);
    }

    public void Divide(GameObject child)
    {
        if (busy) return;
        StartCoroutine(DivideRoutine(child));
    }

    private IEnumerator DivideRoutine(GameObject child)
    {
        busy = true;

        Vector3 baseScale = transform.localScale;
        Vector3 axis = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

        // 1) Prep: squash and stretch (like pressure building)
        Vector3 squashed = new Vector3(
            baseScale.x * (1f - squashAmount),
            baseScale.y * (1f + squashAmount),
            baseScale.z
        );

        yield return TweenScale(baseScale, squashed, prepTime);

        // 2) Spawn daughter at same position
        GameObject daughter = Instantiate(child, transform.position, transform.rotation, transform.parent);
        daughter.transform.localScale = squashed; // match squashed look initially
        if (daughter.name.EndsWith("Prefab(Clone)"))
            daughter.name = daughter.name.Substring(0, daughter.name.Length - 13);
        daughter.name = char.ToUpper(daughter.name[0]) + daughter.name.Substring(1);

        // daughter.transform.SetParent(ObjectManager.GetObjectAtPath("World/Cells/" + daughter.name + "s"), true);

        // 3) Split: separate both halves outward
        Vector3 p0 = transform.position;
        Vector3 pA = p0 - axis * (separationDistance * 0.5f);
        Vector3 pB = p0 + axis * (separationDistance * 0.5f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, splitTime);
            float s = Smooth01(t);

            transform.position = Vector3.Lerp(p0, pA, s);
            daughter.transform.position = Vector3.Lerp(p0, pB, s);

            yield return null;
        }

        // 4) Settle: both return to normal scale
        StartCoroutine(TweenScale(squashed, baseScale, settleTime, daughter.transform));
        yield return TweenScale(squashed, baseScale, settleTime);

        busy = false;
    }

    private IEnumerator TweenScale(Vector3 from, Vector3 to, float duration, Transform target = null)
    {
        Transform tr = target ? target : transform;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float s = Smooth01(t);
            tr.localScale = Vector3.Lerp(from, to, s);
            yield return null;
        }
        tr.localScale = to;
    }

    private static float Smooth01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x); // smoothstep
    }

    void OnDestroy()
    {
        CellManager.Instance.UnregisterCell(this);
    }
}
