using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
public sealed class UpgradeTreeView : MonoBehaviour, IUpgradeTreeView
{
    [SerializeField] UIDocument doc;
    public event Action<string> NodeClicked;

    VisualElement root, content, nodesLayer;
    readonly Dictionary<string, Button> nodeById = new();
    bool initialized = false;

    public void SetDocument(UIDocument document)
    {
        doc = document;
        initialized = false; // Force re-initialization with new document
    }

    void EnsureInitialized()
    {
        if (initialized) return;

        var uiDoc = doc ? doc : GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("UpgradeTreeView: No UIDocument found! Please assign a UIDocument in the Inspector or add a UIDocument component.");
            return;
        }

        root = uiDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("UpgradeTreeView: UIDocument root is null!");
            return;
        }

        nodesLayer = root.Q<VisualElement>("Lineage");

        if (nodesLayer == null)
        {
            Debug.LogError("UpgradeTreeView: Could not find 'Lineage' element in UIDocument. Check that the UXML structure is correct.");
            Debug.Log($"Root element: {root.name}, children: {root.childCount}");

            // Debug: print all elements in hierarchy
            DebugPrintHierarchy(root, 0);
        }
        else
        {
            initialized = true;
            Debug.Log("UpgradeTreeView: Successfully initialized and found Lineage element");
        }
    }

    void DebugPrintHierarchy(VisualElement element, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}{element.GetType().Name} name='{element.name}' class='{string.Join(",", element.GetClasses())}'");

        foreach (var child in element.Children())
        {
            DebugPrintHierarchy(child, depth + 1);
        }
    }

    public void BuildTree(IEnumerable<UpgradeDef> upgrades)
    {
        EnsureInitialized();

        if (nodesLayer == null)
        {
            Debug.LogError("UpgradeTreeView: nodesLayer is null. Cannot build tree.");
            return;
        }

        nodesLayer.hierarchy.Clear();
        nodeById.Clear();

        foreach (var u in upgrades)
        {
            var b = new Button { text = u.Title };
            b.AddToClassList("node");
            b.name = u.Id;

            b.style.left = u.Position.x;
            b.style.top  = u.Position.y;

            b.clicked += () => NodeClicked?.Invoke(u.Id);

            nodesLayer.hierarchy.Add(b);
            nodeById[u.Id] = b;
        }
    }

    public void SetSelected(string id)
    {
        EnsureInitialized();
        foreach (var kv in nodeById)
            kv.Value.EnableInClassList("is-selected", kv.Key == id);
    }

    public void SetUnlocked(string id, bool unlocked)
    {
        EnsureInitialized();
        if (nodeById.TryGetValue(id, out var b))
            b.EnableInClassList("is-unlocked", unlocked);
    }
}
