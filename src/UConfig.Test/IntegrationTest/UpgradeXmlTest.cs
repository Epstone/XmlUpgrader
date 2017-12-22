namespace UConfig.Test.IntegrationTest
{
    using Core;
    using Examples;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class UpgradeXmlTest
    {
        private static readonly string ConfigurationXmlDirectory = @"Examples\Xml\";

        [Fact]
        public void UpgradeXmlFile()
        {
            var xmlToUpgrade = $@"{ConfigurationXmlDirectory}Config_v1.xml";
            var xmlUpgradeReference = @"Examples\Xml\Config_v2.xml";
            
            Upgrader upgrader = new Upgrader();

            // apply upgrade method from config
            upgrader.Register<ExampleConfigV2>();

            // verify output xml compatible with reference xml
            upgrader.UpgradeXml(xmlToUpgrade);

            // load config via .net core configuration
            var updatedConfig = GetConfig<ExampleConfigV2>(xmlToUpgrade);
            var expected = GetConfig<ExampleConfigV2>(xmlUpgradeReference);

            updatedConfig.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void AutomaticVerificationProcess()
        {
            // register all upgradable interfaces
            var upgrader = new Upgrader();
            upgrader.RegisterAll(this.GetType().Assembly);
            
            // register all config files
            upgrader.AddXmlConfigurationDir(ConfigurationXmlDirectory);

            // call verify to get an automated verification of the upgrades
            upgrader.Verify();
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