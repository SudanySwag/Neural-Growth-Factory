using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CellUpgradeData
{
    public static UpgradeCatalog CreateCatalog()
    {
        var upgrades = new List<UpgradeDef>
        {
            // TPC upgrades
            new UpgradeDef
            {
                Id = "tpc-division-rate",
                Title = "Division Rate",
                Description = "Increases TPC division speed.",
                Level = 0, maxLevel = 3,
                Cost = level => 10 + level * 15,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },
            new UpgradeDef
            {
                Id = "tpc-longevity",
                Title = "Longevity",
                Description = "TPC cells survive longer before differentiating.",
                Level = 0, maxLevel = 2,
                Cost = level => 20 + level * 20,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },

            // NSC upgrades
            new UpgradeDef
            {
                Id = "nsc-yield",
                Title = "Yield",
                Description = "NSC produces more daughter cells per division.",
                Level = 0, maxLevel = 3,
                Cost = level => 15 + level * 20,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },
            new UpgradeDef
            {
                Id = "nsc-potency",
                Title = "Potency",
                Description = "NSC can differentiate into more cell types.",
                Level = 0, maxLevel = 2,
                Cost = level => 30 + level * 25,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },

            // RGC upgrades
            new UpgradeDef
            {
                Id = "rgc-scaffold",
                Title = "Scaffold",
                Description = "RGC provides structural guidance for migrating cells.",
                Level = 0, maxLevel = 3,
                Cost = level => 20 + level * 15,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },
            new UpgradeDef
            {
                Id = "rgc-signaling",
                Title = "Signaling",
                Description = "RGC emits stronger growth signals.",
                Level = 0, maxLevel = 2,
                Cost = level => 40 + level * 30,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },

            // GMC upgrades
            new UpgradeDef
            {
                Id = "gmc-burst",
                Title = "Burst",
                Description = "GMC divides into more cells at once.",
                Level = 0, maxLevel = 3,
                Cost = level => 25 + level * 20,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },
            new UpgradeDef
            {
                Id = "gmc-efficiency",
                Title = "Efficiency",
                Description = "GMC consumes fewer resources during division.",
                Level = 0, maxLevel = 2,
                Cost = level => 50 + level * 30,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },

            // NB upgrades
            new UpgradeDef
            {
                Id = "nb-migration",
                Title = "Migration",
                Description = "Neuroblasts travel further before settling.",
                Level = 0, maxLevel = 3,
                Cost = level => 30 + level * 20,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },
            new UpgradeDef
            {
                Id = "nb-survival",
                Title = "Survival",
                Description = "More neuroblasts survive to become neurons.",
                Level = 0, maxLevel = 2,
                Cost = level => 60 + level * 35,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },

            // N upgrades
            new UpgradeDef
            {
                Id = "n-connectivity",
                Title = "Connectivity",
                Description = "Neurons form more synaptic connections.",
                Level = 0, maxLevel = 3,
                Cost = level => 50 + level * 25,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },
            new UpgradeDef
            {
                Id = "n-plasticity",
                Title = "Plasticity",
                Description = "Neurons adapt and strengthen over time.",
                Level = 0, maxLevel = 2,
                Cost = level => 80 + level * 40,
                Prereqs = new string[] {},
                Position = Vector2.zero
            },
        };

        return new UpgradeCatalog(upgrades);
    }

    public static IEnumerable<UpgradeDef> GetUpgradesForType(UpgradeCatalog catalog, CellType cellType)
    {
        string prefix = cellType.ToString().ToLowerInvariant() + "-";
        return catalog.ById.Values.Where(u => u.Id.StartsWith(prefix));
    }
}
