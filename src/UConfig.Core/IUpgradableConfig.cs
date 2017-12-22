namespace UConfig.Core
{
    public interface IUpgradableConfig
    {
        UpgradePlan GetUpgradePlan();

        int Version { get; }
    }
}