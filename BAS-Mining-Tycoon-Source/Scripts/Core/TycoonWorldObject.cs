using UnityEngine;

namespace MiningTycoon
{
    public class TycoonWorldObject : MonoBehaviour, ITycoonDataHolder
    {
        public Item data;
        public ThunderRoad.Item thunderItem;

        private void Awake()
        {
            thunderItem = GetComponent<ThunderRoad.Item>();
            Tycoon.WorldObjects.Add(this);
        }

        private void OnDestroy()
        {
            Tycoon.WorldObjects.Remove(this);
        }

        public void Load(string id)
        {
            // Create a reference copy.
            data = (Tycoon.ItemDatabase[id]).Copy();
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