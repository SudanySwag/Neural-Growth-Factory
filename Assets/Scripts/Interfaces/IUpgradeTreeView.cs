using System;
using System.Collections.Generic;

public interface IUpgradeTreeView
{
    event Action<string> NodeClicked;

    void BuildTree(IEnumerable<UpgradeDef> upgrades);     // create buttons
    void SetSelected(string id);
    void SetUnlocked(string id, bool unlocked);
}
