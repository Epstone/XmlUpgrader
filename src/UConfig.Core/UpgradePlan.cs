namespace UConfig.Core
{
    using System.Dynamic;

    public class UpgradePlan
    {
        public UpgradePlan AddElements(dynamic elementsToAdd)
        {
            this.AddedSettings = elementsToAdd;
            return this;
        }

        public UpgradePlan RenameElements(dynamic elementsToRename)
        {
            this.RenamedSettings = elementsToRename;
            return this;
        }

        internal dynamic RenamedSettings { get; set; }

        internal dynamic AddedSettings { get; set; } 
        public int UpgradeToVersion { get; internal set; }

        public UpgradePlan SetVersion(int version)
        {
            this.UpgradeToVersion = version;
            return this;
        }
    }
}