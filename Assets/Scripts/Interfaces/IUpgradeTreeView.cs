using System;
using System.Collections.Generic;

public interface IUpgradeTreeView
{
    event Action<string> NodeClicked;
    event Action<string> UpgradeClicked;

    void BuildTree(IEnumerable<UpgradeDef> upgrades);     // create buttons
    void SetSelected(string id);
    void SetUnlocked(string id, bool unlocked);
    void SetUpgradeLevel(string id, short level, short maxLevel, bool canPurchase);
}
