namespace MiningTycoon
{
    public class CreatureItem : Item
    {
        public override string GetShopDescription() => $"A companion to aid you in your travels!";

        public override Item Copy()
        { return this.MemberwiseClone() as CreatureItem; }
    }
}