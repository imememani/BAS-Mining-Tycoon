using CarnageReborn;
using UnityEngine;

namespace MiningTycoon
{
    public class OreVein : DamageReciever, ITycoonObject
    {
        public VeinItem data;
        private Deformable deformable;

        private float lastMineTime;

        private void Awake()
        {
            // DEBUG
            data = (VeinItem)Entry.ItemDatabase["Copper"];

            deformable = GetComponent<Deformable>();
            if (deformable == null) deformable = gameObject.AddComponent<Deformable>();

            // Event hook.
            deformable.OnDeform -= HandleDeformation;
            deformable.OnDeform += HandleDeformation;

            deformable.CacheAllRendererMeshes();
        }

        public override void Break()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Handle a deformation event.
        /// </summary>
        private void HandleDeformation(Deformer deformer, Mesh mesh, Collision collisionData)
        {
            // Is the event dealt by a pickaxe?
            if (Time.time < lastMineTime || !(deformer is Pickaxe pickaxe))
            { return; }

            // Prevents mine spam.
            lastMineTime = Time.time + 0.1f;

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
            Entry.SpawnItem(data.dropID, collisionData.contacts[0].point + Vector3.up, Quaternion.identity);
        }

        public void Load(string id)
        {
            // Create a reference copy.
            data = (VeinItem)(Entry.ItemDatabase[id]).Copy();
        }
    }
}