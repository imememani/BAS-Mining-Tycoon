using BASLogger;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    // TODO: Need to configure ore vein json spawn rates.

    /// <summary>
    /// Mod entry.
    /// 
    /// Invoked when loading a tycoon level.
    /// </summary>
    public class Entry : LevelModule
    {
        /// <summary>
        /// Item database.
        /// </summary>
        public static Dictionary<string, Item> ItemDatabase { get; } = new Dictionary<string, Item>();

        /// <summary>
        /// All current tycoon objects.
        /// </summary>
        public static List<TycoonWorldObject> WorldObjects { get; } = new List<TycoonWorldObject>();

        /// <summary>
        /// All current veins.
        /// </summary>
        public static List<OreVein> OreVeins { get; } = new List<OreVein>();

        /// <summary>
        /// All level references.
        /// </summary>
        public static List<Level.CustomReference> References { get; private set; }

        /// <summary>
        /// Location of this mod.
        /// </summary>
        public static string Location { get; set; }

        /// <summary>
        /// Is the tycoon active?
        /// </summary>
        private static bool IsTycoonLoaded { get; set; }

        public override IEnumerator OnLoadCoroutine()
        {
            Location = Path.Combine(Application.streamingAssetsPath, "Mods", "Mining Tycoon");

            if (!Directory.Exists(Location))
            {
                Debug.LogError($"[Mining Tycoon][!!! FATAL ERROR !!!] Can not find mod binaries at path: {Location}");
                return base.OnLoadCoroutine();
            }

            // Initialize the logger.
            Logging.CreateLogger(Path.Combine(Location, "Logs"), "Mining Tycoon");

            // Attach a console to this mod.
            // DELETE THIS LINE WHEN YOU HAVE FINISHED DEBUGGING.
            Logging.CreateConsole();

            // Create any directories required.
            if (!Directory.Exists(Path.Combine(Location, "Saves")))
            {
                Directory.CreateDirectory(Path.Combine(Location, "Saves"));
                Logging.Log("Recreated the 'Saves' directory.");
            }
            if (!Directory.Exists(Path.Combine(Location, "Data")))
            {
                Directory.CreateDirectory(Path.Combine(Location, "Data"));
                Logging.Log("Recreated the 'Data' directory.");
            }

            References = level.customReferences;

            Logging.Log("Loading items...");
            LoadItems();

            // Add the shop UI.
            Logging.Log("Loading shop...");
            level.customReferences[level.customReferences.Count - 1].transforms[0].gameObject.AddComponent<TycoonShop>();

            // Generate ore.
            Logging.Log("Generating ore veins...");
            OreGenerator.GenerateOreVeins();

            IsTycoonLoaded = true;
            Logging.Log("Tycoon initialized!");
            return base.OnLoadCoroutine();
        }

        public override IEnumerator OnPlayerSpawnCoroutine()
        {
            // Load the player.
            TycoonSaveHandler.Load();

            return base.OnPlayerSpawnCoroutine();
        }

        public override void OnUnload()
        {
            UnloadTycoon();
            base.OnUnload();
        }

        /// <summary>
        /// Safely unload the tycoon.
        /// </summary>
        public static void UnloadTycoon()
        {
            // No need to unload again.
            if (!IsTycoonLoaded)
            { return; }

            IsTycoonLoaded = false;
            Logging.Log("Tycoon unloading...");
            Logging.Log("This may show randomly in the current log with an older timestamp, this is normal.");

            // Save.
            TycoonSaveHandler.Save();

            // Clear data.
            References.Clear();
            ItemDatabase.Clear();
            WorldObjects.Clear();
            OreVeins.Clear();
            TycoonSaveHandler.Dispose();
            Logging.Log("Unloaded!");

            // Close the logger.
            Logging.CloseLog();
        }

        /// <summary>
        /// Spawn a serialized tycoon item.
        /// </summary>
        public static void SpawnTycoonItem(string id, Vector3 position, Quaternion rotation, Action<TycoonWorldObject> onItemSpawned = null) => SpawnItem(id, position, rotation, go => onItemSpawned?.Invoke(go.GetComponent<TycoonWorldObject>()), true);

        /// <summary>
        /// Spawn a non-serialized tycoon item.
        /// </summary>
        public static void SpawnItem(string id, Vector3 position, Quaternion rotation, Action<GameObject> onItemSpawned = null, bool serialized = false)
        {
            if (!ItemDatabase.TryGetValue(id, out Item item))
            {
                Logging.LogError($"Item '{id}' does not exist in the tycoon database.");
                return;
            }

            Logging.Log($"Spawning{(serialized ? " serialized item" : "")} '{id}' @ '{item.address}'!");
            Catalog.InstantiateAsync(item.address, position, rotation, null, go =>
            {
                if (serialized)
                {
                    TycoonWorldObject worldObject = go.AddComponent<TycoonWorldObject>();
                    worldObject.Load(id);
                }

                onItemSpawned?.Invoke(go);
            }, "Tycoon->Spawn");
        }

        /// <summary>
        /// Spawn an object.
        /// </summary>
        public static void LoadObject<T>(string address, Action<T> onSpawned = null) where T : UnityEngine.Object
        {
            Logging.Log($"Loading object @ '{address}'!");
            Catalog.LoadAssetAsync(address, onSpawned, "Tycoon->SpawnObject");
        }

        /// <summary>
        /// Load the database.
        /// </summary>
        private void LoadItems()
        {
            string[] files = Directory.GetFiles(Path.Combine(Location, "Data"), "Tycoon_*.json", SearchOption.AllDirectories);
            Logging.Log($"Found {files.Length} tycoon items.");

            foreach (string file in files)
            {
                try
                {
                    Item item = JsonConvert.DeserializeObject<Item>(File.ReadAllText(file), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
                    ItemDatabase.Add(item.id, item);

                    Logging.Log($"Registered '{item.id}' to the tycoon database!");
                }
                catch (Exception e)
                {
                    Logging.LogError(e);
                }
            }
        }
    }
}