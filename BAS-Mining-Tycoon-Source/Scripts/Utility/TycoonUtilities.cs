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
        /// Return a floaty text anchor in front of the player.
        /// </summary>
        public static Vector3 GetFloatyTextPlayerAnchor() 
        {
            return Player.local.head.transform.position + (Player.local.head.transform.forward * 0.65f);
        }

        /// <summary>
        /// Format a doubloon string.
        /// </summary>
        public static string FormatDoubloons(this float value)
        {
            // TODO: Add, a, ab, ac, ad, etc so we can get past the 2bn limit.
            return $"{value:0.00}";
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