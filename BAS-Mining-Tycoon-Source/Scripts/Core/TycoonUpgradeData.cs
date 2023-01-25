using System;

namespace MiningTycoon
{
    public class TycoonUpgradeData
    {
        public string moduleName;
        public string moduleDisplay;

        public double currentValue;
        public double cost;
        public double incrementBy;

        public Action onPurchase;
    }
}