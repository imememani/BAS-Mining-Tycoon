using UnityEngine;

namespace MiningTycoon
{
    public class Item
    {
        /// <summary>
        /// Reference and display name.
        /// </summary>
        public string id;

        // If empty will use ID.
        public string displayName;

        // What item tier is this?
        public Tier tier = Tier.Common;

        // Optional spawn address.
        public string address;

        // Shop icon address.
        public string iconAddress;

        // How much health before breaking?
        public float health = 100.0f;

        // How much is this worth?
        public double value = 0.0d;

        // Is this purchasable in the shop?
        public bool purchasable = false;

        // Which category does it reside in?
        public string shopCategory = "Items";

        // Optional shop description.
        public string shopDescription = "No description.";

        /// <summary>
        /// Obtain a shop description.
        /// </summary>
        public virtual string GetShopDescription() => shopDescription; 

        public virtual Item Copy() 
        { return MemberwiseClone() as Item; }
    }
}