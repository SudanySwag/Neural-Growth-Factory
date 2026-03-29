using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
public sealed class UpgradeTreeView : MonoBehaviour
{
    [SerializeField] UIDocument doc;
    [SerializeField] string panelName = "LineagePanel";
    public event Action<string> NodeClicked;
    public event Action<string> UpgradeClicked;

    VisualElement root, lineageContainer, nodesContainer;
    VisualElement detailPanel;
    Label nameLabel, descriptionLabel, levelLabel, costLabel;
    Button upgradeButton;
    readonly Dictionary<string, Button> nodeById = new();
    readonly Dictionary<string, UpgradeDef> upgradeById = new();
    string currentSelectedId;
    bool initialized = false;

    // Panning state
    bool isPanning;
    Vector2 panStart;
    Vector2 panOffset;
    const float PanThreshold = 5f; // Minimum drag distance to start panning
    bool dragStarted;

    public void SetDocument(UIDocument document)
    {
        doc = document;
        initialized = false;
    }

    public event Action ViewRestored;

    void OnEnable()
    {
        if (initialized)
        {
            initialized = false;
            ViewRestored?.Invoke();
        }
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

        var panel = root.Q<VisualElement>(panelName);
        lineageContainer = panel?.Q<VisualElement>("Lineage");

        if (lineageContainer == null)
        {
            Debug.LogError($"UpgradeTreeView: Could not find 'Lineage' inside '{panelName}'. Check that the UXML structure is correct.");
            Debug.Log($"Root element: {root.name}, children: {root.childCount}");

            // Debug: print all elements in hierarchy
            DebugPrintHierarchy(root, 0);
        }
        else
        {
            // Initialize nodes container for panning
            nodesContainer = lineageContainer.Q<VisualElement>("NodesContainer");

            // Initialize detail panel elements
            detailPanel = lineageContainer.Q<VisualElement>("NodeDetailPanel");
            nameLabel = lineageContainer.Q<Label>("NodeName");
            descriptionLabel = lineageContainer.Q<Label>("NodeDescription");
            levelLabel = lineageContainer.Q<Label>("NodeLevel");
            costLabel = lineageContainer.Q<Label>("NodeCost");
            upgradeButton = lineageContainer.Q<Button>("UpgradeButton");

            if (upgradeButton != null)
            {
                upgradeButton.clicked += () =>
                {
                    if (!string.IsNullOrEmpty(currentSelectedId))
                        UpgradeClicked?.Invoke(currentSelectedId);
                };
            }

            // Set up panning events on the lineage container
            SetupPanning();

            initialized = true;
        }
    }

    void SetupPanning()
    {
        // Use TrickleDown to capture events before children process them
        lineageContainer.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        lineageContainer.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
        lineageContainer.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        lineageContainer.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    void OnPointerDown(PointerDownEvent evt)
    {
        // Only pan with left mouse button or primary touch
        if (evt.button != 0) return;

        // Don't start panning if clicking on the detail panel or its children (including upgrade button)
        if (IsChildOf(evt.target as VisualElement, detailPanel))
            return;

        isPanning = false;
        dragStarted = true;
        panStart = evt.position;
        lineageContainer.CapturePointer(evt.pointerId);
    }

    void OnPointerMove(PointerMoveEvent evt)
    {
        if (!dragStarted) return;

        Vector2 delta = (Vector2)evt.position - panStart;

        // Check if we've moved enough to start panning
        if (!isPanning && delta.magnitude > PanThreshold)
        {
            isPanning = true;
        }

        if (isPanning && nodesContainer != null)
        {
            panOffset += (Vector2)evt.position - panStart;
            panStart = evt.position;

            nodesContainer.style.left = panOffset.x;
            nodesContainer.style.top = panOffset.y;

            // Stop event from propagating to prevent moving view underneath
            evt.StopPropagation();
        }
    }

    void OnPointerUp(PointerUpEvent evt)
    {
        if (!dragStarted) return;

        bool wasPanning = isPanning;
        lineageContainer.ReleasePointer(evt.pointerId);
        dragStarted = false;
        isPanning = false;

        // Stop propagation if we were panning to prevent affecting views underneath
        if (wasPanning)
        {
            evt.StopPropagation();
        }
    }

    void OnPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        dragStarted = false;
        isPanning = false;
    }

    bool IsChildOf(VisualElement element, VisualElement parent)
    {
        if (element == null || parent == null) return false;
        var current = element;
        while (current != null)
        {
            if (current == parent) return true;
            current = current.parent;
        }
        return false;
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

        if (nodesContainer == null)
        {
            Debug.LogError("UpgradeTreeView: nodesContainer is null. Cannot build tree.");
            return;
        }

        nodesContainer.Clear();
        nodeById.Clear();
        upgradeById.Clear();

        foreach (var u in upgrades)
        {
            var b = new Button { text = u.Title };
            b.AddToClassList("node");
            b.AddToClassList("is-locked"); // Default to locked
            b.name = u.Id;

            b.style.position = Position.Absolute;
            b.style.left = new Length(u.Position.x, LengthUnit.Percent);
            b.style.top  = new Length(u.Position.y, LengthUnit.Percent);

            b.clicked += () =>
            {
                // Only fire click if we weren't panning
                if (!isPanning)
                    NodeClicked?.Invoke(u.Id);
            };

            nodesContainer.Add(b);
            nodeById[u.Id] = b;
            upgradeById[u.Id] = u;
        }
    }

    public void SetSelected(string id)
    {
        EnsureInitialized();
        currentSelectedId = id;

        foreach (var kv in nodeById)
            kv.Value.EnableInClassList("is-selected", kv.Key == id);

        // Update detail panel
        if (detailPanel != null)
        {
            if (!string.IsNullOrEmpty(id) && upgradeById.TryGetValue(id, out var upgrade))
            {
                nameLabel.text = upgrade.Title;
                descriptionLabel.text = upgrade.Description ?? "";
                levelLabel.text = $"Level {upgrade.Level} / {upgrade.maxLevel}";
                if (upgrade.Level >= upgrade.maxLevel)
                    costLabel.text = "MAX";
                else
                    costLabel.text = $"Cost: {upgrade.Cost(upgrade.Level)} RGC";
                detailPanel.EnableInClassList("visible", true);
            }
            else
            {
                detailPanel.EnableInClassList("visible", false);
            }
        }
    }

    public void SetUnlocked(string id, bool unlocked)
    {
        EnsureInitialized();
        if (nodeById.TryGetValue(id, out var b))
            b.EnableInClassList("is-unlocked", unlocked);
    }

    public void SetUpgradeLevel(string id, short level, short maxLevel, bool prereqs, bool canPurchase)
    {
        EnsureInitialized();
        if (!nodeById.TryGetValue(id, out var b)) return;

        // Clear all state classes
        b.RemoveFromClassList("is-locked");
        b.RemoveFromClassList("is-available");
        b.RemoveFromClassList("is-affordable");
        b.RemoveFromClassList("is-partial");
        b.RemoveFromClassList("is-maxed");

        if (level >= maxLevel)
        {
            // Fully upgraded
            b.AddToClassList("is-maxed");
        }
        else if (level > 0)
        {
            // Partially upgraded
            b.AddToClassList("is-partial");
        }
        else if (canPurchase)
        {
            // Affordable
            b.AddToClassList("is-affordable");
        }
        else if (prereqs)
        {
            // Available to purchase (prerequisites met)
            b.AddToClassList("is-available");
        }
        else
        {
            // Locked (prerequisites not met)
            b.AddToClassList("is-locked");
        }
    }
}
