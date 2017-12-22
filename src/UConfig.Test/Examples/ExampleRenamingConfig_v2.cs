namespace UConfig.Test.Examples
{
    using Core;

    internal class ExampleRenamingConfigV2 : IUpgradableConfig
    {
        public string TestString { get; set; }

        public int RenamedInteger { get; set; }


        public UpgradePlan GetUpgradePlan()
        {
            var map = new RenameMap()
                .RenameFrom(nameof(ExampleConfigV1.TestInteger)).To(nameof(RenamedInteger));
            return new UpgradePlan();
        }

        public int Version => 2;
    }
}