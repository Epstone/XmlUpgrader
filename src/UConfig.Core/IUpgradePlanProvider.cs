namespace UConfig.Core
{
    using System;

    public interface IUpgradePlanProvider
    {
        UpgradePlan GetUpgradePlan();
    }
}