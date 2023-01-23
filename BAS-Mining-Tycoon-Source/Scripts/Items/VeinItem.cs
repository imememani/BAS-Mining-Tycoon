namespace MiningTycoon
{
    public class VeinItem : Item
    {
        // Ore id to spawn.
        public string dropID;

        // How often between 0-100 does ore drop?
        public float oreDropChance = 100.0f;

        /// <summary>
        /// Should this deposit drop ore this frame?
        /// </summary>
        public bool ShouldDropOre(float dropMultiplier = 1.0f)
        { return UnityEngine.Random.Range(0, 100) <= (oreDropChance * dropMultiplier); }

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