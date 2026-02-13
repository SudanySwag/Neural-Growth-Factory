using UnityEngine;
using UnityEngine.UIElements;

public class SidebarController : MonoBehaviour
{
    [SerializeField] UIDocument sidebarDocument;
    [SerializeField] GameObject[] environments;

    GameObject[] views;

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

        var sidebarRoot = sidebarDocument.rootVisualElement;
        var sidebarGroup = sidebarRoot.Q<RadioButtonGroup>("RadioButtonGroup");

        sidebarGroup.RegisterValueChangedCallback(OnSidebarChanged);

        SetScreen(0);
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
