using Newtonsoft.Json;
using UnityEngine;

namespace MiningTycoon
{
    /// <summary>
    /// Serializable vector.
    /// </summary>
    [System.Serializable]
    public struct SVector
    {
        public float x, y, z;

        [JsonIgnore]
        public Vector3 ToVector3 { get => new Vector3(x, y, z); }

        public SVector From(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;

            return this;
        }
    }
}
