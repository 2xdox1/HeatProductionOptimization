using HeatOptimizerApp.Modules.AssetManager;
using System.Collections.Generic;
using HeatOptimizerApp.Modules.SourceDataManager;
using HeatOptimizerApp.Modules.Optimizer;
using HeatOptimizerApp.Modules.ResultDataManager;
using HeatOptimizerApp.Modules.DataVisualization;
using HeatOptimizerApp.Models;
using System;

namespace HeatOptimizerApp.Modules.Core
{
    public class ProjectController
    {
        private readonly AssetManager.AssetManager _assetManager = new();
        private readonly SourceDataManager.SourceDataManager _sourceDataManager = new();
        private readonly Optimizer.Optimizer _optimizer = new();
        private readonly ResultDataManager.ResultDataManager _resultDataManager = new();
        private readonly DataVisualization.DataVisualization _dataVisualization = new();

        // Exposed preloaded time series
        public List<double> SummerHeatDemand { get; private set; } = new();
        public List<double> SummerElectricityPrices { get; private set; } = new();
        public List<double> WinterHeatDemand { get; private set; } = new();
        public List<double> WinterElectricityPrices { get; private set; } = new();

        public void RunProject()
        {
            Console.WriteLine("=== Heat Optimization Project ===");

            _assetManager.LoadData("./Data/ProductionUnits.csv");

            PreloadTimeSeries();

            // Choose default season for optimization
            _optimizer.LoadData("Data/summer.csv");
            _optimizer.RunOptimization();

            _dataVisualization.LoadData(""); // TODO: hook up preloaded data
            _dataVisualization.DisplayChart();
        }

        private void PreloadTimeSeries()
        {
            Console.WriteLine("Preloading summer.csv...");
            var summer = new SourceDataManager.SourceDataManager();
            summer.LoadData("Data/summer.csv");
            SummerHeatDemand = new List<double>(summer.HeatDemand);
            SummerElectricityPrices = new List<double>(summer.ElectricityPrices);

            Console.WriteLine("Preloading winter.csv...");
            var winter = new SourceDataManager.SourceDataManager();
            winter.LoadData("Data/winter.csv");
            WinterHeatDemand = new List<double>(winter.HeatDemand);
            WinterElectricityPrices = new List<double>(winter.ElectricityPrices);
        }

        public List<ProductionUnit> GetUnits()
        {
            return _assetManager.Units;
        }

        // Deprecated for safety
        [Obsolete("ReloadTimeSeries is now handled at startup to avoid crash.")]
        public void ReloadTimeSeries()
        {
            Console.WriteLine("Reloading time series...");
            _sourceDataManager.LoadData("./Data/TimeSeries_WinterSummer.csv");
        }
    }
}
