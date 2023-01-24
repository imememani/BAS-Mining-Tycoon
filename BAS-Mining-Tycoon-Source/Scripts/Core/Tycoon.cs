using BASLogger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    /// <summary>
    /// The main handler for the tycoon world.
    /// </summary>
    public static class Tycoon
    {
        /// <summary>
        /// Invoked each tick (every 1 minute).
        /// </summary>
        public static event TycoonTickEvent Tick;

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
        /// Is the tycoon active?
        /// </summary>
        private static bool IsTycoonLoaded { get; set; }

        private static float timer;
        private static int worldRegenTickCounter;
        
        /// <summary>
        /// Initialize the tycoon.
        /// </summary>
        internal static void InitializeTycoon()
        {
            // Initialize the logger.
            Logging.CreateLogger(Path.Combine(Entry.Location, "Logs"), "Mining Tycoon");

            // Attach a console to this mod.
            // DELETE THIS LINE WHEN YOU HAVE FINISHED DEBUGGING.
            Logging.CreateConsole();

            // Create any directories required.
            if (!Directory.Exists(Path.Combine(Entry.Location, "Saves")))
            {
                Directory.CreateDirectory(Path.Combine(Entry.Location, "Saves"));
                Logging.Log("Recreated the 'Saves' directory.");
            }
            if (!Directory.Exists(Path.Combine(Entry.Location, "Data")))
            {
                Directory.CreateDirectory(Path.Combine(Entry.Location, "Data"));
                Logging.Log("Recreated the 'Data' directory.");
            }

            Logging.Log("Loading items...");
            LoadItems();

            // Add the shop UI.
            Logging.Log("Loading shop...");
            Entry.References[Entry.References.Count - 1].transforms[0].gameObject.AddComponent<TycoonShop>();

            // Generate ore.
            Logging.Log("Generating ore veins...");
            OreGenerator.GenerateOreVeins();

            // 1 minute reset.
            timer = Time.time + 60.0f;

            IsTycoonLoaded = true;
            Logging.Log("Tycoon initialized!");
        }

        /// <summary>
        /// Ran each frame.
        /// </summary>
        internal static void TycoonUpdate()
        {
            if (Time.time > timer)
            {
                // 1 minute reset.
                timer = Time.time + 60.0f;

                // Run a tick.
                Tick?.Invoke();

                // Bump ticks.
                worldRegenTickCounter++;
            }
            else
            { TycoonShop.local.tickTimer.fillAmount = TycoonUtilities.Normalize(Time.time, timer - 60.0f, timer); }

            // 20 Ticks (20 minutes)
            if (worldRegenTickCounter >= 20)
            {
                // Save game.
                TycoonSaveHandler.Save();

                // Regenerate the world.
                OreGenerator.RegenerateWorldVeins();

                // Reste ticks.
                worldRegenTickCounter = 0;
            }
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

            // Save.
            TycoonSaveHandler.Save();

            // Clear data.
            Entry.References.Clear();
            ItemDatabase.Clear();
            WorldObjects.Clear();
            OreVeins.Clear();
            TycoonSaveHandler.Dispose();
            Logging.Log("Unloaded!");

            // Close the logger.
            Logging.CloseLog();
        }

        /// <summary>
        /// Is the target an item?
        /// </summary>
        public static bool IsTycoonItem(string id) => ItemDatabase.ContainsKey(id);

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
        /// Inject item data into the target item to make it functional without a json.
        /// </summary>
        public static void InjectItemData(this ThunderRoad.Item item, string id)
        {
            Catalog.LoadAssetAsync<GameObject>("MiningTycoon.BaseOre", go =>
            {
                // Obtain the reference.
                ThunderRoad.Item thunderItem = go.GetComponent<ThunderRoad.Item>();

                // Override the data.
                item.data.displayName = id;

            }, "Tycoon->InjectItemData");
        }

        /// <summary>
        /// Load the database.
        /// </summary>
        private static void LoadItems()
        {
            string[] files = Directory.GetFiles(Path.Combine(Entry.Location, "Data"), "Tycoon_*.json", SearchOption.AllDirectories);
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