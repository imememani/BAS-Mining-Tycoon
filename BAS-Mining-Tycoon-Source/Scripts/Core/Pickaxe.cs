using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    public class Pickaxe : Deformer, ITycoonDataHolder
    {
        public ToolItem data;
        private AudioContainer sounds;

        private void Awake()
        {
            Deform += HandleDeform;
        }

        public void Load(string id)
        {
            // Create a reference copy.
            data = (ToolItem)(Tycoon.ItemDatabase[id]).Copy();

            // Load sounds.
            Catalog.LoadAssetAsync<AudioContainer>("Tycoon.Audio.PickaxeSounds", container => sounds = container, "Pickaxe->Load");

            // Bump the velocity.
            velocityMultiplier = 0.01f * data.damageMultiplier;
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

        // Overrides CR's max limit for more impact on ore.
        protected override float GetMaxVelocity()
        { return 10.0f; }

        protected override bool ShouldDeformTarget(Collision collision, Collider target)
        {
            OreVein vein = collision.collider.GetComponentInParent<OreVein>();

            // Velocity has been bumped to 2.0 so the player really needs a hard swing.
            return base.ShouldDeformTarget(collision, target) && collision.relativeVelocity.magnitude >= 5.0f && vein != null && vein.data.CanBeMinedBy(data.tier);
        }

        /// <summary>
        /// Invoked when this deformer has deformed.
        /// </summary>
        private void HandleDeform(Deformer deformer, CarnageReborn.Deformable deformable, Collision collisionData)
        {
            // Has this deformed ore?
            if (deformable.GetComponent<OreVein>() == null)
            { return; }

            // Yes, play a hit sound.
            AudioSource.PlayClipAtPoint(sounds.PickAudioClip(), collisionData.contacts[0].point, 1.0f);
        }
    }
}