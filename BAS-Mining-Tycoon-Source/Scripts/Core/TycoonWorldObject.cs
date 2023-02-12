using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    public class TycoonWorldObject : MonoBehaviour, ITycoonDataHolder
    {
        public Item data;
        public CreatureData creatureData;
        public ThunderRoad.Item thunderItem;

        private void Awake()
        {
            thunderItem = GetComponent<ThunderRoad.Item>();
            if (GetComponent<Creature>() is Creature creature) creature.OnKillEvent += HandleCreatureDeath;

            Tycoon.WorldObjects.Add(this);
        }

        private void HandleCreatureDeath(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime != EventTime.OnEnd)
            { return; }

            Tycoon.WorldObjects.Remove(this);
        }

        private void OnDestroy() => Tycoon.WorldObjects.Remove(this);

        private void OnDisable() => Tycoon.WorldObjects.Remove(this);

        public void Despawn()
        {
            if (thunderItem != null)
            {
                thunderItem.Despawn();
                Tycoon.WorldObjects.Remove(this);
            }
            else if (creatureData != null)
            {
                GetComponent<Creature>().Despawn();
                Tycoon.WorldObjects.Remove(this);
            }
            else
            { Destroy(gameObject); }
        }

        public void Load(string id)
        {
            // Create a reference copy.
            data = (Tycoon.ItemDatabase[id]).Copy();

            // Inject false data?
            if (data is OreItem)
            {
                thunderItem.InjectItemData(id);
            }
        }

        public void LoadCreature(CreatureData data)
        {
            this.creatureData = data;
        }

        /// <summary>
        /// Serialize this data.
        /// </summary>
        public TycoonObjectData Serialize()
        {
            TycoonObjectData objectData = new TycoonObjectData();
            objectData.MapTo(this);
            return objectData;
        }
    }
}