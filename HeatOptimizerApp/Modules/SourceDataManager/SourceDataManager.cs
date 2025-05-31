using HeatOptimizerApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace HeatOptimizerApp.Modules.SourceDataManager
{
    public class SourceDataManager : IDataManager
    {
        public List<double> HeatDemand { get; private set; } = new();
        public List<double> ElectricityPrices { get; private set; } = new();

        public void LoadData(string path)
        {
            HeatDemand.Clear();
            ElectricityPrices.Clear();

            if (!File.Exists(path)) return;

            var lines = File.ReadAllLines(path);

            foreach (var line in lines[1..]) // Skip header
            {
                var parts = line.Split(',');

                if (parts.Length >= 4)
                {
                    HeatDemand.Add(ParseDouble(parts[2]));          // Correct: Heat Demand
                    ElectricityPrices.Add(ParseDouble(parts[3]));   // Correct: Electricity Price
                }
            }

            Console.WriteLine($"Loaded {HeatDemand.Count} data points for heat demand.");
            Console.WriteLine($"Loaded {ElectricityPrices.Count} data points for electricity prices.");
        }

        public void SaveData(string path)
        {
            // Not implemented yet
        }

        private double ParseDouble(string input)
            => double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0;
    }
}
