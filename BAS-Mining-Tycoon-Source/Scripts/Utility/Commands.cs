using IngameDebugConsole;
using ThunderRoad;
using Unity.Collections.LowLevel.Unsafe;

namespace MiningTycoon
{
    public class Commands
    {
        [ConsoleMethod("Tycoon-regen", "Regenerates the tycoon ore map.")]
        public static void Regenerate() => OreGenerator.GenerateWorld();

        [ConsoleMethod("Tycoon-save", "Force the tycoon to save.")]
        public static void Save() => TycoonSaveHandler.Save();

        [ConsoleMethod("Tycoon-load", "Force the tycoon to load.")]
        public static void Load() => TycoonSaveHandler.Load();

        [ConsoleMethod("Tycoon-reset", "Reset the current tycoon.")]
        public static void Reset() => TycoonSaveHandler.Reset();

        [ConsoleMethod("Tycoon-add-doubloons", "Give yourself doubloons.", "doubloons")]
        public static void AddDoubloons(double amount) => TycoonSaveHandler.Current.AddCurrency(amount);

        [ConsoleMethod("Tycoon-add-ore", "Give yourself processed ore.", "ore")]
        public static void AddOre(int amount) => TycoonSaveHandler.Current.AddOre(amount);

        [ConsoleMethod("Tycoon-spawn", "Spawn a tycoon item or creature.", "id", "isCreature")]
        public static void Spawn(string id, bool creature) { if (creature) Tycoon.SpawnTycoonCreature(id, TycoonUtilities.GetFloatyTextPlayerAnchor(), 0); else Tycoon.SpawnTycoonItem(id, TycoonUtilities.GetFloatyTextPlayerAnchor(), UnityEngine.Quaternion.identity, null); }
    }
}