using BASLogger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
            //Logging.CreateConsole();

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
            try
            { LoadItems(); }
            catch (Exception e)
            { e.LogError(); }

            // Add the shop UI.
            Logging.Log("Loading shop...");
            try
            {
                if (Entry.GetReference("Shop") is Level.CustomReference shop)
                {
                    // Spawn the shop.
                    Transform target = shop.transforms[0];
                    Catalog.InstantiateAsync("Tycoon.Shop", target.position, target.rotation, target.parent, go =>
                    {
                        UnityEngine.Object.Destroy(target.gameObject);
                        go.AddComponent<TycoonShop>();
                    }, "Tycoon->ShopSpawn");
                }
                else
                {
                    Logging.LogError("NO TYCOON SHOP FOUND!");
                }
            }
            catch (Exception e)
            { e.LogError(); }

            Logging.Log("Loading sell zones...");
            try
            {
                if (Entry.GetReference("SellZone") is Level.CustomReference shop)
                {
                    foreach (Transform target in shop.transforms)
                    {
                        // Spawn the zone.
                        Catalog.InstantiateAsync("Tycoon.SellZone", target.position, target.rotation, target.parent, go =>
                        {
                            UnityEngine.Object.Destroy(target.gameObject);
                            go.AddComponent<SellZone>();
                        }, "Tycoon->SellZones");
                    }
                }
                else
                {
                    Logging.LogError("NO SELL ZONES FOUND!");
                }
            }
            catch (Exception e)
            { e.LogError(); }

            // Generate ore.
            Logging.Log("Generating ore veins...");
            try
            { OreGenerator.GenerateWorld(); }
            catch (Exception e)
            { e.LogError(); }

            // 1 minute reset.
            timer = Time.time + 60.0f;

            // Set the fog colour.
            RenderSettings.fogColor = new Color32(50, 50, 50, 125);

            IsTycoonLoaded = true;
            Logging.Log("Tycoon initialized!");
        }

        /// <summary>
        /// Ran each frame.
        /// </summary>
        internal static void TycoonUpdate()
        {
            // Await everything to load in.
            if (TycoonShop.local == null)
            { return; }

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
                try
                { OreGenerator.GenerateWorld(); }
                catch (Exception e)
                { e.LogError(); }

                // Reset ticks.
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
        /// Spawn a creature by id.
        /// </summary>
        public static void SpawnTycoonCreature(string id, Vector3 position, float rotationY, Action<TycoonWorldObject> onItemSpawned = null)
        {
            CreatureData data = Catalog.GetData<CreatureData>(id);
            Logging.Log($"Spawning creature '{id}'!");

            data.SpawnAsync(position, rotationY, null, false, callback: creature =>
            {
                TycoonWorldObject worldObject = creature.gameObject.AddComponent<TycoonWorldObject>();
                worldObject.LoadCreature(creature.data);

                onItemSpawned?.Invoke(worldObject);
            });
        }

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
        /// Generate an item icon if supported.
        /// </summary>
        public static void GenerateItemIcon(this Item item, Action<Texture2D> onTextureLoaded)
        {
            LoadObject<GameObject>(item.address, async go =>
            {
                Logging.Log($"Generating icon for {item.id}!");
                ThunderRoad.Item thunderItem = go.GetComponent<ThunderRoad.Item>();

                // Try forcing a load.
                if (!thunderItem.loaded)
                { thunderItem.Load(Catalog.GetData<ItemData>(item.id)); }

                // Try wait for any load events.
                TimeSpan timeout = TimeSpan.FromSeconds(5);
                TimeSpan second = TimeSpan.FromSeconds(1);
                while (thunderItem != null && !thunderItem.loaded && timeout.Seconds > 0)
                {
                    await Task.Delay(second);
                    timeout = timeout.Subtract(second);
                }

                // Item exist?
                if (thunderItem == null || thunderItem.data == null)
                {
                    Logging.Log($"Object does not contain any ItemData!");
                    return;
                }

                // Does an icon already exist?
                if (thunderItem.data.icon != null)
                {
                    Logging.Log($"Object already has an icon cached!");
                    onTextureLoaded?.Invoke(thunderItem.data.icon.ConvertTexture());
                    return;
                }

                // Run the generation.
                GameManager.local.StartCoroutine(ObjectViewer.CreateIcon(thunderItem.data));

                // Try wait for any generation events.
                timeout = TimeSpan.FromSeconds(5);
                while (thunderItem.data.icon == null && timeout.Seconds > 0)
                {
                    await Task.Delay(second);
                    timeout = timeout.Subtract(second);
                }

                Logging.Log(thunderItem.data.icon != null ? "Icon generated!" : "Unable to generate icon!");

                if (thunderItem.data.icon != null)
                { onTextureLoaded?.Invoke(thunderItem.data.icon.ConvertTexture()); }
            });
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