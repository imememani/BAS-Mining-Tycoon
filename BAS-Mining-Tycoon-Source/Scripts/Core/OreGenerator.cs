using System.Collections;
using ThunderRoad;
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
            GameManager.local.StartCoroutine(InternalProcess());

            // Uses an IEnumerator to help divide the load across frames.
            IEnumerator InternalProcess()
            {
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
                        string tier = zone.name.Replace("-Zone", string.Empty);

                        // Generate a new batch for this zone.
                        zone.GenerateOreInZone(tier);

                        // Wait for next frame.
                        yield return null;
                    }
                }
            }
        }

        /// <summary>
        /// Generate ore in a zone.
        /// </summary>
        public static void GenerateOreInZone(this Transform zoneObject, string tier)
        {
            // The maximum bounds to spawn.
            Bounds bounds = new Bounds(zoneObject.position, zoneObject.localScale / 2);

            // Obtain a copy of the vein data.
            VeinItem vein = (VeinItem)Tycoon.ItemDatabase[tier.ToString()];

            // Calculate a random amount.
            Vector2 map = new Vector2(vein.minSpawn, vein.maxSpawn);
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
                Tycoon.SpawnItem(vein.id, origin, Quaternion.identity, go =>
                {
                    // Randomize the scale.
                    float scale = Random.Range(0.8f, 1.2f);
                    go.transform.localScale = Vector3.one * scale;

                    // Align to the surface.
                    if (Physics.Raycast(go.transform.position, Vector3.down, out RaycastHit hit, 200, ~0, QueryTriggerInteraction.Ignore))
                    {
                        go.transform.SetPositionAndRotation(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal * 0.5f));
                    }

                    // Randomize the rotation.
                    go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, Random.Range(-360, 360), go.transform.localEulerAngles.z);

                    // Load the vein up.
                    OreVein ore = go.AddComponent<OreVein>();
                    ore.Load(tier.ToString());

                    // Add scale factor to health.
                    ore.data.health += (Mathf.Abs((1.0f - scale)) * 100);
                });
            }
        }

        /// <summary>
        /// This will regenerate the worlds ore veins, this should only be called AFTER GenerateOreVeins.
        /// </summary>
        public static void RegenerateWorldVeins()
        {
            GameManager.local.StartCoroutine(InternalProcess());

            // Uses an IEnumerator to help divide the load across frames.
            // Doing it all in one will cause a lag spike.
            IEnumerator InternalProcess()
            {
                // Despawn all veins.
                for (int i = Tycoon.OreVeins.Count - 1; i >= 0; i--)
                {
                    // TODO: Have a fancy anim for despawning ore veins.
                    Object.Destroy(Tycoon.OreVeins[i].gameObject);
                    yield return null;
                }

                Tycoon.OreVeins.Clear();

                // Spawn new veins.
                GenerateOreVeins();
            }
        }
    }
}