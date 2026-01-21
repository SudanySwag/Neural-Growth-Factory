using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains test upgrade data for development and testing purposes.
/// </summary>
public static class TestUpgradeData
{
    public static UpgradeCatalog CreateTestCatalog()
    {
        var upgrades = new List<UpgradeDef>
        {
            // Root upgrade
            new UpgradeDef
            {
                Id = "mitochondria",
                Title = "Mitochondria",
                Level = 0,
                maxLevel = 5,
                Cost = level => 10 + (level * 5),
                Prereqs = new string[] { },
                Position = new Vector2(150, 150)
            },

            // First tier upgrades
            new UpgradeDef
            {
                Id = "nucleus",
                Title = "Nucleus",
                Level = 0,
                maxLevel = 3,
                Cost = level => 25 + (level * 10),
                Prereqs = new[] { "mitochondria" },
                Position = new Vector2(50, 250)
            },

            new UpgradeDef
            {
                Id = "membrane",
                Title = "Cell Membrane",
                Level = 0,
                maxLevel = 4,
                Cost = level => 20 + (level * 8),
                Prereqs = new[] { "mitochondria" },
                Position = new Vector2(250, 250)
            },

            // Second tier upgrades
            new UpgradeDef
            {
                Id = "chloroplast",
                Title = "Chloroplast",
                Level = 0,
                maxLevel = 3,
                Cost = level => 40 + (level * 15),
                Prereqs = new[] { "mitochondria", "nucleus" },
                Position = new Vector2(50, 350)
            },

            new UpgradeDef
            {
                Id = "ribosome",
                Title = "Ribosome",
                Level = 0,
                maxLevel = 5,
                Cost = level => 35 + (level * 12),
                Prereqs = new[] { "nucleus" },
                Position = new Vector2(150, 350)
            },

            new UpgradeDef
            {
                Id = "golgi",
                Title = "Golgi Apparatus",
                Level = 0,
                maxLevel = 3,
                Cost = level => 30 + (level * 10),
                Prereqs = new[] { "nucleus", "membrane" },
                Position = new Vector2(250, 350)
            },

            // Advanced upgrades
            new UpgradeDef
            {
                Id = "reproduction",
                Title = "Reproduction",
                Level = 0,
                maxLevel = 2,
                Cost = level => 100 + (level * 50),
                Prereqs = new[] { "chloroplast", "ribosome", "golgi" },
                Position = new Vector2(150, 450)
            }
        };

        return new UpgradeCatalog(upgrades);
    }
}
