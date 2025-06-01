namespace HeatOptimizerApp.Models
{
    public class TimeSeriesPoint
    {
        public int Hour { get; set; }      // Hour of day (0-23)
        public double Value { get; set; }  // Value for heat demand or electricity price
    }
}