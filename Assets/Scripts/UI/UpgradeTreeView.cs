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

    void Awake()
    {
        root = (doc ? doc : GetComponent<UIDocument>()).rootVisualElement;
        content = root.Q<VisualElement>("Content");
        nodesLayer = root.Q<VisualElement>("Lineage");
    }

    public void BuildTree(IEnumerable<UpgradeDef> upgrades)
    {
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

        var v = new Button { text = "Testing" };
        v.AddToClassList("node");
        v.name = "test";

        v.style.left = 0;
        v.style.top  = 0;

        v.clicked += () => NodeClicked?.Invoke("test");

        nodesLayer.hierarchy.Add(v);
        nodeById["test"] = v;
    }

    public void SetSelected(string id)
    {
        foreach (var kv in nodeById)
            kv.Value.EnableInClassList("is-selected", kv.Key == id);
    }

    public void SetUnlocked(string id, bool unlocked)
    {
        if (nodeById.TryGetValue(id, out var b))
            b.EnableInClassList("is-unlocked", unlocked);
    }
}
