using HeatOptimizerApp.Interfaces;
using HeatOptimizerApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

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

        // New method: generic CSV loader for time series data (single column of values)
        public List<TimeSeriesPoint> LoadTimeSeries(string csvFilePath)
        {
            var records = new List<TimeSeriesPoint>();

            if (!File.Exists(csvFilePath)) return records;

            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var hour = csv.GetField<int>("Hour");
                var value = csv.GetField<double>("Value");

                records.Add(new TimeSeriesPoint { Hour = hour, Value = value });
            }

            Console.WriteLine($"Loaded {records.Count} time series points from {csvFilePath}");

            return records;
        }

        public void SaveData(string path)
        {
            // Not implemented yet
        }

        private double ParseDouble(string input)
            => double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0;
    }
}