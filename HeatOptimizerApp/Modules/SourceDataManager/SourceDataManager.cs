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

            // If header suggests Danfoss format
            var header = lines[0].ToLower();

            if (header.Contains("time") && (header.Contains("heat demand") || header.Contains("heatdemand")))
            {
                foreach (var line in lines[1..])
                {
                    var parts = line.Split(',');

                    if (parts.Length >= 3)
                    {
                        // Adapt to 3-column Danfoss format
                        double heat = ParseDouble(parts[1]);
                        double price = ParseDouble(parts[2]);

                        HeatDemand.Add(heat);
                        ElectricityPrices.Add(price);
                    }
                }
            }
            else if (lines[0].Split(',').Length >= 4)
            {
                // Older test format with index + 2 extra fields
                foreach (var line in lines[1..])
                {
                    var parts = line.Split(',');

                    if (parts.Length >= 4)
                    {
                        HeatDemand.Add(ParseDouble(parts[2]));
                        ElectricityPrices.Add(ParseDouble(parts[3]));
                    }
                }
            }
            else
            {
                Console.WriteLine($"Unsupported file format: {path}");
            }

            Console.WriteLine($"Loaded {HeatDemand.Count} data points for heat demand.");
            Console.WriteLine($"Loaded {ElectricityPrices.Count} data points for electricity prices.");
        }

        // Generic loader for "Hour,Value" test format
        public List<TimeSeriesPoint> LoadTimeSeries(string csvFilePath)
        {
            var records = new List<TimeSeriesPoint>();

            if (!File.Exists(csvFilePath)) return records;

            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

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
