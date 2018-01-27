namespace XmlUpgrader.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    public class XmlFileUpgrader
    {
        private readonly List<Registration> registrations = new List<Registration>();

        public void Verify()
        {
            if (registrations.Count == 1)
            {
                throw new InvalidOperationException("just one registration, nothing to upgrade or verify.");
            }

            Action<XmlFile> verifyAction = upgradedFile =>
            {
                Registration referenceRegistration = registrations.FirstOrDefault(registration => registration.Version == upgradedFile.Version);
                referenceRegistration.LoadFile();
                bool areEqual = upgradedFile.VerifyEqualTo(referenceRegistration.File);

                if (!areEqual)
                {
                    var errorMessageBuilder = new StringBuilder();
                    errorMessageBuilder.AppendLine("Xml upgrade script does not create the expected reference content.");
                    errorMessageBuilder.AppendLine($"Tried to upgrade to version {referenceRegistration.Version}");
                    errorMessageBuilder.AppendLine($"Upgrade result: {upgradedFile.Document}");
                    errorMessageBuilder.AppendLine($"Reference xml: {referenceRegistration.File.Document}");

                    throw new InvalidOperationException(errorMessageBuilder.ToString());
                }

            };

            RunUpgradesAndExecuteAction(registrations.OrderBy(x => x.Version).First().FilePath, verifyAction);
            // todo edge cases
        }

        public UpgradeResult UpgradeXml(string xmlToUpgradeFilePath)
        {
            return RunUpgradesAndExecuteAction(xmlToUpgradeFilePath, xmlFile => xmlFile.Document.Save(xmlToUpgradeFilePath));
        }

        private UpgradeResult RunUpgradesAndExecuteAction(string xmlToUpgradeFilePath, Action<XmlFile> documentOperation)
        {
            XmlFile xmlToUpgrade = XmlFile.LoadXml(xmlToUpgradeFilePath);
            if (xmlToUpgrade.Version.Equals(registrations.Max(x => x.Version)))
            {
                return new UpgradeResult
                {
                    UpgradeNeeded = false
                };
            }

            IEnumerable<Registration> upgradesToApply = registrations
                .OrderBy(x => x.Version)
                .Where(x => x.Version > xmlToUpgrade.Version)
                .ToArray();

            var initialVersion = xmlToUpgrade.Version;

            foreach (var registration in upgradesToApply)
            {
                var upgrader = new OneVersionUpgrader(registration.GetUpgradePlan(), xmlToUpgrade);
                xmlToUpgrade = upgrader.Upgrade();
            }

            if (documentOperation != null)
            {
                documentOperation(xmlToUpgrade); // save or verify
            }
            else
            {
                throw new InvalidOperationException();
            }

            return new UpgradeResult()
            {
                UpgradeNeeded = true,
                UpgradedFromVersion = initialVersion,
                UpgradedToVersion = xmlToUpgrade.Version
            };
        }

        public void AddRegistration(Version version, string filePath, Type type = null)
        {
            registrations.Add(new Registration(filePath, version, type));
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
            UpgradePlan upgradePlan = ((IUpgradePlanProvider)Activator.CreateInstance(type)).GetUpgradePlan();
            upgradePlan.SetVersion(this.Version);
            return upgradePlan;
        }

        public Version Version { get; }

        internal string FilePath { get; set; }

        public XmlFile File { get; set; }
    }
}