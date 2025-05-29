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
            if (!File.Exists(path)) return;

            var lines = File.ReadAllLines(path);

            foreach (var line in lines[1..]) // Skipping headers
            {
                var parts = line.Split(',');

                if (parts.Length >= 3)
                {
                    HeatDemand.Add(ParseDouble(parts[1]));
                    ElectricityPrices.Add(ParseDouble(parts[2]));
                }
            }

            Console.WriteLine($"Loaded {HeatDemand.Count} data points for heat demand.");
            Console.WriteLine($"Loaded {ElectricityPrices.Count} data points for electricity prices.");
        }

        public void SaveData(string path)
        {
            // Later!
        }

        private double ParseDouble(string input)
            => double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0;
    }
}
