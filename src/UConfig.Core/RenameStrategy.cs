namespace UConfig.Core
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Xml.Linq;
    using System.Xml.XPath;

    internal class RenameStrategy
    {
        private readonly XElement workingTree;
        private XElement currentNode;

        public RenameStrategy(XElement workingTree)
        {
            this.workingTree = workingTree;
            this.currentNode = workingTree;
        }

        internal XElement FindXmlNode(dynamic nodeToFind, string currentNodeName)
        {
            XElement xmlNode;
            if (string.IsNullOrEmpty(currentNodeName))
            {
                xmlNode = this.workingTree;
            }
            else
            {
                xmlNode = currentNode.Element(currentNodeName); // try to grab existing node
            }

            if (xmlNode == null)
            {
                xmlNode = new XElement(currentNodeName); // node not yet existing, create new
                currentNode.Add(xmlNode);
            }

            if (xmlNode == null)
            {
                throw new InvalidOperationException($"cannot find node with name {nodeToFind}");
            }

            foreach (KeyValuePair<string, object> property in (IDictionary<string, object>)nodeToFind)
            {
                if (property.Value is string valueString)
                {
                    XElement oldNode = this.workingTree.XPathSelectElement(valueString);
                    // find old node and move and rename
                    oldNode.Remove();
                    oldNode.Name = property.Key;
                    xmlNode.Add(oldNode);
                }
                else if (property.Value is ExpandoObject)
                {
                    FindXmlNode(property.Value, property.Key);
                }
            }

            return xmlNode;
        }
    }
}