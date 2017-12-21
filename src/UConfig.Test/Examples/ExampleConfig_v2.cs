namespace UConfig.Test.Examples
{
    using System.Dynamic;
    using Core;

    class ExampleConfigV2 : IUpgradableConfig
    {
        public string TestString { get; set; }

        public int TestInteger { get; set; }

        public string AddedValue { get; set; }
        public dynamic GetUpgradeMap()
        {
            dynamic upgradeMap = new ExpandoObject();
            upgradeMap.AddedValue = "I'm an added value!";
            return upgradeMap;
        }
    }
}