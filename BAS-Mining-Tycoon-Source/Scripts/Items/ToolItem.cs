namespace MiningTycoon
{
    public class ToolItem : Item
    {
        // How much faster does the ore deplete?
        public float damageMultiplier = 1.0f;

        // How much ore does a deposite spawn when hit by this?
        public float oreMultiplier = 1.0f;

        public override Item Copy()
        { return this.MemberwiseClone() as ToolItem; }
    }
}