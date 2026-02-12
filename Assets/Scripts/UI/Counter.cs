using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

public class Counter : MonoBehaviour
{
    [SerializeField] UIDocument doc;

    VisualElement root;
    VisualElement counter;
    Label count;

    public void SetDocument(UIDocument document)
    {
        doc = document;
    }

    private void SetRGCCount(CellType cellType)
    {
        if (cellType == CellType.RGC)
            count.text = CellManager.Instance.GetCellCount(CellType.RGC).ToString();
    }

    private void slideDown(CellType cellType) {
        if (cellType == CellType.RGC && CellManager.Instance.GetCellCount(CellType.RGC) > 100) {
            counter.style.top = new Length(0, LengthUnit.Percent);
            CellManager.CellBirth -= slideDown;
        }
    }

    public void Awake()
    {
        var uiDoc = doc ? doc : GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("Counter: No UIDocument found! Please assign a UIDocument in the Inspector or add a UIDocument component.");
            return;
        }

        root = uiDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("Counter: UIDocument root is null!");
            return;
        }

        counter = root.Q<VisualElement>("Counter");
        count = root.Q<Label>("Count");

        counter.style.top = new Length(-150, LengthUnit.Percent);

        CellManager.CellBirth += slideDown;
    }

    public void Update()
    {
        SetRGCCount(CellType.RGC);
    }

}
