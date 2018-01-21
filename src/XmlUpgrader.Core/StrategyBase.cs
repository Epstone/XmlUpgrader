namespace XmlUpgrader.Core
{
    using System.Xml.Linq;

    internal abstract class StrategyBase
    {
        protected dynamic settingsTree;
        protected XElement workingTree;

        protected StrategyBase(XElement workingTree, dynamic settingsTree)
        {
            this.workingTree = workingTree;
            this.settingsTree = settingsTree;
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