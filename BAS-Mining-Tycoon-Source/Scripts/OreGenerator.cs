using UnityEngine;

namespace MiningTycoon
{
    /// <summary>
    /// Handles ore generation in the map.
    /// </summary>
    public static class OreGenerator
    {
        /// <summary>
        /// Generates a new batch of ore in all zones.
        /// Will clean any existing ore veins.
        /// </summary>
        public static void GenerateOreVeins()
        {
            foreach (var reference in Entry.References)
            {
                // Is this a zone?
                if (reference == null || !reference.name.Contains("Zones"))
                { continue; }

                foreach (Transform zone in reference.transforms)
                {
                    if (zone == null)
                    { continue; }

                    // Destroy any existing ore.
                    Transform[] children = zone.GetComponentsInChildren<Transform>();
                    for (int i = 1; i < children.Length; i++)
                    {
                        Object.Destroy(children[i].gameObject);
                    }

                    // Parse the tier.
                    Tier tier = (Tier)System.Enum.Parse(typeof(Tier), reference.name.Replace("Zones", string.Empty));

                    // Generate a new batch for this zone.
                    zone.GenerateOreInZone(tier);
                }
            }
        }

        /// <summary>
        /// Generate ore in a zone.
        /// </summary>
        public static void GenerateOreInZone(this Transform zoneObject, Tier tier)
        {
            // The maximum bounds to spawn.
            Bounds bounds = new Bounds(zoneObject.position, zoneObject.localScale);

            // Obtain a copy of the vein data.
            VeinItem vein = (VeinItem)Entry.ItemDatabase[tier.ToString()];

            // Calculate a random amount.
            Vector2 map = tier.GetPopulationMap();
            int population = (int)Random.Range(map.x, map.y);

            // Spawn the ores.
            Vector3 origin = zoneObject.position;
            for (int i = 0; i < population; i++)
            {
                // Position within the bounds.
                origin.x += Random.Range(-bounds.extents.x, bounds.extents.x);
                origin.z += Random.Range(-bounds.extents.z, bounds.extents.z);
                origin.y = 100.0f;

                // Spawn a vein.
                Entry.SpawnItem(vein.id, origin, Quaternion.identity, go =>
                {
                    // Align to the surface.
                    if (Physics.Raycast(go.transform.position, Vector3.down, out RaycastHit hit, 200, ~0, QueryTriggerInteraction.Ignore))
                    {
                        go.transform.SetPositionAndRotation(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal * 0.5f));
                    }

                    // Load the vein up.
                    go.AddComponent<OreVein>().Load(tier.ToString());
                    go.transform.SetParent(zoneObject);
                });
            }
        }

        /// <summary>
        /// Get a tiers population map.
        /// </summary>
        public static Vector2 GetPopulationMap(this Tier tier)
        {
            switch (tier)
            {
                case Tier.Copper: return new Vector2(3, 15);
                case Tier.Iron: return new Vector2(2, 13);
                case Tier.Ruby: return new Vector2(1, 10);
                case Tier.Gold: return new Vector2(0, 5);
            }

            return Vector2.one;
        }
    }
}