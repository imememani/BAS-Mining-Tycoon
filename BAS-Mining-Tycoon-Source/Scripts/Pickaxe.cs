using UnityEngine;

namespace MiningTycoon
{
    public class Pickaxe : Deformer, ITycoonObject
    {
        public ToolItem data;

        public void Load(string id)
        {
            // Create a reference copy.
            data = (ToolItem)(Entry.ItemDatabase[id]).Copy();

            // Bump the velocity.
            velocityMultiplier = 0.02f * data.damageMultiplier;
        }

        /// <summary>
        /// Load data in to this object.
        /// </summary>
        public void Load(ToolItem data)
        {
            this.data = data;
        }

        /// <summary>
        /// Subtract health.
        /// </summary>
        public void TakeHealth(float amount)
        {
            data.health -= amount;

            // Break?
            if (data.health <= 0)
            {
                // Break item.
                return;
            }
        }

        protected override bool ShouldDeformTarget(Collision collision, Collider target)
        {
            OreVein vein = collision.collider.GetComponentInParent<OreVein>();
            return base.ShouldDeformTarget(collision, target) && vein != null && vein.data.CanBeMinedBy(data.tier);
        }
    }
}