namespace MiningTycoon
{
    public class VeinItem : Item
    {
        // Ore id to spawn.
        public string dropID;

        // How often between 0-100 does ore drop?
        public float oreDropChance = 100.0f;

        // Spawn rates.
        public int minSpawn = 1, 
                   maxSpawn = 1;

        /// <summary>
        /// Should this deposit drop ore this frame?
        /// </summary>
        public bool ShouldDropOre(float dropMultiplier = 1.0f)
        { return UnityEngine.Random.Range(0, 100) <= (oreDropChance * dropMultiplier); }

        /// <summary>
        /// Can this item mine the target tier?
        /// </summary>
        public bool CanBeMinedBy(int otherTier)
        {
            int a = otherTier + 1; // + 1 because we want the current tier to be able to mine the next tier too.
            int b = tier;

            return b <= a;
        }

        public override Item Copy()
        { return this.MemberwiseClone() as VeinItem; }
    }
}