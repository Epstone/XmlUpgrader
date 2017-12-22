namespace UConfig.Test.UnitTest
{
    using System.Dynamic;
    using System.Xml.Linq;
    using Core;
    using FluentAssertions;
    using Xunit;

    public class FileUpgraderTests
    {
        [Fact]
        public void When_I_add_a_setting_to_the_config_Then_it_is_added_to_the_xml_document()
        {
            XElement oldTree = new XElement("configuration",
                new XElement("ExampleString", "test"),
                new XElement("ExampleNumber", 2)
            );
            ConfigurationFile configFile = new ConfigurationFile
            {
                Document = oldTree,
                Version = 1
            };

            dynamic elementsToAdd = new ExpandoObject();
            elementsToAdd.AddedNumber = "3";

            UpgradePlan upgradePlan = new UpgradePlan(elementsToAdd);
            FileUpgrader fileUpgrader = new FileUpgrader(upgradePlan, configFile);

            ConfigurationFile upgradedConfig = fileUpgrader.Upgrade();
            upgradedConfig.Document.Element("AddedNumber").Value.Should().Be("3");
        }
    }
}