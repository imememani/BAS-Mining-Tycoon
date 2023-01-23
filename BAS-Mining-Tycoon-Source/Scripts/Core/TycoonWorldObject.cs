using UnityEngine;

namespace MiningTycoon
{
    public class TycoonWorldObject : MonoBehaviour, ITycoonDataHolder
    {
        public Item data;

        private void Awake()
        {
            Entry.WorldObjects.Add(this);
        }

        private void OnDestroy()
        {
            Entry.WorldObjects.Remove(this);
        }

        public void Load(string id)
        {
            // Create a reference copy.
            data = (Entry.ItemDatabase[id]).Copy();
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