using UnityEngine;
using UnityEngine.UIElements;

public class SidebarController : MonoBehaviour
{
    [SerializeField] UIDocument sidebarDocument;
    [SerializeField] UIDocument upgradesDocument;
    [SerializeField] GameObject[] environments;

    VisualElement[] holders;
    int currentIndex;

    void Start()
    {
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
        currentIndex = index;

        // Switch 3D environments
        for (int i = 0; i < environments.Length; i++)
        {
            if (environments[i] != null)
                environments[i].SetActive(i == index);
        }
    }
}
