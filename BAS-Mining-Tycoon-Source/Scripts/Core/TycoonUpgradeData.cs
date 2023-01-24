using System;

namespace MiningTycoon
{
    public class TycoonUpgradeData
    {
        public string moduleName;
        public string moduleDisplay;

        public float currentValue;
        public float cost;
        public float incrementBy;

        public Action onPurchase;
    }
}