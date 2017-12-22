namespace UConfig.Core
{
    public class UpgradePlan
    {
        public UpgradePlan(dynamic valuesToAdd)
        {
            AddedValues = valuesToAdd;
        }

        public dynamic AddedValues { get; set; }
        public int UpgradeToVersion { get; set; }
    }
}