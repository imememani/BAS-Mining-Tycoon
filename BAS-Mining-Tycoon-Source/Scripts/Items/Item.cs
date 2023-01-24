using UnityEngine;

namespace MiningTycoon
{
    public class Item
    {
        /// <summary>
        /// Reference and display name.
        /// </summary>
        public string id;

        // What item tier is this?
        public int tier = 0;

        // Optional spawn address.
        public string address;

        // Shop icon address.
        public string iconAddress;

        // How much health before breaking?
        public float health = 100.0f;

        // How much is this worth?
        public float value = 0.0f;

        /// <summary>
        /// Obtain a shop description.
        /// </summary>
        public virtual string GetShopDescription() => "No description."; 

        public virtual Item Copy() 
        { return MemberwiseClone() as Item; }
    }
}