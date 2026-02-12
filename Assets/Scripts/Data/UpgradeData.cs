using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// List of upgrades
/// </summary>
public static class UpgradeData
{
    public static UpgradeCatalog CreateCatalog()
    {
        var upgrades = new List<UpgradeDef>
        {
            // GMC Tier 1 - Unlock GMC spawning (cost: 10 RGC)
            new UpgradeDef
            {
                Id = "gmc-basic",
                Title = "GMC Production",
                Description = "Unlocks the ability to spawn Ganglion Mother Cells. Begin building your neural network.",
                Level = 0,
                maxLevel = 1,
                Cost = level => 10,
                Prereqs = new string[] {},
                Position = new Vector2(50, 25)
            },

            // GMC Tier 2 - Enhanced GMC (cost: 100 RGC)
            new UpgradeDef
            {
                Id = "gmc-enhanced",
                Title = "GMC Enhancement",
                Description = "Improves GMC capabilities and efficiency. Enhanced cells produce better offspring.",
                Level = 0,
                maxLevel = 1,
                Cost = level => 100,
                Prereqs = new[] {"gmc-basic"},
                Position = new Vector2(50, 50)
            },

            // GMC Tier 3 - Advanced GMC (cost: 1000 RGC)
            new UpgradeDef
            {
                Id = "gmc-advanced",
                Title = "GMC Amplification",
                Description = "Maximizes GMC potential. Unlocks the most powerful neural growth capabilities.",
                Level = 0,
                maxLevel = 1,
                Cost = level => 1000,
                Prereqs = new[] {"gmc-enhanced"},
                Position = new Vector2(50, 75)
            },

            new UpgradeDef
            {
                Id = "auto-divide",
                Title = "Auto-Divide",
                Description = "Automatically splits neural cells when they reach a certain size.",
                Level = 0,
                maxLevel = 1,
                Cost = level => 100,
                Prereqs = new string[] {},
                Position = new Vector2(25, 50)
            },
        };

        return new UpgradeCatalog(upgrades);
    }
}
