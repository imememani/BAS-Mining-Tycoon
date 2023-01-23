using BASLogger;
using Newtonsoft.Json;
using System.IO;
using ThunderRoad;

namespace MiningTycoon
{
    /// <summary>
    /// Handles the players save state.
    /// </summary>
    public static class TycoonSaveHandler
    {
        /// <summary>
        /// The current tycoon player.
        /// </summary>
        public static TycoonPlayer Current { get; private set; }

        /// <summary>
        /// Location of the players save.
        /// </summary>
        private static string SaveLocation { get => Path.Combine(Entry.Location, "Saves", "player0.tycoon"); }

        /// <summary>
        /// Save the current session.
        /// </summary>
        public static void Save()
        {
            // Any player to save?
            Current = Current ?? new TycoonPlayer();

            // Update any required data.
            Current.Refresh();

            // Serialize the data.
            File.WriteAllText(SaveLocation, JsonConvert.SerializeObject(Current));

            Logging.Log("Current session saved!");
        }

        /// <summary>
        /// Try load an existing save.
        /// </summary>
        public static bool Load()
        {
            try
            {
                // Anything to load?
                if (!File.Exists(SaveLocation))
                {
                    Logging.Log("Creating a new session!");
                    Current = new TycoonPlayer();
                    return true;
                }

                // Deserialize the data.
                Current = JsonConvert.DeserializeObject<TycoonPlayer>(File.ReadAllText(SaveLocation));

                // Load the data.
                Player.local.transform.position = Current.position.ToVector3;
                Player.local.locomotion.rb.velocity = Current.velocity.ToVector3;

                foreach (TycoonObjectData objectData in Current.worldObjects)
                {
                    objectData.Load();
                    Logging.Log($"Loaded Object: {objectData.itemID}");
                }

                Logging.Log("Loaded previous session!");
                return true;
            }
            catch (System.Exception e)
            {
                Logging.Log("Unable to load previous session!");
                Logging.LogError(e);

                Current = new TycoonPlayer();
            }

            return false;
        }
    }
}