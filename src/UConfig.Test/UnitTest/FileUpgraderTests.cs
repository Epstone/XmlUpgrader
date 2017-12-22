namespace UConfig.Test.UnitTest
{
    using System.Dynamic;
    using System.Linq;
    using System.Xml.Linq;
    using Core;
    using FluentAssertions;
    using Xunit;

    public class FileUpgraderTests
    {
        private XElement DefaultXmlVersionOne
        {
            get
            {
                var xml = new XElement(ConfigVersionOne);
                xml.Add(
                    new XElement("ExampleString", "test"),
                    new XElement("ExampleNumber", 2)
                );
                return xml;
            }
        }

        private XElement ConfigVersionOne
        {
            get
            {
                XElement xmlTree = new XElement("configuration");
                xmlTree.SetAttributeValue("version", 1);
                return xmlTree;
            }
        }

        private XElement DeepStructuredXmlVersionOne
        {
            get
            {
                var xml = new XElement(ConfigVersionOne);
                xml.Add(
                    new XElement("ExampleStructure", new XElement("DeepSettingOne", "One"))
                );
                return xml;
            }
        }

        [Fact]
        public void AddElement()
        {
            var configFile = ConfigurationFile.FromXElement(DefaultXmlVersionOne);

            dynamic elementsToAdd = new ExpandoObject();
            elementsToAdd.AddedNumber = "3";

            UpgradePlan upgradePlan = new UpgradePlan(elementsToAdd)
            {
                UpgradeToVersion = 2
            };
            FileUpgrader fileUpgrader = new FileUpgrader(upgradePlan, configFile);

            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Element("AddedNumber").Value.Should().Be("3");
            upgradedConfig.Version.Should().Be(2);
        }

        [Fact]
        public void AddElementStructure()
        {
            var configFile = ConfigurationFile.FromXElement(DefaultXmlVersionOne);

            dynamic elementsToAdd = new ExpandoObject();
            elementsToAdd.AddedStructure = new ExpandoObject();
            elementsToAdd.AddedStructure.SettingOne = "works";

            UpgradePlan upgradePlan = new UpgradePlan(elementsToAdd)
            {
                UpgradeToVersion = 2
            };

            FileUpgrader fileUpgrader = new FileUpgrader(upgradePlan, configFile);
            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Element("AddedStructure").Element("SettingOne").Value.Should().Be("works");
        }

        [Fact]
        public void ExtendDeepElementStructure()
        {
            var configFile = ConfigurationFile.FromXElement(DeepStructuredXmlVersionOne);

            dynamic elementsToAdd = new ExpandoObject();
            elementsToAdd.ExampleStructure = new ExpandoObject();
            elementsToAdd.ExampleStructure.DeepSettingTwo = "Two";

            UpgradePlan upgradePlan = new UpgradePlan(elementsToAdd)
            {
                UpgradeToVersion = 2
            };

            FileUpgrader fileUpgrader = new FileUpgrader(upgradePlan, configFile);
            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Element("ExampleStructure").Element("DeepSettingOne").Value.Should().Be("One");
            upgradedConfig.Document.Element("ExampleStructure").Element("DeepSettingTwo").Value.Should().Be("Two");
            upgradedConfig.Document.Elements("ExampleStructure").Count().Should().Be(1);
        }

        // todo add setting structure 2 levels deep
        // todo rename setting
        // todo remove setting
        // todo load xml without version has flag set
        // todo upgrade for two versions
        // todo too high upgrade version throws exception
    }
}