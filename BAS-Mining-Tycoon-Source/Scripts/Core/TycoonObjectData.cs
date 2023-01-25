using System.Threading.Tasks;
using ThunderRoad;
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

        public string holsteredAt;
        public int heldAt = -1;

        public SVector position;
        public SVector rotation;
        public SVector velocity;

        /// <summary>
        /// Map this instance to an object.
        /// </summary>
        public void MapTo(TycoonWorldObject worldObject)
        {
            itemID = worldObject.data.id;

            heldAt = worldObject.thunderItem.handlers.Count > 0 ? (int)worldObject.thunderItem.handlers[0].side : -1;
            holsteredAt = worldObject.thunderItem.holder != null ? worldObject.thunderItem.holder.data.id : null;

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
            Tycoon.SpawnTycoonItem(itemID, position.ToVector3, Quaternion.Euler(rotation.ToVector3), async worldObject =>
            {
                worldObject.data.health = itemHealth;
                ThunderRoad.Item item = worldObject.GetComponent<ThunderRoad.Item>();

                // Await the item to load.
                while (!item.loaded)
                { await Task.Delay(5); }

                if (item != null)
                {
                    if (heldAt != -1)
                    {
                        Player.local.GetHand((Side)heldAt).ragdollHand.Grab(worldObject.GetComponentInChildren<Handle>());
                    }
                    else if (holsteredAt != null)
                    {
                        foreach (Holder holder in Player.local.creature.holders)
                        {
                            if (string.CompareOrdinal(holder.data.id, holsteredAt) == 0)
                            {
                                if (holder.items.Count > 0)
                                { holder.UnSnap(holder.items[0], true); }

                                holder.Snap(item, true, false);
                                break;
                            }
                        }
                    }
                }
            });
        }
    }
}
