namespace HeatOptimizerApp.Models
{
    public class SummaryMetrics
    {
        public double TotalMaxHeat { get; set; }
        public double TotalCost { get; set; }
        public double TotalCO2 { get; set; }
        public double TotalGas { get; set; }
        public double TotalOil { get; set; }
        public double TotalElectricityCapacity { get; set; }

        public string FormattedHeat => $"{TotalMaxHeat:N2} MW heat capacity";
        public string FormattedCost => $"{TotalCost:N0} DKK total cost (configured)";
        public string FormattedCO2 => $"{TotalCO2:N0} kg CO₂ (configured)";
        public string FormattedGas => $"{TotalGas:N2} MWh (gas)";
        public string FormattedOil => $"{TotalOil:N2} MWh (oil)";
        public string FormattedElectricity => $"{TotalElectricityCapacity:N2} MW electricity capacity";
    }
}