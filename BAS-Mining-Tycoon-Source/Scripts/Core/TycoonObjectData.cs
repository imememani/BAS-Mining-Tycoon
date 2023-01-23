using UnityEngine;

namespace MiningTycoon
{
    /// <summary>
    /// This class retains information about a tycoon object in the world.
    /// </summary>
    [System.Serializable]
    public class TycoonObjectData
    {
        public string itemID;
        public float itemHealth;

        public SVector position;
        public SVector rotation;
        public SVector velocity;

        /// <summary>
        /// Map this instance to an object.
        /// </summary>
        public void MapTo(TycoonWorldObject worldObject)
        {
            itemID = worldObject.data.id;

            Rigidbody rb = worldObject.GetComponent<Rigidbody>();
            if (rb != null)
            { velocity = new SVector().From(rb.velocity); }

            position = new SVector().From(worldObject.transform.position);
            rotation = new SVector().From(worldObject.transform.localEulerAngles);
        }

        /// <summary>
        /// Load this object in to the world.
        /// </summary>
        public void Load()
        {
            Entry.SpawnTycoonItem(itemID, position.ToVector3, Quaternion.Euler(rotation.ToVector3), worldObject =>
            {
                worldObject.data.health = itemHealth;
            });
        }
    }
}
