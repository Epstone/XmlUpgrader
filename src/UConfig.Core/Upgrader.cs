namespace UConfig.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    public class Upgrader
    {
        private XElement tree;

        private readonly List<UpgradePlan> upgradePlans = new List<UpgradePlan>();
        private string[] xmlFilePaths;
        private List<ConfigurationFile> configFiles = new List<ConfigurationFile>();

        public void AddEntry(dynamic xpath)
        {
            upgradePlans.Add(xpath);
        }

        public void Register<T>() where T : IUpgradableConfig, new()
        {
            ReadUpgradPlan(new T());
        }

        private void ReadUpgradPlan(IUpgradableConfig upgradableConfig)
        {
            UpgradePlan upgradePlan = upgradableConfig.GetUpgradePlan();
            upgradePlan.UpgradeToVersion = upgradableConfig.Version;
            upgradePlans.Add(upgradePlan);
        }

        public void RegisterAll(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(t => typeof(IUpgradableConfig).IsAssignableFrom(t));
            Registrations.AddRange(types);

            foreach (Type registration in Registrations)
            {
                this.ReadUpgradPlan((IUpgradableConfig)Activator.CreateInstance(registration));
            }
        }

        public void AddXmlConfigurationDir(string directory)
        {
            XmlConfigurationDirectory = directory;
            this.xmlFilePaths = Directory.GetFiles(XmlConfigurationDirectory, "*.xml");

            if (xmlFilePaths.Length == 0)
            {
                throw new InvalidOperationException("no xml configuration files found");
            }
            this.configFiles.AddRange(xmlFilePaths.Select(ConfigurationFile.LoadXml));
        }

        public void Verify()
        {

            if (xmlFilePaths.Length == 1)
            {
                throw new InvalidOperationException("just one configuration file found, nothing to upgrade.");
            }


            // verify, that we have an upgrade script to the next version
            var configurationFiles = configFiles.OrderBy(x => x.Version).ToArray();
            for (int i = 0; i < configurationFiles.Length - 1; i++) // do not upgrade to a version which is not existing yet
            {
                var configurationFile = configurationFiles[i];
                // todo no version gaps allowed
                UpgradePlan upgradePlan = this.upgradePlans.FirstOrDefault(m => m.UpgradeToVersion == configurationFile.Version + 1);
                if (upgradePlan == null)
                {
                    throw new InvalidOperationException($"No upgrade script for xml version {configurationFile.Version} found.");
                }

                FileUpgrader upgrader = new FileUpgrader(upgradePlan, configurationFile);
                ConfigurationFile upgradeConfig = upgrader.Upgrade();

                // execute the update and compare with reference version
                upgradeConfig.VerifyEqualTo(configurationFiles[i + 1]);
            }
        }

        public List<Type> Registrations { get; set; } = new List<Type>();

        public string XmlConfigurationDirectory { get; set; }

        public void UpgradeXml(string xmlToUpgrade)
        {
            var configToUpgrade = ConfigurationFile.LoadXml(xmlToUpgrade);
            foreach (UpgradePlan upgradePlan in upgradePlans.OrderBy(x => x.UpgradeToVersion))
            {
                var upgrader = new FileUpgrader(upgradePlan, configToUpgrade);
                configToUpgrade = upgrader.Upgrade();   
            }

            configToUpgrade.Document.Save(xmlToUpgrade);
        }
    }
}