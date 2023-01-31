namespace MiningTycoon
{
    public class CreatureItem : Item
    {
        public override Item Copy()
        { return this.MemberwiseClone() as CreatureItem; }
    }
}