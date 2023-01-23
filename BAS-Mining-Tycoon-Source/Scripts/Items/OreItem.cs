namespace MiningTycoon
{
    public class OreItem : Item
    {
        public override Item Copy()
        { return this.MemberwiseClone() as OreItem; }
    }
}