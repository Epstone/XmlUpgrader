namespace UConfig.Core
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Threading;
    using System.Xml.Linq;

    public class Upgrader
    {
        private readonly XElement tree;

        public Upgrader(XElement tree)
        {
            this.tree = tree;
            mapping = new List<dynamic>();
        }

        private List<dynamic> mapping;


        public XElement Apply()
        {
            foreach (Type registration in Registrations)
            {
                IUpgradableConfig instance = (IUpgradableConfig)Activator.CreateInstance(registration);
                this.mapping.Add(instance.GetUpgradeMap());
            }

            foreach (var entry in mapping)
            {
                expandoToXML(entry, "");
            }

            return tree;
        }

        private XElement expandoToXML(dynamic node, String nodeName)
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


            foreach (var property in (IDictionary<String, Object>)node)
            {

                if (property.Value.GetType() == typeof(ExpandoObject))

                    xmlNode.Add(expandoToXML(property.Value, property.Key));

                else if (property.Value.GetType() == typeof(List<dynamic>))

                    foreach (var element in (List<dynamic>)property.Value)

                        xmlNode.Add(expandoToXML(element, property.Key));

                else

                    xmlNode.Add(new XElement(property.Key, property.Value));

            }

            return xmlNode;

        }

        public void AddEntry(dynamic xpath)
        {
            this.mapping.Add(xpath);
        }

        public void Register<T>() where T :  IUpgradableConfig, new()
        {
            this.Registrations.Add(typeof(T));
        }

        public List<Type> Registrations { get; set; } = new List<Type>();
    }
}