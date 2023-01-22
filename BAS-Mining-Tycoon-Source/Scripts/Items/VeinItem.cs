namespace MiningTycoon
{
    public class VeinItem : Item
    {
        public string dropID;

        /// <summary>
        /// Can this item mine the target tier?
        /// </summary>
        public bool CanBeMinedBy(Tier otherTier)
        {
            int a = (int)otherTier;
            int b = (int)tier;

            return b <= a;
        }

        public override Item Copy()
        { return this.MemberwiseClone() as VeinItem; }
    }
}