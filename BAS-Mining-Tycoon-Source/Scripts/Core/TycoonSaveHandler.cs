using BASLogger;
using MiningTycoon.Scripts.Core;
using Newtonsoft.Json;
using System.IO;
using ThunderRoad;
using UnityEngine;

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
        /// Invoked when the player has loaded.
        /// </summary>
        public static event PlayerLoadedEvent PlayerLoaded;

        /// <summary>
        /// Dispose of the current session.
        /// </summary>
        public static void Dispose()
        {
            PlayerLoaded = null;
            Current = null;
        }

        /// <summary>
        /// Reset the players tycoon.
        /// </summary>
        public static void Reset()
        {
            // Delete the save file.
            if (File.Exists(SaveLocation))
            { File.Delete(SaveLocation); }

            // Reload the tycoon.
            GameManager.LoadLevel(Entry.TycoonLevelID);
        }

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

            // Display floaty.
            TycoonFloatyText.CreateFloatyText($"<color=green>Tycoon Saved!</color>",
                                                TycoonUtilities.GetFloatyTextPlayerAnchor() - new Vector3(0, 0.1f, 0),
                                                Player.local.head.transform,
                                                    2.0f);

            Logging.Log("Current session saved!");
        }

        /// <summary>
        /// Try load an existing save.
        /// </summary>
        public static bool Load()
        {
            try
            {
                // Strip the player.

                // Items.
                for (int i = Player.local.creature.container.contents.Count - 1; i >= 0; i--)
                {
                    if (Player.local.creature.container.contents[i].reference == ContainerData.Content.Reference.Item)
                    { Player.local.creature.container.contents.RemoveAt(i); }
                }

                // Spells.
                TycoonUtilities.RemoveMagic();

                // Anything to load?
                if (!File.Exists(SaveLocation))
                {
                    Logging.Log("Creating a new session!");
                    Current = new TycoonPlayer();
                }
                else
                {
                    // Deserialize the data.
                    Current = JsonConvert.DeserializeObject<TycoonPlayer>(File.ReadAllText(SaveLocation));

                    // Load player position/velocity.
                    Player.local.transform.position = Current.position.ToVector3;
                    Player.local.locomotion.rb.velocity = Current.velocity.ToVector3;

                    // Display floaty.
                    TycoonFloatyText.CreateFloatyText($"<color=green>Welcome Back!</color>",
                                                        TycoonUtilities.GetFloatyTextPlayerAnchor(),
                                                        Player.local.head.transform,
                                                            2.0f);
                }

                // Load world objects.
                foreach (TycoonObjectData objectData in Current.worldObjects)
                {
                    objectData.Load();
                    Logging.Log($"Loaded Object: {objectData.itemID}");
                }

                // Load plots.
                int index = 0;
                foreach (Transform plotObject in Entry.References[1].transforms)
                {
                    Plot plot = plotObject.gameObject.AddComponent<Plot>();
                    plot.MachineID = index;

                    // Try load the plot data.
                    for (int i = 0; i < Current.plots.Count; i++)
                    {
                        if (Current.plots[i].machineID == plot.MachineID)
                        {
                            plot.Load(Current.plots[i]);
                            break;
                        }
                    }

                    plot.RefreshPlotUI();
                    Logging.Log($"PLOT LOADED: {plotObject}[{plot.MachineID}]");
                    index++;
                }

                // Trigger events.
                PlayerLoaded?.Invoke(Current);

                Logging.Log("Loaded previous session!");
                return true;
            }
            catch (System.Exception e)
            {
                Logging.LogError("Unable to load previous session!");
                Logging.LogError(e);

                Current = new TycoonPlayer();
            }

            return false;
        }
    }
}