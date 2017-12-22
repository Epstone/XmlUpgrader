namespace UConfig.Core
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Xml.Linq;

    internal class FileUpgrader
    {
        private readonly UpgradePlan upgradePlan;
        private readonly ConfigurationFile configFile;


        public FileUpgrader(UpgradePlan upgradePlan, ConfigurationFile configFile)
        {
            this.upgradePlan = upgradePlan;
            this.configFile = configFile;
        }


        public ConfigurationFile Upgrade()
        {
            var treeToUpgrade = new XElement(this.configFile.Document);

            //foreach (var entry in upgradePlan.AddedValues)
            //{
                expandoToXML(upgradePlan.AddedValues, "", treeToUpgrade);
            //}

            return new ConfigurationFile()
            {
                Document = treeToUpgrade,
                Version = this.upgradePlan.UpgradeToVersion,
            };
        }

        private XElement expandoToXML(dynamic node, string nodeName, XElement treeToUpgrade)
        {
            XElement xmlNode;
            if (string.IsNullOrEmpty(nodeName))
            {
                xmlNode = treeToUpgrade;
            }
            else
            {
                xmlNode = new XElement(nodeName);
            }


            foreach (var property in (IDictionary<string, object>)node)
            {
                if (property.Value.GetType() == typeof(ExpandoObject))

                {
                    xmlNode.Add(expandoToXML(property.Value, property.Key, xmlNode));
                }

                else if (property.Value.GetType() == typeof(List<dynamic>))

                {
                    foreach (var element in (List<dynamic>)property.Value)
                    {
                        xmlNode.Add(expandoToXML(element, property.Key, xmlNode));
                    }
                }

                else
                {
                    xmlNode.Add(new XElement(property.Key, property.Value));
                }
            }

            return xmlNode;
        }
    }
}
