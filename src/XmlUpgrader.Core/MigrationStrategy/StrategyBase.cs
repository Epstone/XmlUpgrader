namespace XmlUpgrader.Core.MigrationStrategy
{
    using System.Xml.Linq;

    internal abstract class StrategyBase
    {
        protected dynamic ElementsToAddTree;
        protected XElement WorkingTree;

        protected StrategyBase(XElement workingTree, dynamic elementsToAddTree)
        {
            this.WorkingTree = workingTree;
            this.ElementsToAddTree = elementsToAddTree;
        }

        protected XElement GetOrCreateNode(string currentNodeName, XElement currentNode)
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