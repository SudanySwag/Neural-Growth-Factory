using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour
{
    [Header("Prefabs Container")]
    [SerializeField] private GameObject prefabContainer;

    public static CellManager Instance { get; private set; }

    [Serializable]
    public struct Entry {public string className; public GameObject prefab;}

    private Dictionary<Type, GameObject> map;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        foreach (var cell in prefabContainer.GetComponentsInChildren<Cell>())
        {
            // Create a empty game object with cell name in World/Cells/
            GameObject cellInstance = new GameObject(cell.name + "s");
            cellInstance.transform.SetParent(ObjectManager.GetObjectAtPath("World/Cells").transform, true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
