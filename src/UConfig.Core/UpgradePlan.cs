namespace UConfig.Core
{
    using System;
    using System.Collections.Generic;

    public class UpgradePlan
    {
        public UpgradePlan AddElements(dynamic elementsToAdd)
        {
            AddedSettings = elementsToAdd;
            return this;
        }

        public UpgradePlan RenameElements(dynamic elementsToRename)
        {
            RenamingSettings = elementsToRename;
            return this;
        }

        public UpgradePlan SetVersion(Version version)
        {
            UpgradeToVersion = version;
            return this;
        }

        public UpgradePlan SetVersion(string version)
        {
            UpgradeToVersion = new Version(version);
            return this;
        }

        public UpgradePlan RemoveElements(IEnumerable<string> removeElements)
        {
            RemovedElements.AddRange(removeElements);
            return this;
        }

        internal dynamic RenamingSettings { get; set; }

        internal dynamic AddedSettings { get; set; }
        
        public List<string> RemovedElements { get; set; } = new List<string>();

        public Version UpgradeToVersion { get; internal set; }
    }
}