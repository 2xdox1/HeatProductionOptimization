using System;

namespace HeatOptimizerApp.Models
{
    public class SimulatedResult
    {
        public DateTime Time { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public double HeatProduced { get; set; }
        public double Cost { get; set; }
        public double CO2 { get; set; }
        public double? ElectricityProduced { get; set; }
        public double? ElectricityConsumed { get; set; }
    }
}
