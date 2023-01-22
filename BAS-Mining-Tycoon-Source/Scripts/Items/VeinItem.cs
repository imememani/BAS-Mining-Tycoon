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
            int a = (int)otherTier + 1; // + 1 because we want the current tier to be able to mine the next tier too.
            int b = (int)tier;

            return b <= a;
        }

        public override Item Copy()
        { return this.MemberwiseClone() as VeinItem; }
    }
}