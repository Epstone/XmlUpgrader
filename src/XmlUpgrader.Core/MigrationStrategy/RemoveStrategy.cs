namespace XmlUpgrader.Core.MigrationStrategy
{
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Xml.XPath;

    internal class RemoveStrategy
    {
        private readonly List<string> removedElements;
        private readonly XElement workingTree;

        internal RemoveStrategy(XElement workingTree, List<string> removedElements)
        {
            this.workingTree = workingTree;
            this.removedElements = removedElements;
        }

        internal void Execute()
        {
            removedElements.ForEach(x =>
                {
                    XElement selectedElement = workingTree.XPathSelectElement(x);
                    if (selectedElement != null)
                    {
                        selectedElement.Remove();
                    }
                }
            );
        }
    }
}