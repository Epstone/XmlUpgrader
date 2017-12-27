using System.Xml.Linq;

namespace UConfig.Core
{
    internal class ExtensionStrategy
    {
        private XElement workingTree;
        private dynamic addedSettings;

        public ExtensionStrategy(XElement workingTree, dynamic addedSettings)
        {
            this.workingTree = workingTree;
            this.addedSettings = addedSettings;
        }
    }
}