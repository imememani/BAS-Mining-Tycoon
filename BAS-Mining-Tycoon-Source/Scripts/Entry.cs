using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ThunderRoad;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
        /// All level references.
        /// </summary>
        public static List<Level.CustomReference> References { get; private set; }

        /// <summary>
        /// Location of this mod.
        /// </summary>
        private static string Location { get; set; }

        public override IEnumerator OnLoadCoroutine()
        {
            Location = Path.Combine(Application.streamingAssetsPath, "Mods", "Mining Tycoon");

            References = level.customReferences;
            LoadItems();

            // Add the shop UI.
            level.customReferences[level.customReferences.Count - 1].transforms[0].gameObject.AddComponent<TycoonShop>();

            // Generate ore.
            OreGenerator.GenerateOreVeins();

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
        /// Spawn an object.
        /// </summary>
        public static void SpawnObject<T>(string address, bool spawn = true, Action<T> onSpawned = null) where T : UnityEngine.Object
        {
            GameManager.local.StartCoroutine(InternalSpawn());

            IEnumerator InternalSpawn()
            {
                // Get the operation handle.
                AsyncOperationHandle<T> assetHandle = Addressables.LoadAssetAsync<T>(address);

                // Wait and load.
                yield return assetHandle;

                if (assetHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    // Spawn the asset.
                    T asset = spawn ? UnityEngine.Object.Instantiate(assetHandle.Result) : assetHandle.Result;

                    // Return the target asset.
                    onSpawned?.Invoke(asset);
                }
                else
                {
                    Debug.LogError($"[{address}] is invalid[{assetHandle.Status}], could not find any asset!");
                }
            }
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