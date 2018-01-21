namespace XmlUpgrader.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class XmlFileUpgrader
    {
        private readonly List<Registration> extendedRegistrations = new List<Registration>();

        public void Verify()
        {
            if (extendedRegistrations.Count == 1)
            {
                throw new InvalidOperationException("just one XML file found, nothing to upgrade.");
            }

            extendedRegistrations.ForEach(x => x.LoadFile());

            // verify, that we have an upgrade script to the next version
            var registrations = extendedRegistrations.OrderBy(x => x.Version).ToArray();
            for (var i = 1; i < registrations.Length - 1; i++)
            {
                var registration = registrations[i];
                var nextRegistration = registrations[i + 1];

                var upgrader = new OneVersionUpgrader(registration.GetUpgradePlan(), registration.File);
                XmlFile upgradedXmlFile = upgrader.Upgrade();

                // execute the update and compare with reference version
                upgradedXmlFile.VerifyEqualTo(nextRegistration.File);
            }
        }

        public UpgradeResult UpgradeXml(string xmlToUpgradeFilePath)
        {
            XmlFile xmlToUpgrade = XmlFile.LoadXml(xmlToUpgradeFilePath);

            if (xmlToUpgrade.Version.Equals(extendedRegistrations.Max(x => x.Version)))
            {
                return new UpgradeResult
                {
                    UpgradeNeeded = false
                };
            }

            IEnumerable<Registration> upgradesToApply = extendedRegistrations
                .OrderBy(x => x.Version)
                .Where(x => x.Version > xmlToUpgrade.Version)
                .ToArray();

            foreach (var registration in upgradesToApply)
            {
                var upgrader = new OneVersionUpgrader(registration.GetUpgradePlan(), xmlToUpgrade);
                xmlToUpgrade = upgrader.Upgrade();
            }

            xmlToUpgrade.Document.Save(xmlToUpgradeFilePath);

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
            File = XmlFile.LoadXml(FilePath);
        }

        public UpgradePlan GetUpgradePlan()
        {
            return ((IUpgradePlanProvider) Activator.CreateInstance(type)).GetUpgradePlan();
        }

        public Version Version { get; }

        internal string FilePath { get; set; }

        public XmlFile File { get; set; }
    }
}