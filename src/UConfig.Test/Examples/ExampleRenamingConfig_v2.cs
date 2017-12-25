namespace UConfig.Test.Examples
{
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

        public int Version => 2;
    }
}