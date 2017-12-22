namespace UConfig.Core
{
    using System.Xml.Linq;
    using System.Xml.Schema;

    internal class ConfigurationFile
    {
        public int Version
        {
            get;
            set;
        }

        public XElement Document
        {
            get;
            set;
        }

        public void VerifyEqualTo(ConfigurationFile updatedConfig)
        {
            XmlUtils.DeepEqualsWithNormalization(new XDocument(this.Document), new XDocument(updatedConfig.Document), null); //todo remove null
        }

        public static ConfigurationFile LoadXml(string xmlPath)
        {
            return LoadFromXmlElement(XElement.Load(xmlPath));
        }

        private static ConfigurationFile LoadFromXmlElement(XElement xml)
        {
            var result = new ConfigurationFile();
            result.Document = xml;
            result.Version = int.Parse(result.Document.Attribute("version").Value);
            return result;
        }

        public static ConfigurationFile FromXElement(XElement xml)
        {
            return LoadFromXmlElement(xml);
        }
    }

}