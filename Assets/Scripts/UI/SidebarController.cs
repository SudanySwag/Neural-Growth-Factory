using UnityEngine;
using UnityEngine.UIElements;

public class SidebarController : MonoBehaviour
{
    [SerializeField] UIDocument sidebarDocument;
    [SerializeField] GameObject[] environments;

    const int SIDEBAR_UNLOCK_THRESHOLD = 100;

    GameObject[] views;
    VisualElement sidebarRoot;

    void Start()
    {
        // Cache each environment's View child
        views = new GameObject[environments.Length];
        for (int i = 0; i < environments.Length; i++)
        {
            if (environments[i] != null)
            {
                Transform view = environments[i].transform.Find("View");
                if (view != null) views[i] = view.gameObject;
            }
        }

        sidebarRoot = sidebarDocument.rootVisualElement;
        var sidebarGroup = sidebarRoot.Q<RadioButtonGroup>("RadioButtonGroup");

        sidebarGroup.RegisterValueChangedCallback(OnSidebarChanged);

        // Hide sidebar until threshold is reached
        sidebarRoot.style.display = DisplayStyle.None;
        CellManager.CellBirth += CheckUnlock;

        SetScreen(0);
    }

    void OnDisable()
    {
        CellManager.CellBirth -= CheckUnlock;
    }

    void CheckUnlock(CellType cellType)
    {
        if (cellType == CellType.RGC &&
            CellManager.Instance.GetCellCount(CellType.RGC) >= SIDEBAR_UNLOCK_THRESHOLD)
        {
            sidebarRoot.style.display = DisplayStyle.Flex;
            sidebarRoot.Q<RadioButtonGroup>("RadioButtonGroup").value = 0;
            CellManager.CellBirth -= CheckUnlock;
        }
    }

    void OnSidebarChanged(ChangeEvent<int> evt)
    {
        if (evt.newValue < 0) return;
        SetScreen(evt.newValue);
    }

    void SetScreen(int index)
    {
        if (index < 0 || index >= environments.Length) return;
        // Show only the active environment's view
        for (int i = 0; i < views.Length; i++)
        {
            if (views[i] != null)
                views[i].SetActive(i == index);
        }
    }
}
