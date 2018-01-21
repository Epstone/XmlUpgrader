namespace XmlUpgrader.Core
{
    using System.Xml.Linq;

    internal class OneVersionUpgrader
    {
        private readonly XmlFile configFile;
        private readonly UpgradePlan upgradePlan;
        private XElement workingTree;


        internal OneVersionUpgrader(UpgradePlan upgradePlan, XmlFile configFile)
        {
            this.upgradePlan = upgradePlan;
            this.configFile = configFile;
        }

        internal XmlFile Upgrade()
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

            return new XmlFile
            {
                Document = workingTree,
                Version = upgradePlan.UpgradeToVersion
            };
        }
    }
}