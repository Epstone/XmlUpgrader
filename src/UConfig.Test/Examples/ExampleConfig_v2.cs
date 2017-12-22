namespace UConfig.Test.Examples
{
    using System.Dynamic;
    using Core;

    internal class ExampleConfigV2 : IUpgradableConfig
    {
        public string TestString { get; set; }

        public int TestInteger { get; set; }

        public string AddedValue { get; set; }

        public UpgradePlan GetUpgradePlan()
        {
            dynamic addedSettings = new ExpandoObject();
            addedSettings.AddedValue = "I'm an added value!";
            var upgradePlan = new UpgradePlan();
            upgradePlan.AddedSettings(addedSettings);
            return upgradePlan;
        }

        public int Version => 2;
    }
}