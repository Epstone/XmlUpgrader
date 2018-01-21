namespace UConfig.Core
{
    using System.Xml.Linq;

    internal class OneVersionUpgrader
    {
        private readonly ConfigurationFile configFile;
        private readonly UpgradePlan upgradePlan;
        private XElement workingTree;


        public OneVersionUpgrader(UpgradePlan upgradePlan, ConfigurationFile configFile)
        {
            this.upgradePlan = upgradePlan;
            this.configFile = configFile;
        }

        public ConfigurationFile Upgrade()
        {
            workingTree = new XElement(configFile.Document);

            if (upgradePlan.RemovedElements != null)
            {
                RemoveStrategy removeStrategy = new RemoveStrategy(workingTree, upgradePlan.RemovedElements);
                removeStrategy.Execute();
            }

            if (upgradePlan.RenamingSettings != null)
            {
                RenameStrategy renameStrategy = new RenameStrategy(workingTree, upgradePlan.RenamingSettings);
                renameStrategy.Execute();
            }

            if (upgradePlan.AddedSettings != null)
            {
                ExtensionStrategy strategy = new ExtensionStrategy(workingTree, upgradePlan.AddedSettings);
                strategy.Execute();
            }

            return new ConfigurationFile
            {
                Document = workingTree,
                Version = upgradePlan.UpgradeToVersion
            };
        }
    }
}