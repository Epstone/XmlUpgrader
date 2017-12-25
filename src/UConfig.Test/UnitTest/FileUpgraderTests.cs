namespace UConfig.Test.UnitTest
{
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Xml.Linq;
    using Core;
    using Examples;
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
                var xmlTree = new XElement("configuration");
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
            ConfigurationFile configFile = ConfigurationFile.FromXElement(DefaultXmlVersionOne);

            dynamic elementsToAdd = new ExpandoObject();
            elementsToAdd.AddedNumber = "3";

            var upgradePlan = new UpgradePlan()
            .AddElements(elementsToAdd)
            .SetVersion(2);

            var fileUpgrader = new FileUpgrader(upgradePlan, configFile);

            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Element("AddedNumber").Value.Should().Be("3");
            upgradedConfig.Version.Should().Be(2);
        }

        [Fact]
        public void AddElementStructure()
        {
            ConfigurationFile configFile = ConfigurationFile.FromXElement(DefaultXmlVersionOne);

            dynamic elementsToAdd = new ExpandoObject();
            elementsToAdd.AddedStructure = new ExpandoObject();
            elementsToAdd.AddedStructure.SettingOne = "works";

            var upgradePlan = new UpgradePlan()
            .SetVersion(2)
            .AddElements(elementsToAdd);

            var fileUpgrader = new FileUpgrader(upgradePlan, configFile);
            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Element("AddedStructure").Element("SettingOne").Value.Should().Be("works");
        }

        [Fact]
        public void ExtendDeepElementStructure()
        {
            ConfigurationFile configFile = ConfigurationFile.FromXElement(DeepStructuredXmlVersionOne);

            dynamic elementsToAdd = new ExpandoObject();
            elementsToAdd.ExampleStructure = new ExpandoObject();
            elementsToAdd.ExampleStructure.DeepSettingTwo = "Two";

            var upgradePlan = new UpgradePlan()
                                    .SetVersion(2)
                                    .AddElements(elementsToAdd);

            var fileUpgrader = new FileUpgrader(upgradePlan, configFile);
            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Element("ExampleStructure").Element("DeepSettingOne").Value.Should().Be("One");
            upgradedConfig.Document.Element("ExampleStructure").Element("DeepSettingTwo").Value.Should().Be("Two");
            upgradedConfig.Document.Elements("ExampleStructure").Count().Should().Be(1);
        }

        [Fact]
        public void RenameElement()
        {
            ConfigurationFile configFile = ConfigurationFile.FromXElement(DefaultXmlVersionOne);

            //var map = new RenameMap()
            dynamic renameMap = new ExpandoObject();    
            renameMap.ExampleStringRenamed = "/ExampleString";

            var upgradePlan = new UpgradePlan()
                .SetVersion(2)
                .RenameElements(renameMap);

            var fileUpgrader = new FileUpgrader(upgradePlan, configFile);
            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Element("ExampleString").Should().BeNull();
            upgradedConfig.Document.Element("ExampleStringRenamed").Value.Should().Be("test");
        }

        [Fact]
        public void MoveElement()
        {
            ConfigurationFile configFile = ConfigurationFile.FromXElement(DefaultXmlVersionOne);

            //var map = new RenameMap()
            dynamic renameMap = new ExpandoObject();
            renameMap.TestStructure = new ExpandoObject();
            renameMap.TestStructure.MovedSetting = "/ExampleString";

            var upgradePlan = new UpgradePlan()
                .SetVersion(2)
                .RenameElements(renameMap);

            var fileUpgrader = new FileUpgrader(upgradePlan, configFile);
            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document. Element("ExampleString").Should().BeNull();
            upgradedConfig.Document.Should().HaveElement("TestStructure").Which.
                Should().HaveElement("MovedSetting").Which.Value.Should().Be("test");
        }

        // todo remove setting
        // todo load xml without version has flag set
        // todo upgrade for two versions
        // todo too high upgrade version throws exception
    }
}