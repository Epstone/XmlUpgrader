namespace XmlUpgrader.Core
{
    using System;
    using System.Xml.Linq;

    internal class XmlFile
    {
        public void VerifyEqualTo(XmlFile upgradedXmlFile)
        {
            XmlUtils.DeepEqualsWithNormalization(new XDocument(Document), new XDocument(upgradedXmlFile.Document), null); //todo remove null
        }

        public static XmlFile LoadXml(string xmlPath)
        {
            return LoadFromXElement(XElement.Load(xmlPath));
        }

        public static XmlFile FromXElement(XElement xml)
        {
            return LoadFromXElement(xml);
        }

        public Version Version { get; set; }

        public XElement Document { get; set; }

        private static XmlFile LoadFromXElement(XElement xml)
        {
            var result = new XmlFile();
            result.Document = xml;
            XAttribute versionAttribute = result.Document.Attribute("version");

            var defaultsToVersionOne = new Version(1, 0);
            result.Version = versionAttribute == null ? defaultsToVersionOne : new Version(versionAttribute.Value);
            return result;
        }
    }
}