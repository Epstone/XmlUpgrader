namespace UConfig.Core
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Xml.Linq;

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

        private XElement extendXml(dynamic node, string nodeName, XElement treeToUpgrade)
        {
            XElement xmlNode;
            if (string.IsNullOrEmpty(nodeName))
            {
                xmlNode = treeToUpgrade;
            }
            else
            {
                xmlNode = treeToUpgrade.Element(nodeName); // try to grab existing node
            }

            if (xmlNode == null)
            {
                xmlNode = new XElement(nodeName); // node not yet existing, create new
                treeToUpgrade.Add(xmlNode);
            }

            foreach (KeyValuePair<string, object> property in (IDictionary<string, object>)node)
            {
                if (IsExpandoObject(property.Value))
                {
                    extendXml(property.Value, property.Key, xmlNode);
                }
                else
                {
                    xmlNode.Add(new XElement(property.Key, property.Value));
                }
            }

            return xmlNode;
        }

        private static bool IsDynamicList(KeyValuePair<string, object> property)
        {
            return property.Value.GetType() == typeof(List<dynamic>);
        }

        private static bool IsExpandoObject(object propertyValue)
        {
            return propertyValue.GetType() == typeof(ExpandoObject);
        }
    }
}