namespace UConfig.Test.UnitTest
{
    using System.Dynamic;
    using System.Xml.Linq;
    using Core;
    using FluentAssertions;
    using Xunit;

    public class UpgraderTests
    {
        [Fact]
        public void When_I_add_a_setting_to_the_config_Then_it_is_added_to_the_xml_document()
        {
            XElement oldTree = new XElement("configuration",
                new XElement("ExampleString", "test"),
                new XElement("ExampleNumber", 2)
            );


            Upgrader upgrader = new Upgrader(oldTree);
            dynamic configuration = new ExpandoObject();
            configuration.AddedNumber = "3";
            upgrader.AddEntry(configuration);
            upgrader.Apply();

            oldTree.Element("AddedNumber").Value.Should().Be("3");
        }
    }
}