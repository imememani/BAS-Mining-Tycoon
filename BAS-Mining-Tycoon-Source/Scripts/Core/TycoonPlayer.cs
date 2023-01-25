using MiningTycoon.Scripts.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    /// <summary>
    /// Player tycoon save data.
    /// </summary>
    [System.Serializable]
    public class TycoonPlayer
    {
        // Aka doubloons.
        public double doubloons = 100.0d;

        // How many ores have been processed.
        public long oresCollected = 0;

        // The exact date this player was created.
        public TimeSpan playtime;

        // Players last position.
        public SVector position;

        // Players last velocity.
        public SVector velocity;

        // All world objects.
        public TycoonObjectData[] worldObjects = Array.Empty<TycoonObjectData>();

        // All unlocked plots.
        public List<PlotData> plots = new List<PlotData>();

        // Invoked when change has happened.
        [JsonIgnore]
        public Action<double> currencyChanged;
        [JsonIgnore]
        public Action<int> oreChanged;

        // Set on load.
        [JsonIgnore]
        private DateTime loadTime;

        public TycoonPlayer()
        {
            loadTime = DateTime.Now;
        }

        /// <summary>
        /// Add or subtract currency.
        /// </summary>
        public void AddCurrency(double amount, bool showFloaty = false)
        {
            doubloons += amount;
            currencyChanged?.Invoke(amount);

            if (showFloaty)
            {
                // Play sound.
                // 0 - Purchase.
                AudioSource.PlayClipAtPoint(TycoonShop.local.shopSFX.sounds[0], Vector3.zero);

                // Display floaty.
                TycoonFloatyText.CreateFloatyCurrency($"\n     <color={(amount < 0 ? "red" : "white")}>{(amount < 0 ? "-" : "")}{amount.FormatDoubloons()}</color>",
                                            TycoonUtilities.GetFloatyTextPlayerAnchor(),
                                           Player.local.head.transform,
                                               3.0f);
            }
        }

        /// <summary>
        /// Add or subtract ore.
        /// </summary>
        public void AddOre(int amount)
        {
            oresCollected = (long)Mathf.Clamp(oresCollected + amount, 0, long.MaxValue);
            oreChanged?.Invoke(amount);
        }

        /// <summary>
        /// Get the current playtime.
        /// </summary>
        public TimeSpan GetRealtimePlaytime() => playtime.Add(DateTime.Now.Subtract(loadTime));

        /// <summary>
        /// Update this instances data, like the position and velocity.
        /// </summary>
        public void Refresh()
        {
            // Map the players position and rotation.
            position = new SVector().From(Player.local.transform.position);
            velocity = new SVector().From(Player.local.locomotion.rb.velocity);

            // Update playtime.
            playtime = playtime.Add(DateTime.Now.Subtract(loadTime));

            // Under the map?
            if (position.y <= -10.0f)
            {
                // Plop 'em back on the map.
                position.y = 5.0f;
            }

            // Map all objects.
            worldObjects = new TycoonObjectData[Tycoon.WorldObjects.Count];
            for (int i = 0; i < Tycoon.WorldObjects.Count; i++)
            {
                worldObjects[i] = Tycoon.WorldObjects[i].Serialize();
            }
        }
    }
}