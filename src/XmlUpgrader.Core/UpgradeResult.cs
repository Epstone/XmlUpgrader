namespace XmlUpgrader.Core
{
    using System;

    public class UpgradeResult
    {
        public bool UpgradeNeeded { get; set; }
        public Version UpgradedFromVersion { get; set; }
        public Version UpgradedToVersion { get; set; }
    }
}