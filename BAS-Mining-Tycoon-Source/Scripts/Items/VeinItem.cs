namespace MiningTycoon
{
    public class VeinItem : Item
    {
        public string dropID;

        /// <summary>
        /// Can this item mine the target tier?
        /// </summary>
        public bool CanMine(Tier tier)
        {
            return (int)this.tier >= (int)tier;
        }
    }
}
