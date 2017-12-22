namespace UConfig.Core
{
    using System.Xml.Linq;
    using System.Xml.Schema;

    internal class ConfigurationFile
    {
        private string xmlPath;

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
            var result = new ConfigurationFile();
            result.xmlPath = xmlPath;
            result.Document = XElement.Load(xmlPath);
            result.Version = int.Parse(result.Document.Attribute("version").Value);
            return result;
        }
    }

}