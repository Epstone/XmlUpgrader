namespace UConfig.Core
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    public class Upgrader
    {
        private XElement tree;

        private readonly List<dynamic> mapping;

        public Upgrader(XElement tree)
        {
            this.tree = tree;
            mapping = new List<dynamic>();
        }

        public Upgrader()
        {
        }


        public XElement Apply()
        {
            foreach (Type registration in Registrations)
            {
                IUpgradableConfig instance = (IUpgradableConfig)Activator.CreateInstance(registration);
                mapping.Add(instance.GetUpgradeMap());
            }

            foreach (var entry in mapping)
            {
                expandoToXML(entry, "");
            }

            return tree;
        }

        internal ConfigurationFile ApplyForOne(UpgradeMap upgradeMap)
        {
            var currentTree = new XElement(tree);
            this.tree = currentTree;
            foreach (var entry in mapping)
            {
                expandoToXML(entry, "");
            }

            return new ConfigurationFile()
            {
                Version = upgradeMap.UpgradeToVersion,
                Document = currentTree
            };
        }

        public void AddEntry(dynamic xpath)
        {
            mapping.Add(xpath);
        }

        public void Register<T>() where T : IUpgradableConfig, new()
        {
            Registrations.Add(typeof(T));
        }

        public void RegisterAll(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(t => typeof(IUpgradableConfig).IsAssignableFrom(t));
            Registrations.AddRange(types);
        }

        public void AddXmlConfigurationDir(string directory)
        {
            XmlConfigurationDirectory = directory;
        }

        public void Verify()
        {
            // create dictionary of Xml configuration files (version -> xml)
            var xmlFiles = Directory.GetFiles(XmlConfigurationDirectory, "*.xml");


            if (xmlFiles.Length == 0)
            {
                throw new InvalidOperationException("no xml configuration files found");
            }
            IEnumerable<ConfigurationFile> configFiles = LoadAsConfigFiles(xmlFiles);

            if (xmlFiles.Length == 1)
            {
                throw new InvalidOperationException("just one configuration file found, nothing to upgrade.");
            }


            // verify, that we have an upgrade script to the next version
            var configurationFiles = configFiles.OrderBy(x => x.Version).ToArray();
            for (int i = 0; i < configurationFiles.Length - 1; i++) // do not upgrade to a version which is not existing yet
            {
                var configurationFile = configurationFiles[i];
                // todo no version gaps allowed
                dynamic mapping = this.mapping.FirstOrDefault(m => m.Version == configurationFile.Version + 1);
                if (mapping == null)
                {
                    throw new InvalidOperationException($"No upgrade script for xml version {configurationFile.Version} found.");
                }

                this.tree = configurationFile.Document; // todo do not touch original content
                ConfigurationFile updatedConfig = ApplyForOne(new UpgradeMap() { AddedValues = mapping });

                // execute the update and compare with reference version
                configurationFiles[i + 1].VerifyEqualTo(updatedConfig);
            }
            
        }

        public List<Type> Registrations { get; set; } = new List<Type>();

        public string XmlConfigurationDirectory { get; set; }

        private XElement expandoToXML(dynamic node, string nodeName)
        {
            XElement xmlNode;
            if (string.IsNullOrEmpty(nodeName))
            {
                xmlNode = tree;
            }
            else
            {
                xmlNode = new XElement(nodeName);
            }


            foreach (var property in (IDictionary<string, object>)node)
            {
                if (property.Value.GetType() == typeof(ExpandoObject))

                {
                    xmlNode.Add(expandoToXML(property.Value, property.Key));
                }

                else if (property.Value.GetType() == typeof(List<dynamic>))

                {
                    foreach (var element in (List<dynamic>)property.Value)
                    {
                        xmlNode.Add(expandoToXML(element, property.Key));
                    }
                }

                else
                {
                    xmlNode.Add(new XElement(property.Key, property.Value));
                }
            }

            return xmlNode;
        }

        private IEnumerable<ConfigurationFile> LoadAsConfigFiles(string[] xmlFiles)
        {
            return xmlFiles.Select(x => new ConfigurationFile(x));
        }
    }

    public class UpgradeMap
    {
        public dynamic AddedValues { get; set; }
        public int UpgradeToVersion { get; set; }
    }
}