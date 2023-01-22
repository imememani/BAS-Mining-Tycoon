using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
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
        /// Location of this mod.
        /// </summary>
        private static string Location { get; set; }

        public override IEnumerator OnLoadCoroutine()
        {
            Location = Path.Combine(Application.streamingAssetsPath, "Mods", "Mining Tycoon");
            LoadItems();

            // For testing.
            foreach (var ore in level.customReferences[0].transforms)
            {
                ore.gameObject.AddComponent<OreVein>();
            }

            return base.OnLoadCoroutine();
        }

        /// <summary>
        /// Spawn a tycoon icon by id.
        /// </summary>
        public static void SpawnItem(string id, Vector3 position, Quaternion rotation, Action<GameObject> onItemSpawned = null)
        {
            if (!ItemDatabase.TryGetValue(id, out Item item))
            {
                Debug.LogError($"Item '{id}' does not exist in the tycoon database.");
                return;
            }

            Debug.Log($"Spawning '{id}' @ '{item.address}'");

            Catalog.InstantiateAsync(item.address, position, rotation, null, go => onItemSpawned?.Invoke(go), "Tycoon->Spawn");
        }

        /// <summary>
        /// Load the database.
        /// </summary>
        private void LoadItems()
        {
            string[] files = Directory.GetFiles(Path.Combine(Location, "Items"), "*.json", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                Item item = JsonConvert.DeserializeObject<Item>(File.ReadAllText(file), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
                ItemDatabase.Add(item.id, item);
            }
        }
    }
}