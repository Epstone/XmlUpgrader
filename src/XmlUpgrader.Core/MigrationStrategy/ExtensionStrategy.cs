namespace XmlUpgrader.Core.MigrationStrategy
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Xml.Linq;

    internal class ExtensionStrategy : StrategyBase
    {
        internal ExtensionStrategy(XElement workingTree, dynamic addedElements) : base(workingTree, (object) addedElements)
        {
        }

        internal void Execute()
        {
            this.TraverseTree(ElementsToAddTree, string.Empty, WorkingTree);
        }

        internal XElement TraverseTree(dynamic renamingElement, string currentNodeName, XElement currentNode)
        {
            XElement xmlNode = GetOrCreateNode(currentNodeName, currentNode);

            foreach (var property in (IDictionary<string, object>) renamingElement)
            {
                if (property.Value is ExpandoObject)
                {
                    TraverseTree(property.Value, property.Key, xmlNode);
                }
                else
                {
                    xmlNode.Add(new XElement(property.Key, property.Value));
                }
            }

            return xmlNode;
        }
    }
}