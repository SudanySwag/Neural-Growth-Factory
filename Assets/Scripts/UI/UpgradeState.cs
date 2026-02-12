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

    public bool IsUnlocked(string id) => ById.TryGetValue(id, out var def) && def.Level > 0;
    public int GetUpgradeLevel(string id) => ById.TryGetValue(id, out var def) ? def.Level : 0;
}