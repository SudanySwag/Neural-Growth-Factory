using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public sealed class UpgradeDef{
        public string Id;
        public string Title;
        public string Description;
        public Func<short, int> Cost;
        public short Level;
        public short maxLevel;
        public string[] Prereqs;
        public Vector2 Position;  // for your draggable evo tree layout
    }

public sealed class UpgradeCatalog
{
    public readonly Dictionary<string, UpgradeDef> ById;

    public UpgradeCatalog(IEnumerable<UpgradeDef> defs)
        => ById = defs.ToDictionary(d => d.Id);
}

public sealed class UpgradeState
{
    public event Action Changed;

    public string SelectedId { get; private set; }
    public HashSet<string> Unlocked { get; } = new();

    public bool IsUnlocked(string id) => Unlocked.Contains(id);

    public void Select(string id)
    {
        SelectedId = id;
        Changed?.Invoke();
    }

    public void Unlock(string id)
    {
        if (Unlocked.Add(id))
            Changed?.Invoke();
    }
}
