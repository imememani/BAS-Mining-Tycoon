using System;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    /// <summary>
    /// Tycoon utility class to aid with various stuff.
    /// </summary>
    public static class TycoonUtilities
    {
        /// <summary>
        /// Numerical representation of a.
        /// </summary>
        private static int A { get; } = Convert.ToInt32('a');

        /// <summary>
        /// Doubloon conversion table.
        /// </summary>
        private static Dictionary<int, string> Units { get; } = new Dictionary<int, string>
        {
            {0, ""},
            {1, "K"},
            {2, "M"},
            {3, "B"},
            {4, "T"}
        };

        /// <summary>
        /// Return a floaty text anchor in front of the player.
        /// </summary>
        public static Vector3 GetFloatyTextPlayerAnchor()
        {
            return Player.local.head.transform.position + (Player.local.head.transform.forward * 0.65f);
        }

        /// <summary>
        /// Format a doubloon string.
        /// </summary>
        public static string FormatDoubloons(this double value)
        {
            if (value < 1.0d)
            { return $"{value:0.00}"; }

            int n = (int)Math.Log(value, 1000);
            double m = value / Math.Pow(1000, n);
            string unit;

            if (n < Units.Count)
            {
                unit = Units[n];
            }
            else
            {
                int unitInt = n - Units.Count;
                int secondUnit = unitInt % 26;
                int firstUnit = unitInt / 26;
                unit = $"{Convert.ToChar($"{firstUnit}{A}")}{Convert.ToChar($"{secondUnit}{A}")}";
            }

            return $"{(Math.Floor(m * 100) / 100):0.00}{unit}";
        }


        /// <summary>
        /// Normalize a value between 0 and 1.
        /// </summary>
        public static float Normalize(this float value, float min, float max)
        {
            return (value - min) / (max - min);
        }
    }
}