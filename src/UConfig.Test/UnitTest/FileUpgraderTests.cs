namespace UConfig.Test.UnitTest
{
    using System;
    using System.Collections.Generic;
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
        private Version Version2Oh = new Version(2,0);
        private Version Version3Oh = new Version(3,0);

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
                xmlTree.SetAttributeValue("version", "1.0");
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
                .AddElements(elementsToAdd).SetVersion(Version2Oh);
            

            var fileUpgrader = new FileUpgrader(upgradePlan, configFile);

            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Element("AddedNumber").Value.Should().Be("3");
            upgradedConfig.Version.Should().Be(Version2Oh);
        }

        [Fact]
        public void AddElementStructure()
        {
            ConfigurationFile configFile = ConfigurationFile.FromXElement(DefaultXmlVersionOne);

            dynamic elementsToAdd = new ExpandoObject();
            elementsToAdd.AddedStructure = new ExpandoObject();
            elementsToAdd.AddedStructure.SettingOne = "works";

            var upgradePlan = new UpgradePlan().SetVersion(Version2Oh)
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

            var upgradePlan = new UpgradePlan().SetVersion(Version2Oh)
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

            var upgradePlan = new UpgradePlan().SetVersion(Version2Oh)
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

            dynamic renameMap = new ExpandoObject();
            renameMap.TestStructure = new ExpandoObject();
            renameMap.TestStructure.MovedSetting = "/ExampleString";

            var upgradePlan = new UpgradePlan().SetVersion(Version2Oh)
                .RenameElements(renameMap);

            var fileUpgrader = new FileUpgrader(upgradePlan, configFile);
            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Element("ExampleString").Should().BeNull();
            upgradedConfig.Document.Should().HaveElement("TestStructure").Which.
                Should().HaveElement("MovedSetting").Which.Value.Should().Be("test");
        }

        [Fact]
        public void RemoveElement()
        {
            ConfigurationFile configFile = ConfigurationFile.FromXElement(DefaultXmlVersionOne);

            var removeElements = new List<string>();

            removeElements.Add("/ExampleString");

            var test = new ExpandoObject();

            var upgradePlan = new UpgradePlan().SetVersion(Version2Oh)
                .RemoveElements(removeElements).AddElements(test);

            var fileUpgrader = new FileUpgrader(upgradePlan, configFile);
            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Should().NotBeNull();
            upgradedConfig.Document.Element("ExampleString").Should().BeNull();
        }

        [Fact]
        public void MissingVersionAttributeIsHandledAsVersionOne()
        {
            var configXml = DefaultXmlVersionOne;
            configXml.Attribute("version").Should().NotBeNull();
            configXml.Attribute("version").Remove();
            configXml.Attribute("version").Should().BeNull();

            var upgradableFile = ConfigurationFile.FromXElement(configXml);

            var upgradePlan = new UpgradePlan().SetVersion(Version2Oh)
                .RemoveElements(new[] { "/ExampleString" });

            var fileUpgrader = new FileUpgrader(upgradePlan, upgradableFile);

            ConfigurationFile upgradedFile = fileUpgrader.Upgrade();
            upgradedFile.Version.Should().Be(Version2Oh);
        }


        [Fact]
        public void UpgradeWithHigherInitialVersionThanOne()
        {
            var config = DefaultXmlVersionOne;
            config.Attribute("version").Value = Version2Oh.ToString();

            var configFile = ConfigurationFile.FromXElement(config);
            configFile.Version.Should().Be(Version2Oh);


            dynamic elementsToAdd = new ExpandoObject();
            elementsToAdd.AddedElementIsHere = "test-value";
            UpgradePlan plan = new UpgradePlan()
                .SetVersion(Version3Oh)
                .AddElements(elementsToAdd);



            var upgrader = new FileUpgrader(plan, configFile);
            ConfigurationFile upgradedConfig = upgrader.Upgrade();
            upgradedConfig.Version.Should().Be(Version3Oh);
        }

        [Fact]
        public void JustMessingAroundWithVersionCode()
        {
            var oneoh = new Version("1.0");
            var oneohohoh = new Version("1.0.0.0");
            var oneohohone = new Version("1.0.0.1");
            (oneoh == oneohohoh).Should().Be(false);
            (oneoh < oneohohoh).Should().Be(true);
            (oneoh > oneohohoh).Should().Be(false);
            oneohohone.Should().BeGreaterThan(oneoh);
            oneohohone.Should().BeGreaterThan(oneohohoh);
        }

        // todo allow versions with major.minor.revision.build number
        // todo upgrade for two versions
        // todo too high upgrade version throws exception
    }
}