namespace UConfig.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class Upgrader
    {
        private readonly List<UpgradePlan> upgradePlans = new List<UpgradePlan>();
        private readonly List<ConfigurationFile> configFiles = new List<ConfigurationFile>();
        private string[] xmlFilePaths;

        public void AddEntry(dynamic xpath)
        {
            upgradePlans.Add(xpath);
        }

        public void Register<T>() where T : IUpgradableConfig, new()
        {
            ReadUpgradPlan(new T());
        }

        //public void RegisterAll(Assembly assembly)
        //{
        //    IEnumerable<Type> types = assembly.GetTypes().Where(t => typeof(IUpgradableConfig).IsAssignableFrom(t));
        //    Registrations.AddRange(types);

        //    foreach (Type registration in Registrations)
        //    {
        //        ReadUpgradPlan((IUpgradableConfig)Activator.CreateInstance(registration));
        //    }
        //}

        //public void AddXmlConfigurationDir(string directory)
        //{
        //    XmlConfigurationDirectory = directory;
        //    xmlFilePaths = Directory.GetFiles(XmlConfigurationDirectory, "*.xml");

        //    if (xmlFilePaths.Length == 0)
        //    {
        //        throw new InvalidOperationException("no xml configuration files found");
        //    }
        //    configFiles.AddRange(xmlFilePaths.Select(ConfigurationFile.LoadXml));
        //}

        public void Verify()
        {
            if (extendedRegistrations.Count == 1)
            {
                throw new InvalidOperationException("just one configuration file found, nothing to upgrade.");
            }

            extendedRegistrations.ForEach(x=>x.LoadFile());

            // verify, that we have an upgrade script to the next version
            var registrations = extendedRegistrations.OrderBy(x => x.Version).ToArray();
            for (var i = 1; i < registrations.Length - 1; i++) 
            {
                var registration = registrations[i];
                var nextRegistration = registrations[i + 1];

                var upgrader = new FileUpgrader(registration.GetUpgradePlan(), registration.File);
                ConfigurationFile upgradeConfig = upgrader.Upgrade();

                // execute the update and compare with reference version
                upgradeConfig.VerifyEqualTo(nextRegistration.File);
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

        public void AddRegistration(Version version, string filePath, Type type = null)
        {
            this.extendedRegistrations.Add(new Registration(filePath, version, type));
        }

        private readonly List<Registration> extendedRegistrations = new List<Registration>();
    }

    internal class Registration
    {
        public Version Version { get; }
        private Type type;

        public Registration(string filePath, Version version, Type type)
        {
            FilePath = filePath;
            this.Version = version;
            this.type = type;
        }

        internal string FilePath { get; set; }

        public void LoadFile()
        {
            this.File = ConfigurationFile.LoadXml(this.FilePath);
        }

        public ConfigurationFile File { get; set; }

        public UpgradePlan GetUpgradePlan()
        {
            return ((IUpgradableConfig) Activator.CreateInstance(type)).GetUpgradePlan();
        }
    }
}