using CarnageReborn;
using MiningTycoon.Scripts.Core;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    public class OreVein : DamageReciever, ITycoonDataHolder
    {
        public VeinItem data;
        private Deformable deformable;

        private float lastMineTime;
        private bool isDespawning = false;

        private void Awake()
        {
            deformable = gameObject.AddComponent<Deformable>();
            
            // Event hook.
            deformable.Deformed -= HandleDeformation;
            deformable.Deformed += HandleDeformation;

            // Configure.
            deformable.maxOffset = 100;
            deformable.CacheAllRendererMeshes();

            Tycoon.OreVeins.Add(this);
        }

        private void OnDestroy()
        {
            Tycoon.OreVeins.Remove(this);
        }

        public override void Break()
        {
            if (isDespawning)
            { return; }

            // Create floaty.
            TycoonFloatyText.CreateFloatyText($"<color=red>Vein Depleted!</color>",
                               TycoonUtilities.GetFloatyTextPlayerAnchor(),
                             Player.local.head.transform,
                                 1.5f);

            // Despawn.
            Despawn();
        }

        /// <summary>
        /// Despawn this ore, plays an animation first.
        /// </summary>
        public void Despawn()
        {
            isDespawning = true;
            StartCoroutine(Process());

            IEnumerator Process()
            {
                float currentY = transform.position.y;
                float targetY = currentY - 10;

                while (currentY != targetY)
                {
                    currentY = Mathf.MoveTowards(currentY, targetY, Time.deltaTime);
                    transform.position = new Vector3(transform.position.x, currentY, transform.position.z);

                    yield return null;
                }

                Destroy(gameObject);

                yield break;
            }
        }

        /// <summary>
        /// Handle a deformation event.
        /// </summary>
        private void HandleDeformation(Deformer deformer, Mesh mesh, Collision collisionData)
        {
            // Is the event dealt by a pickaxe?
            if (Time.time < lastMineTime || isDespawning)
            { return; }
            if (!(deformer is Pickaxe pickaxe))
            {
                TycoonFloatyText.CreateFloatyText($"<color=red>Pickaxe required!</color>",
                                               TycoonUtilities.GetFloatyTextPlayerAnchor(),
                                             Player.local.head.transform,
                                                 1.0f);
                return;
            }

            // Prevents mine spam.
            lastMineTime = Time.time + 0.25f;

            // Deduct health from pickaxe.
            pickaxe.TakeHealth(5);
            TakeHealth(ref data.health, 5.0f * pickaxe.data.damageMultiplier);

            // Refresh collision.
            MeshCollider mc = GetComponentInChildren<MeshCollider>();
            if (mc != null)
            {
                mc.sharedMesh = null;
                mc.sharedMesh = mesh;
            }

            // Spawn ore.
            if (data.ShouldDropOre(pickaxe.data.oreMultiplier))
            { Tycoon.SpawnTycoonItem(data.dropID, collisionData.contacts[0].point, Quaternion.identity); }
        }

        public void Load(string id)
        {
            // Create a reference copy.
            data = (VeinItem)(Tycoon.ItemDatabase[id]).Copy();
        }
    }
}