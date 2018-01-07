namespace UConfig.Test.IntegrationTest
{
    using System;
    using Core;
    using Examples;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class UpgradeXmlTest
    {
        private static readonly string ConfigurationXmlDirectory = @"Examples\Xml\";


        [Fact]
        public void DetailedRegistration()
        {
            var upgrader = new Upgrader();

            upgrader.AddRegistration(new Version(1, 0), @"Examples\Xml\Config_v1.xml");
            upgrader.AddRegistration(new Version(2, 0), @"Examples\Xml\Config_v2.xml", typeof(ExampleConfigV2));

            upgrader.Verify();
        }
        

        [Fact]
        public void UpgradeXmlFile()
        {
            string xmlToUpgrade = $@"{ConfigurationXmlDirectory}Config_v1.xml";
            var xmlUpgradeReference = @"Examples\Xml\Config_v2.xml";

            var upgrader = new Upgrader();

            // apply upgrade method from config
            upgrader.Register<ExampleConfigV2>();

            // verify output xml compatible with reference xml
            upgrader.UpgradeXml(xmlToUpgrade);

            // load config via .net core configuration
            var updatedConfig = GetConfig<ExampleConfigV2>(xmlToUpgrade);
            var expected = GetConfig<ExampleConfigV2>(xmlUpgradeReference);

            updatedConfig.ShouldBeEquivalentTo(expected);
        }

        private static T GetConfig<T>(string xmlVersion1Path)
        {
            IConfigurationBuilder builderUpdated = new ConfigurationBuilder()
                .AddXmlFile(xmlVersion1Path);
            IConfigurationRoot configurationRootUpdated = builderUpdated.Build();
            return configurationRootUpdated.Get<T>();
        }
    }
}