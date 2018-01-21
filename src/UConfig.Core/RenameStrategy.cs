namespace UConfig.Core
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Xml.Linq;
    using System.Xml.XPath;

    internal class RenameStrategy : StrategyBase
    {
        internal RenameStrategy(XElement workingTree, dynamic renamingTree) : base(workingTree, (object) renamingTree)
        {
        }

        internal void Execute()
        {
            this.TraverseTree(settingsTree, string.Empty, workingTree);
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
    }
}