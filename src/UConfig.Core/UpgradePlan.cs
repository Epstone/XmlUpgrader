namespace UConfig.Core
{
    using System.Collections.Generic;

    public class UpgradePlan
    {
        internal dynamic RenamingSettings { get; set; }

        internal dynamic AddedSettings { get; set; }

        public int UpgradeToVersion { get; internal set; }

        public List<string> RemovedElements { get; set; } = new List<string>();

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

        public UpgradePlan SetVersion(int version)
        {
            UpgradeToVersion = version;
            return this;
        }

        public UpgradePlan RemoveElements(IEnumerable<string> removeElements)
        {
            RemovedElements.AddRange(removeElements);
            return this;
        }
    }
}