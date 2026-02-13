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
    private bool active = false;

    public void SetDocument(UIDocument document)
    {
        doc = document;
    }

    private void SetRGCCount(CellType cellType)
    {
        if (cellType == CellType.RGC)
            count.text = CellManager.Instance.GetCellCount(CellType.RGC).ToString();
    }

    private void SlideDown(CellType cellType) {
        if (cellType == CellType.RGC && CellManager.Instance.GetCellCount(CellType.RGC) >= 100) {
            counter.style.top = new Length(0, LengthUnit.Percent);
            CellManager.CellBirth -= SlideDown;
            active = true;
        }
    }
    void OnEnable()
    {
        var uiDoc = doc ? doc : GetComponent<UIDocument>();
        if (uiDoc == null) return;

        root = uiDoc.rootVisualElement;
        if (root == null) return;

        counter = root.Q<VisualElement>("Counter");
        count = root.Q<Label>("Count");

        if (counter != null && !active) {
            counter.style.top = new Length(-150, LengthUnit.Percent);
            CellManager.CellBirth += SlideDown;
        }
    }

    void OnDisable()
    {
        CellManager.CellBirth -= SlideDown;
    }

    public void Update()
    {
        SetRGCCount(CellType.RGC);
    }

}
