using MiningTycoon.Scripts.Core;
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
            impactRadius = data.impactRadius;
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
                GetComponent<ThunderRoad.Item>().Despawn();
                AudioSource.PlayClipAtPoint(sounds.sounds[1], transform.position);

                // Notify user.
                TycoonFloatyText.CreateFloatyText("<color=red>Pickaxe Snapped!</color>",
                                                 TycoonUtilities.GetFloatyTextPlayerAnchor(),
                                                 Player.local.head.transform,
                                                     3.0f);
                return;
            }
        }

        // Overrides CR's max limit for more impact on ore.
        protected override float GetMaxVelocity()
        { return 10.0f; }

        protected override bool ShouldDeformTarget(Collision collision, Collider target)
        {
            OreVein vein = collision.collider.GetComponentInParent<OreVein>();

            bool canBeMined = vein != null && vein.data.CanBeMinedBy(data.tier);
            if (vein != null && !canBeMined)
            {
                TycoonFloatyText.CreateFloatyText($"<color=red>Stronger Pickaxe required!</color>",
                                   TycoonUtilities.GetFloatyTextPlayerAnchor(),
                                 Player.local.head.transform,
                                     1.5f);
            }

            // Velocity has been bumped to 2.0 so the player really needs a hard swing.
            return base.ShouldDeformTarget(collision, target) && collision.relativeVelocity.magnitude >= 5.0f && canBeMined;
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
            AudioSource.PlayClipAtPoint(sounds.sounds[0], collisionData.contacts[0].point, 1.0f);
        }
    }
}