namespace UConfig.Core
{
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Xml.XPath;

    internal class FileUpgrader
    {
        private readonly ConfigurationFile configFile;
        private readonly UpgradePlan upgradePlan;
        private XElement workingTree;


        public FileUpgrader(UpgradePlan upgradePlan, ConfigurationFile configFile)
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

    internal class RemoveStrategy
    {
        private XElement workingTree;
        private List<string> removedElements;

        public RemoveStrategy(XElement workingTree, List<string> removedElements)
        {
            this.workingTree = workingTree;
            this.removedElements = removedElements;
        }

        public void Execute()
        {
            removedElements.ForEach(x =>
                {
                    XElement selectedElement = this.workingTree.XPathSelectElement(x);
                    if (selectedElement != null)
                    {
                        selectedElement.Remove();
                    }
                }
            );
        }
    }
}