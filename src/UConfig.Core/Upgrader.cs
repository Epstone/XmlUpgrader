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
        private readonly List<UpgradePlan> upgradePlans = new List<UpgradePlan>();
        private readonly List<ConfigurationFile> configFiles = new List<ConfigurationFile>();
        private XElement tree;
        private string[] xmlFilePaths;

        public void AddEntry(dynamic xpath)
        {
            upgradePlans.Add(xpath);
        }

        public void Register<T>() where T : IUpgradableConfig, new()
        {
            ReadUpgradPlan(new T());
        }

        public void RegisterAll(Assembly assembly)
        {
            IEnumerable<Type> types = assembly.GetTypes().Where(t => typeof(IUpgradableConfig).IsAssignableFrom(t));
            Registrations.AddRange(types);

            foreach (Type registration in Registrations)
            {
                ReadUpgradPlan((IUpgradableConfig) Activator.CreateInstance(registration));
            }
        }

        public void AddXmlConfigurationDir(string directory)
        {
            XmlConfigurationDirectory = directory;
            xmlFilePaths = Directory.GetFiles(XmlConfigurationDirectory, "*.xml");

            if (xmlFilePaths.Length == 0)
            {
                throw new InvalidOperationException("no xml configuration files found");
            }
            configFiles.AddRange(xmlFilePaths.Select(ConfigurationFile.LoadXml));
        }

        public void Verify()
        {
            if (xmlFilePaths.Length == 1)
            {
                throw new InvalidOperationException("just one configuration file found, nothing to upgrade.");
            }


            // verify, that we have an upgrade script to the next version
            ConfigurationFile[] configurationFiles = configFiles.OrderBy(x => x.Version).ToArray();
            for (var i = 0; i < configurationFiles.Length - 1; i++) // do not upgrade to a version which is not existing yet
            {
                ConfigurationFile configurationFile = configurationFiles[i];
                // todo no version gaps allowed
                UpgradePlan upgradePlan = upgradePlans.FirstOrDefault(m => m.UpgradeToVersion == configurationFile.Version + 1);
                if (upgradePlan == null)
                {
                    throw new InvalidOperationException($"No upgrade script for xml version {configurationFile.Version} found.");
                }

                var upgrader = new FileUpgrader(upgradePlan, configurationFile);
                ConfigurationFile upgradeConfig = upgrader.Upgrade();

                // execute the update and compare with reference version
                upgradeConfig.VerifyEqualTo(configurationFiles[i + 1]);
            }
        }

        public void UpgradeXml(string xmlToUpgrade)
        {
            ConfigurationFile configToUpgrade = ConfigurationFile.LoadXml(xmlToUpgrade);
            foreach (UpgradePlan upgradePlan in upgradePlans.OrderBy(x => x.UpgradeToVersion))
            {
                var upgrader = new FileUpgrader(upgradePlan, configToUpgrade);
                configToUpgrade = upgrader.Upgrade();
            }

            configToUpgrade.Document.Save(xmlToUpgrade);
        }

        public List<Type> Registrations { get; set; } = new List<Type>();

        public string XmlConfigurationDirectory { get; set; }

        private void ReadUpgradPlan(IUpgradableConfig upgradableConfig)
        {
            UpgradePlan upgradePlan = upgradableConfig.GetUpgradePlan();
            upgradePlan.UpgradeToVersion = upgradableConfig.Version;
            upgradePlans.Add(upgradePlan);
        }
    }
}