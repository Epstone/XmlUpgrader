namespace XmlUpgrader.Core
{
    using System.Xml.Linq;
    using MigrationStrategy;

    internal class OneVersionUpgrader
    {
        private readonly XmlFile xmlFile;
        private readonly UpgradePlan upgradePlan;
        private XElement workingTree;


        internal OneVersionUpgrader(UpgradePlan upgradePlan, XmlFile xmlFile)
        {
            this.upgradePlan = upgradePlan;
            this.xmlFile = xmlFile;
        }

        internal XmlFile Upgrade()
        {
            workingTree = new XElement(xmlFile.Document);

            if (upgradePlan.RemovedElements != null)
            {
                RemoveStrategy removeStrategy = new RemoveStrategy(workingTree, upgradePlan.RemovedElements);
                removeStrategy.Execute();
            }

            if (upgradePlan.RenamedElements != null)
            {
                RenameStrategy renameStrategy = new RenameStrategy(workingTree, upgradePlan.RenamedElements);
                renameStrategy.Execute();
            }

            if (upgradePlan.AddedElements != null)
            {
                ExtensionStrategy strategy = new ExtensionStrategy(workingTree, upgradePlan.AddedElements);
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