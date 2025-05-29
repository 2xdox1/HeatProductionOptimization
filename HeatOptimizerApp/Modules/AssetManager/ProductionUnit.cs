namespace HeatOptimizerApp.Modules.AssetManager
{
    public class ProductionUnit
    {
        public string Name { get; set; } = "";
        public double MaxHeat { get; set; }
        public double? MaxElectricity { get; set; }
        public double ProductionCost { get; set; }
        public double? CO2Emission { get; set; }
        public double? GasConsumption { get; set; }
        public double? OilConsumption { get; set; }
    }
}
