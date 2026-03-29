using UnityEngine;
using System;

[RequireComponent(typeof(SphereCollider))]
public class CellNodeClickable : MonoBehaviour, IClickable
{
    [SerializeField] CellType cellType;

    public CellType CellType => cellType;

    public static event Action<CellNodeClickable> Clicked;

    public void Click()
    {
        Debug.Log($"CellNodeClickable: clicked {cellType} on {gameObject.name}");
        Clicked?.Invoke(this);
    }
}
