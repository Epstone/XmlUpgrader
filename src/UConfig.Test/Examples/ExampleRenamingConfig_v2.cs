namespace UConfig.Test.Examples
{
    using System;
    using Core;

    internal class ExampleRenamingConfigV2 : IUpgradableConfig
    {
        public string TestString { get; set; }

        public int RenamedInteger { get; set; }


        public UpgradePlan GetUpgradePlan()
        {
            return new UpgradePlan();

            // todo use in test
        }

        public Version Version => new Version(2,0);
    }
}