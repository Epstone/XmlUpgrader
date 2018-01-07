namespace UConfig.Core
{
    using System;

    public interface IUpgradableConfig
    {
        UpgradePlan GetUpgradePlan();
    }
}