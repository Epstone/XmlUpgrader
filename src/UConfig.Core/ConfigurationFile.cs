namespace UConfig.Core
{
    using System.Xml.Linq;

    internal class ConfigurationFile
    {
        public void VerifyEqualTo(ConfigurationFile updatedConfig)
        {
            XmlUtils.DeepEqualsWithNormalization(new XDocument(Document), new XDocument(updatedConfig.Document), null); //todo remove null
        }

        public static ConfigurationFile LoadXml(string xmlPath)
        {
            return LoadFromXmlElement(XElement.Load(xmlPath));
        }

        public static ConfigurationFile FromXElement(XElement xml)
        {
            return LoadFromXmlElement(xml);
        }

        public int Version { get; set; }

        public XElement Document { get; set; }

        private static ConfigurationFile LoadFromXmlElement(XElement xml)
        {
            var result = new ConfigurationFile();
            result.Document = xml;
            result.Version = int.Parse(result.Document.Attribute("version").Value);
            return result;
        }
    }
}