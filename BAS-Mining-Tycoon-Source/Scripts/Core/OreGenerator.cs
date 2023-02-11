using BASLogger;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    /// <summary>
    /// Handles ore generation in the map.
    /// </summary>
    public static class OreGenerator
    {
        // Keep track of generation.
        private static bool hasGenerated = false;

        /// <summary>
        /// Generates a new batch of ore in all zones.
        /// Will clean any existing ore veins.
        /// </summary>
        public static void GenerateWorld()
        {
            GameManager.local.StartCoroutine(InternalProcess());

            // Uses an IEnumerator to help divide the load across frames.
            IEnumerator InternalProcess()
            {
                // Should the world be cleared first?
                if (hasGenerated)
                { yield return ClearWorldGeneration(); }

                // Wait for the end of the frame.
                yield return Yielders.EndOfFrame;

                List<(Vector3[], Tier tier)> oreMap = new List<(Vector3[], Tier tier)>();
                foreach (var reference in Entry.References)
                {
                    // Is this a zone?
                    if (reference == null || reference.name != "Zones")
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
                        Tier tier = (Tier)System.Enum.Parse(typeof(Tier), zone.name.Replace("-Zone", string.Empty));

                        // Calculate a random amount.
                        Vector2 map = GetTierPopulation(tier);
                        int population = (int)Random.Range(map.x, map.y);

                        Logging.Log($"Generating {tier} Ore Map!");

                        // Generate population map.
                        Vector3[] ore = GenerateOrePopulationMap(zone, population);
                        oreMap.Add((ore, tier));

                        Logging.Log($"Generated {ore.Length}!");
                    }
                }

                // Finally spawn in all the ore.
                foreach (var data in oreMap)
                {
                    Logging.Log($"Spawning {data.tier} map!");

                    // Generate the ore.
                    yield return GenerateVeinsFromPopulationMap(data.Item1, data.tier);

                    Logging.Log($"{data.tier} spawned!");
                }

                // Set flag.
                hasGenerated = true;
            }
        }

        /// <summary>
        /// Generate ore in a zone.
        /// </summary>
        public static IEnumerator GenerateVeinsFromPopulationMap(this Vector3[] map, Tier tier)
        {
            Logging.Log($"{tier} Map Size: {map.Length}");

            // A collection of all ores under the target tier.
            List<string> ids = new List<string>();

            // Obtain all ore under the tier.
            foreach (var entry in Tycoon.ItemDatabase)
            {
                if (entry.Value.tier == tier && entry.Value is VeinItem)
                { ids.Add(entry.Key); }
            }

            // Spawn the ores.
            for (int i = 0; i < map.Length; i++)
            {
                int index = i;

                // Spawn a vein.
                string id = ids[Random.Range(0, ids.Count)];
                Tycoon.SpawnItem(id, map[index], Quaternion.identity, go =>
                {
                    Logging.Log($"@ {map[index]}");

                    // Randomize the scale.
                    float scale = Random.Range(0.8f, 1.2f);
                    go.transform.localScale = Vector3.one * scale;

                    // Randomize the rotation.
                    go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, Random.Range(-360, 360), go.transform.localEulerAngles.z);

                    // Load the vein up.
                    OreVein ore = go.AddComponent<OreVein>();
                    ore.Load(id);

                    // Add scale factor to health.
                    ore.data.health += (Mathf.Abs((1.0f - scale)) * 100);
                });

                yield return null;
            }
        }

        /// <summary>
        /// This will clear the world gen.
        /// </summary>
        private static IEnumerator ClearWorldGeneration()
        {
            Logging.Log($"Clearing world of {Tycoon.OreVeins.Count} veins!");

            // Despawn all veins.
            for (int i = Tycoon.OreVeins.Count - 1; i >= 0; i--)
            {
                Tycoon.OreVeins[i].Despawn();
                yield return null;
            }

            Tycoon.OreVeins.Clear();
            hasGenerated = false;

            Logging.Log("World cleared!");
        }

        /// <summary>
        /// Generate a map of positions for an ore zone.
        /// </summary>
        private static Vector3[] GenerateOrePopulationMap(Transform zone, int population)
        {
            List<Vector3> map = new List<Vector3>();

            // The maximum bounds to spawn.
            Bounds bounds = new Bounds(zone.position, zone.localScale / 2);

            // Spawn the ores.
            Vector3 origin = zone.position;
            for (int i = 0; i < population; i++)
            {
                // Position within the bounds.
                origin.x += Random.Range(-bounds.extents.x, bounds.extents.x);
                origin.z += Random.Range(-bounds.extents.z, bounds.extents.z);
                origin.y += bounds.extents.y;

                // Align to the surface.
                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200, ~0, QueryTriggerInteraction.Ignore))
                {
                    map.Add(hit.point);
                }
            }

            return map.ToArray();
        }

        /// <summary>
        /// Obtain a tier population map.
        /// </summary>
        private static Vector2 GetTierPopulation(Tier tier)
        {
            switch (tier)
            {
                case Tier.Very_Common:
                    return new Vector2(10, 20);
                case Tier.Common:
                    return new Vector2(8, 16);
                case Tier.UnCommon:
                    return new Vector2(6, 14);
                case Tier.Rare:
                    return new Vector2(4, 11);
                case Tier.Very_Rare:
                    return new Vector2(2, 10);
                case Tier.Extremely_Rare:
                    return new Vector2(1, 5);
                case Tier.So_Rare_Theres_Only_One:
                    return new Vector2(0, 1);
            }

            return Vector2.zero;
        }
    }
}