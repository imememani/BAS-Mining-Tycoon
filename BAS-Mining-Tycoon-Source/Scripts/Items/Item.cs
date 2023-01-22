using UnityEngine;

namespace MiningTycoon
{
    public class Item
    {
        // What item tier is this?
        public Tier tier = Tier.Copper;

        /// <summary>
        /// Reference and display name.
        /// </summary>
        public string id;

        // Optional spawn address.
        public string address;

        // How much health before breaking?
        public float health = 100.0f;

        // How much is this worth?
        public float value = 0.0f;
    }
}