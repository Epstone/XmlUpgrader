namespace UConfig.Core
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
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

            if (upgradePlan.RenamedSettings != null)
            {
                this.FindXmlNode(upgradePlan.RenamedSettings, "", workingTree);
            }

            if (upgradePlan.AddedSettings != null)
            {
                extendXml(upgradePlan.AddedSettings, "", workingTree);
            }

            return new ConfigurationFile
            {
                Document = workingTree,
                Version = upgradePlan.UpgradeToVersion
            };
        }

        internal XElement FindXmlNode(dynamic nodeToFind, string nodeName, XElement treeToUpgrade)
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

            if (xmlNode == null)
            {
                throw new InvalidOperationException($"cannot find node with name {nodeToFind}");
            }

            foreach (KeyValuePair<string, object> property in (IDictionary<string, object>)nodeToFind)
            {
                if (property.Value is string valueString)
                {
                    XElement oldNode = this.workingTree.XPathSelectElement(valueString);
                    // find old node and move and rename
                    oldNode.Remove();
                    oldNode.Name = property.Key;
                    xmlNode.Add(oldNode);
                }
                else if (IsExpandoObject(property.Value))
                {
                    FindXmlNode(property.Value, property.Key, xmlNode);
                }
                else
                {
                    xmlNode = FindXmlNode(property.Value, property.Key, xmlNode);
                }
            }

            return xmlNode;
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