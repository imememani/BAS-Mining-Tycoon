using IngameDebugConsole;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    // TODO: Serialization refactor, want it to be a bit more generic.

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

        /// <summary>
        /// ID of this tycoon.
        /// </summary>
        public static string TycoonLevelID { get; private set; }

        public override void Update() => Tycoon.TycoonUpdate();

        public override IEnumerator OnLoadCoroutine()
        {
            // Add commands.
            DebugLogConsole.AddCommand("Tycoon-regen", "Regenerates the tycoon ore map.", Commands.Regenerate);
            DebugLogConsole.AddCommand("Tycoon-save", "Force the tycoon to save.", Commands.Save);
            DebugLogConsole.AddCommand("Tycoon-load", "Force the tycoon to load.", Commands.Load);
            DebugLogConsole.AddCommand("Tycoon-reset", "Reset the current tycoon.", Commands.Reset);
            DebugLogConsole.AddCommand<double>("Tycoon-add-doubloons", "Give yourself doubloons.", Commands.AddDoubloons, "doubloons");
            DebugLogConsole.AddCommand<int>("Tycoon-add-ore", "Give yourself processed ore.", Commands.AddOre, "ore");
            DebugLogConsole.AddCommand<string, bool>("Tycoon-spawn", "Spawn a tycoon item or creature.", Commands.Spawn, "id", "isCreature");

            Location = Path.Combine(Application.streamingAssetsPath, "Mods", "Mining Tycoon");

            if (!Directory.Exists(Location))
            {
                Debug.LogError($"[Mining Tycoon][!!! FATAL ERROR !!!] Can not find mod binaries at path: {Location}");
                return base.OnLoadCoroutine();
            }

            References = level.customReferences;
            TycoonLevelID = level.data.id;

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