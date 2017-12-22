namespace UConfig.Test.IntegrationTest
{
    using System;
    using System.IO;
    using System.Xml.Linq;
    using Core;
    using Examples;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class UpgradeXmlTest
    {
        [Fact]
        public void UpgradeXml()
        {
            var xmlVersion1Path = @"Examples\Xml\Config_v1.xml";
            var xmlVersion2Path = @"Examples\Xml\Config_v2.xml";
            XElement configV1 = XElement.Load(xmlVersion1Path);

            Upgrader upgrader = new Upgrader(configV1);

            // apply upgrade method from config
            upgrader.Register<ExampleConfigV2>();

            // verify output xml compatible with reference xml
            XElement v2 = upgrader.Apply();
            string updatedConfigPath = SaveAsTemporaryConfig(v2);

            // load config via .net core configuration
            var updatedConfig = GetConfig<ExampleConfigV2>(updatedConfigPath);
            var expected = GetConfig<ExampleConfigV2>(xmlVersion2Path);

            updatedConfig.ShouldBeEquivalentTo(expected);
        }


        private string SaveAsTemporaryConfig(XElement updatedConfig)
        {
            string tempConfigPath = Path.GetTempPath() + Guid.NewGuid() + ".xml";
            updatedConfig.Save(tempConfigPath);
            return tempConfigPath;
        }

        private static T GetConfig<T>(string xmlVersion1Path)
        {
            var builderUpdated = new ConfigurationBuilder()
                .AddXmlFile(xmlVersion1Path);
            IConfigurationRoot configurationRootUpdated = builderUpdated.Build();
            return configurationRootUpdated.Get<T>();
        }
    }
}