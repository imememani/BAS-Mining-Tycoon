namespace MiningTycoon
{
    public class VeinItem : Item
    {
        public string dropID;

        /// <summary>
        /// Can this item mine the target tier?
        /// </summary>
        public bool CanMine(Tier otherTier)
        {
            int a = (int)otherTier;
            int b = (int)tier;

            return a <= b;
        }

        public override Item Copy()
        { return this.MemberwiseClone() as VeinItem; }
    }
}
