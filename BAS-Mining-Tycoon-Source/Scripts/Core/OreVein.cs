using CarnageReborn;
using UnityEngine;

namespace MiningTycoon
{
    public class OreVein : DamageReciever, ITycoonDataHolder
    {
        public VeinItem data;
        private Deformable deformable;

        private float lastMineTime;

        private void Awake()
        {
            deformable = GetComponent<Deformable>();
            if (deformable == null) deformable = gameObject.AddComponent<Deformable>();

            // Event hook.
            deformable.Deformed -= HandleDeformation;
            deformable.Deformed += HandleDeformation;

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
            // TODO: Have a fancy smash animation play or something.
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