namespace UConfig.Core
{
    using System.Xml.Linq;

    public abstract class StrategyBase
    {
        protected dynamic renamingTree;
        protected XElement workingTree;

        public StrategyBase(XElement workingTree, dynamic renamingTree)
        {
            this.workingTree = workingTree;
            this.renamingTree = renamingTree;
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