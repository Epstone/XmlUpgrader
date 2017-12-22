namespace UConfig.Core
{
    using System.Xml.Linq;

    internal class ConfigurationFile
    {
        private string xmlPath;

        public ConfigurationFile()
        {
        }

        public ConfigurationFile(string xmlPath)
        {
            this.xmlPath = xmlPath;
            Document = XElement.Load(xmlPath);
            Version = int.Parse(Document.Attribute("version").Value);
        }

        public int Version { get; set; }

        public XElement Document { get; set; }

        public void VerifyEqualTo(ConfigurationFile updatedConfig)
        {
            throw new System.NotImplementedException();
        }
    }
}