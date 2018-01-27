namespace XmlUpgrader.Test.IntegrationTest
{
    using System;
    using System.IO;
    using Core;
    using Examples;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class UpgradeXmlTest
    {
        private static readonly string ConfigurationXmlDirectory = @"Examples\Xml\";
        private static readonly string VersionOnePath = $@"{ConfigurationXmlDirectory}Config_v1.xml";
        private static readonly string VersionTwoPath = $@"{ConfigurationXmlDirectory}Config_v2.xml";
        private Version version_1 = new Version(1, 0);
        private Version version_2 = new Version(2, 0);


        [Fact]
        public void DetailedRegistration()
        {
            var upgrader = new XmlFileUpgrader();

            upgrader.AddRegistration(version_1, @"Examples\Xml\Config_v1.xml");
            upgrader.AddRegistration(version_2, @"Examples\Xml\Config_v2.xml", typeof(ExampleConfigV2));

            upgrader.Verify();
        }

        [Fact]
        public void RespectNoUpgradeNeeded()
        {
            string versionOneXml = GetVersionOneCopy();
            var versionTwoXml = GetVersionTwoCopy();

            var upgrader = new XmlFileUpgrader();

            upgrader.AddRegistration(version_1, versionOneXml);
            upgrader.AddRegistration(version_2, versionTwoXml, typeof(ExampleConfigV2));

            UpgradeResult result =  upgrader.UpgradeXml(versionTwoXml);
            result.UpgradeNeeded.Should().BeFalse();
            
            VerifyEquivalent<ExampleConfigV2>(versionTwoXml, GetVersionTwoCopy());
        }


        [Fact]
        public void UpgradeXmlFileForTwoVersions()
        {
            string xmlToUpgradeFilePath = GetVersionOneCopy();
            var xmlUpgradeReferencePath = GetVersionTwoCopy();

var upgrader = new XmlFileUpgrader();

upgrader.AddRegistration(new Version(1, 0), xmlToUpgradeFilePath);
upgrader.AddRegistration(new Version(2, 0), xmlUpgradeReferencePath, typeof(ExampleConfigV2));

var result = upgrader.UpgradeXml(xmlToUpgradeFilePath);
result.UpgradeNeeded.Should().BeTrue();

            VerifyEquivalent<ExampleConfigV2>(xmlToUpgradeFilePath, xmlUpgradeReferencePath);

            result.UpgradedFromVersion.ShouldBeEquivalentTo(version_1);
            result.UpgradedToVersion.ShouldBeEquivalentTo(version_2);
        }

        private static string GetVersionTwoCopy()
        {
            return GetXmlCopy(VersionTwoPath);
        }

        private static string GetVersionOneCopy()
        {
            return GetXmlCopy(VersionOnePath);
        }

        private static string GetXmlCopy(string path)
        {
            var copyFileName = Path.GetFileNameWithoutExtension(path) + Guid.NewGuid() + Path.GetExtension(path);
            File.Copy(path, copyFileName);

            return copyFileName;
        }

        [Fact]
        public void UpgradeXmlFileWithoutVersionInfo()
        {
            string xmlToUpgrade = $@"{ConfigurationXmlDirectory}Config_v1_missing_version.xml";
            var xmlUpgradeReference = @"Examples\Xml\Config_v2.xml";

            var upgrader = new XmlFileUpgrader();

            upgrader.AddRegistration(version_1, xmlToUpgrade);
            upgrader.AddRegistration(version_2, xmlUpgradeReference, typeof(ExampleConfigV2));

            upgrader.UpgradeXml(xmlToUpgrade);

            VerifyEquivalent<ExampleConfigV2>(xmlToUpgrade, xmlUpgradeReference);
        }

        private static void VerifyEquivalent<T>(string xmlToUpgrade, string xmlUpgradeReference)
        {
            var updatedConfig = GetConfig<T>(xmlToUpgrade);
            var expected = GetConfig<T>(xmlUpgradeReference);

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