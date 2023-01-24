namespace MiningTycoon
{
    /// <summary>
    /// Stores information about a plot.
    /// </summary>
    [System.Serializable]
    public class PlotData
    {
        public int machineID; // Which plot does this data belong to?

        // How many doubloons this earns per tick.
        public float doubloonsPerTick = 5.0f;

        // Doubloon multiplier per tick.
        public float doubloonMultiplier = 1.0f;
    }
}