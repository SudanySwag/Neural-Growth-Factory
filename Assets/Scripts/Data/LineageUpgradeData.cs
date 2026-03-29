using System.Collections.Generic;
using UnityEngine;

public static class LineageUpgradeData
{
    public static UpgradeCatalog CreateCatalog()
    {
        var upgrades = new List<UpgradeDef>
        {
            new UpgradeDef
            {
                Id = "lineage-alpha",
                Title = "Lineage Alpha",
                Description = "First lineage upgrade. Placeholder.",
                Level = 0,
                maxLevel = 3,
                Cost = level => 20 + (level * 10),
                Prereqs = new string[] {},
                Position = new Vector2(50, 25)
            },
            new UpgradeDef
            {
                Id = "lineage-beta",
                Title = "Lineage Beta",
                Description = "Second lineage upgrade. Placeholder.",
                Level = 0,
                maxLevel = 2,
                Cost = level => 50 + (level * 25),
                Prereqs = new[] { "lineage-alpha" },
                Position = new Vector2(50, 55)
            },
        };

        return new UpgradeCatalog(upgrades);
    }
}
