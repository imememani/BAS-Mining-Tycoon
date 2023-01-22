using UnityEngine;

namespace MiningTycoon
{
    public class Pickaxe : Deformer
    {
        public ToolItem data;

        public void LoadByID(string id)
        {
            data = (ToolItem)Entry.ItemDatabase[id];

            // Bump the velocity.
            velocityMultiplier = 0.0025f * data.damageMultiplier;

            Debug.Log($"PICKAXE: {data?.id}");
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
            return base.ShouldDeformTarget(collision, target) && vein != null && vein.data.CanMine(data.tier);
        }
    }
}
