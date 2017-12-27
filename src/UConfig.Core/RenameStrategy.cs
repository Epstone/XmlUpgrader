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
        private readonly dynamic upgradeSettings;

        public RenameStrategy(XElement workingTree, dynamic upgradeSettings)
        {
            this.workingTree = workingTree;
            this.upgradeSettings = upgradeSettings;
        }

        public void Execute()
        {
            this.MoveNode(this.upgradeSettings, string.Empty, workingTree);
        }

        internal XElement MoveNode(dynamic nodeToFind, string currentNodeName, XElement currentNode)
        {
            XElement xmlNode = GetOrCreateNode(currentNodeName, currentNode);

            foreach (KeyValuePair<string, object> property in (IDictionary<string, object>)nodeToFind)
            {
                if (property.Value is string xPath)
                {
                    MoveNodeByXpath(xPath, newName: property.Key, moveTarget: xmlNode);
                }
                else if (property.Value is ExpandoObject)
                {
                    MoveNode(property.Value, property.Key, xmlNode);
                }
            }

            return xmlNode;
        }

        private void MoveNodeByXpath(string xPath, string newName, XElement moveTarget)
        {
            XElement oldNode = this.workingTree.XPathSelectElement(xPath);
            oldNode.Remove();
            oldNode.Name = newName;
            moveTarget.Add(oldNode);
        }

        private XElement GetOrCreateNode(string currentNodeName, XElement currentNode)
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
            return xmlNode;
        }
    }
}