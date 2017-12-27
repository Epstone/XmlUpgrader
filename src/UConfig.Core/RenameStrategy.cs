namespace UConfig.Core
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Xml.Linq;
    using System.Xml.XPath;

    internal class RenameStrategy
    {
        private readonly dynamic renamingTree;
        private readonly XElement workingTree;

        public RenameStrategy(XElement workingTree, dynamic renamingTree)
        {
            this.workingTree = workingTree;
            this.renamingTree = renamingTree;
        }

        public void Execute()
        {
            this.TraverseTree(renamingTree, string.Empty, workingTree);
        }

        internal XElement TraverseTree(dynamic renamingElement, string currentNodeName, XElement currentNode)
        {
            XElement xmlNode = GetOrCreateNode(currentNodeName, currentNode);

            foreach (var property in (IDictionary<string, object>) renamingElement)
            {
                if (property.Value is string xPath)
                {
                    MoveNodeByXpath(xPath, property.Key, xmlNode);
                }
                else if (property.Value is ExpandoObject)
                {
                    TraverseTree(property.Value, property.Key, xmlNode);
                }
            }

            return xmlNode;
        }

        private void MoveNodeByXpath(string xPath, string newName, XElement moveTarget)
        {
            XElement oldNode = workingTree.XPathSelectElement(xPath);
            oldNode.Remove();
            oldNode.Name = newName;
            moveTarget.Add(oldNode);
        }

        private XElement GetOrCreateNode(string currentNodeName, XElement currentNode)
        {
            XElement xmlNode = currentNode;
            if (!string.IsNullOrEmpty(currentNodeName))
            {
                xmlNode = currentNode.Element(currentNodeName); 
            }

            if (xmlNode == null)
            {
                xmlNode = new XElement(currentNodeName); 
                currentNode.Add(xmlNode);
            }
            return xmlNode;
        }
    }
}