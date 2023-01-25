namespace MiningTycoon
{
    /// <summary>
    /// Stores information about a plot.
    /// </summary>
    [System.Serializable]
    public class PlotData
    {
        // Which plot does this data belong to?
        public int machineID;

        // How many doubloons this earns per tick.
        public double doubloonsPerTick = 5.0f;

        // Doubloon multiplier per tick.
        public double doubloonMultiplier = 1.0f;
    }
}