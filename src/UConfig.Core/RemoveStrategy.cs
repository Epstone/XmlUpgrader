namespace UConfig.Core
{
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Xml.XPath;

    internal class RemoveStrategy
    {
        private readonly List<string> removedElements;
        private readonly XElement workingTree;

        public RemoveStrategy(XElement workingTree, List<string> removedElements)
        {
            this.workingTree = workingTree;
            this.removedElements = removedElements;
        }

        public void Execute()
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