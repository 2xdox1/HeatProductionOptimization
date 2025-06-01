namespace HeatOptimizerApp.Models
{
    public class HourlyHeatProduction
    {
        public int Hour { get; set; }
        public string UnitName { get; set; } = "";
        public double HeatProduced { get; set; }
    }
}