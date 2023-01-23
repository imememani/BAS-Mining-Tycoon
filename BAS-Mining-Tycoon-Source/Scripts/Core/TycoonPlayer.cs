using System;
using ThunderRoad;

namespace MiningTycoon
{
    /// <summary>
    /// Player tycoon save data.
    /// </summary>
    [System.Serializable]
    public class TycoonPlayer
    {
        // Aka doubloons.
        public float currency = 100.0f;

        // How many ores have been processed.
        public long oresCollected = 0;

        // The exact date this player was created.
        public DateTime profileCreationDate;

        // Players last position.
        public SVector position;

        // Players last velocity.
        public SVector velocity;

        // All world objects.
        public TycoonObjectData[] worldObjects = System.Array.Empty<TycoonObjectData>();

        /// <summary>
        /// Update this instances data, like the position and velocity.
        /// </summary>
        public void Refresh()
        {
            // Map the players position and rotation.
            position = new SVector().From(Player.local.transform.position);
            velocity = new SVector().From(Player.local.locomotion.rb.velocity);

            // Under the map?
            if (position.y <= -10.0f)
            {
                // Plop 'em back on the map.
                position.y = 5.0f;
            }

            // Map all objects.
            worldObjects = new TycoonObjectData[Entry.WorldObjects.Count];
            for (int i = 0; i < Entry.WorldObjects.Count; i++)
            {
                worldObjects[i] = Entry.WorldObjects[i].Serialize();
            }
        }
    }
}