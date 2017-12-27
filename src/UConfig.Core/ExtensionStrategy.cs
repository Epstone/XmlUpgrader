namespace UConfig.Core
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Xml.Linq;

    internal class ExtensionStrategy : StrategyBase
    {
        public ExtensionStrategy(XElement workingTree, dynamic addedSettings) : base(workingTree, (object)addedSettings)
        {
        }

        public void Execute()
        {
            this.TraverseTree(settingsTree, string.Empty, workingTree);
        }

        internal XElement TraverseTree(dynamic renamingElement, string currentNodeName, XElement currentNode)
        {
            XElement xmlNode = GetOrCreateNode(currentNodeName, currentNode);

            foreach (var property in (IDictionary<string, object>)renamingElement)
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