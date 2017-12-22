namespace UConfig.Core
{
    public class UpgradePlan
    {
        public UpgradePlan AddSettingsToAdd(dynamic elementsToAdd)
        {
            this.AddedSettings = elementsToAdd;
            return this;
        }

        public UpgradePlan AddSettingsToRename(RenameMap renameMap)
        {
            this.RenamedSettings = renameMap;
            return this;
        }

        internal RenameMap RenamedSettings { get; set; }

        internal dynamic AddedSettings { get; set; }
        public int UpgradeToVersion { get; internal set; }

        public UpgradePlan SetVersion(int version)
        {
            this.UpgradeToVersion = version;
            return this;
        }
    }
}