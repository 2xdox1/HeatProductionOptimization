using HeatOptimizerApp.Modules.AssetManager;
using System.Collections.Generic;
using HeatOptimizerApp.Modules.SourceDataManager;
using HeatOptimizerApp.Modules.Optimizer;
using HeatOptimizerApp.Modules.ResultDataManager;
using HeatOptimizerApp.Modules.DataVisualization;
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

        public void RunProject()
        {
            Console.WriteLine("=== Heat Optimization Project ===");

            _assetManager.LoadData("./Data/ProductionUnits.csv");
            _sourceDataManager.LoadData("./Data/TimeSeries_WinterSummer.csv");

            _optimizer.LoadData("");         // Placeholder
            _optimizer.RunOptimization();

            _dataVisualization.LoadData(""); // Placeholder
            _dataVisualization.DisplayChart();
        }

        public List<ProductionUnit> GetUnits()
        {
            return _assetManager.Units;
        }
        public void ReloadTimeSeries()
        {
            Console.WriteLine("Reloading time series...");
            _sourceDataManager.LoadData("./Data/TimeSeries_WinterSummer.csv");
        }
    }
}
