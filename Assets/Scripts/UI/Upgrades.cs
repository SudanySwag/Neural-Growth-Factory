using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
public class Lineage : MonoBehaviour
{
    // Reference to the UIDocument component (assign in the Inspector)
    public UIDocument document;
    static public short GMCLevel = 0;

    void OnEnable()
    {
        CellManager.CellBirth += HandleCellBirth;
        // Get the root visual element
        var root = document.rootVisualElement;

        // Query for the button by its name in the UXML (e.g., "my-button")
        Button myButton = root.Q<Button>("mgc-test");

        // Check if the button was found and add a listener to its clicked event
        if (myButton != null)
        {
            myButton.clicked += ButtonClicked;
        }
    }

    void OnDisable()
    {
        CellManager.CellBirth -= HandleCellBirth;
    }

    //Hide screen until certain count
    void HandleCellBirth(CellType cellType)
    {
        if (cellType == CellType.RGC && CellManager.Instance.GetCellCount(CellType.RGC) >= 10)
        {
            document.rootVisualElement.Q<VisualElement>("LineageHolder").style.opacity = 1f;
            document.rootVisualElement.Q<VisualElement>("LineageHolder").SetEnabled(true);
        }
    }

    // The method called when the button is clicked
    private void ButtonClicked()
    {
        print("MGC Button Clicked!");
        switch(GMCLevel)
        {
            case 0:
                if (CellManager.Instance.GetCellCount(CellType.RGC) < 10) return;
                CellManager.Instance.KillCells(CellType.RGC, 10);
                GMCLevel++;
                break;
            case 1:
                if (CellManager.Instance.GetCellCount(CellType.RGC) < 100) return;
                CellManager.Instance.KillCells(CellType.RGC, 100);
                GMCLevel++;
                break;
            case 2:
                if (CellManager.Instance.GetCellCount(CellType.RGC) < 1000) return;
                CellManager.Instance.KillCells(CellType.RGC, 1000);
                GMCLevel++;
                break;
        }
    }
}
