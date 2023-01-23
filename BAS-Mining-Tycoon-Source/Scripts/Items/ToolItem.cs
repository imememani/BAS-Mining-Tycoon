namespace MiningTycoon
{
    public class ToolItem : Item
    {
        // How much faster does the ore deplete?
        public float damageMultiplier = 1.0f;

        // A multiplier on the vein's ore spawn chance.
        // e.g:
        // 75 * 1.1 = 82.5
        // Instead of a 75% chance there's now an 82.5% chance.
        public float oreMultiplier = 1.0f;

        public override string GetShopDescription() => $"+ {health} health\n+ x{damageMultiplier} damage\n+ x{oreMultiplier} ore";

        public override Item Copy()
        { return this.MemberwiseClone() as ToolItem; }
    }
}