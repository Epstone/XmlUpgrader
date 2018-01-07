namespace UConfig.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Upgrader
    {
        private readonly List<Registration> extendedRegistrations = new List<Registration>();

        public void Verify()
        {
            if (extendedRegistrations.Count == 1)
            {
                throw new InvalidOperationException("just one configuration file found, nothing to upgrade.");
            }

            extendedRegistrations.ForEach(x => x.LoadFile());

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

        public UpgradeResult UpgradeXml(string xmlToUpgrade)
        {
            ConfigurationFile configToUpgrade = ConfigurationFile.LoadXml(xmlToUpgrade);

            if (configToUpgrade.Version.Equals(extendedRegistrations.Max(x => x.Version)))
            {
                return new UpgradeResult
                {
                    UpgradeNeeded = false
                };
            }

            IEnumerable<Registration> upgradesToApply = extendedRegistrations
                .OrderBy(x => x.Version)
                .Where(x => x.Version > configToUpgrade.Version)
                .ToArray();

            foreach (var registration in upgradesToApply)
            {
                var upgrader = new FileUpgrader(registration.GetUpgradePlan(), configToUpgrade);
                configToUpgrade = upgrader.Upgrade();
            }

            configToUpgrade.Document.Save(xmlToUpgrade);

            return new UpgradeResult();
        }

        public void AddRegistration(Version version, string filePath, Type type = null)
        {
            extendedRegistrations.Add(new Registration(filePath, version, type));
        }
    }

    internal class Registration
    {
        private readonly Type type;

        public Registration(string filePath, Version version, Type type)
        {
            FilePath = filePath;
            Version = version;
            this.type = type;
        }

        public void LoadFile()
        {
            File = ConfigurationFile.LoadXml(FilePath);
        }

        public UpgradePlan GetUpgradePlan()
        {
            return ((IUpgradePlanProvider)Activator.CreateInstance(type)).GetUpgradePlan();
        }

        public Version Version { get; }

        internal string FilePath { get; set; }

        public ConfigurationFile File { get; set; }
    }
}