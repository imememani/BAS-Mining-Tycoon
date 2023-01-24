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
        /// All level references.
        /// </summary>
        public static List<Level.CustomReference> References { get; private set; }

        /// <summary>
        /// Location of this mod.
        /// </summary>
        public static string Location { get; set; }

        public override void Update() => Tycoon.TycoonUpdate();

        public override IEnumerator OnLoadCoroutine()
        {
            Location = Path.Combine(Application.streamingAssetsPath, "Mods", "Mining Tycoon");

            if (!Directory.Exists(Location))
            {
                Debug.LogError($"[Mining Tycoon][!!! FATAL ERROR !!!] Can not find mod binaries at path: {Location}");
                return base.OnLoadCoroutine();
            }

            References = level.customReferences;

            return base.OnLoadCoroutine();
        }

        public override IEnumerator OnPlayerSpawnCoroutine()
        {
            // Initialize the tycoon.
            Tycoon.InitializeTycoon();

            // Load the player.
            TycoonSaveHandler.Load();

            return base.OnPlayerSpawnCoroutine();
        }

        public override void OnUnload()
        {
            Tycoon.UnloadTycoon();
            base.OnUnload();
        }
    }
}