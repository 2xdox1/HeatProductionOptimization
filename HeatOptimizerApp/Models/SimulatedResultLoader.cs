using HeatOptimizerApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HeatOptimizerApp.Modules.ResultDataManager
{
    public static class SimulatedResultLoader
    {
        public static List<SimulatedResult> LoadSimulatedResults(string filePath)
        {
            var results = new List<SimulatedResult>();

            var lines = File.ReadAllLines(filePath).Skip(1); // skip header
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (DateTime.TryParse(parts[0], out var time) &&
                    !string.IsNullOrWhiteSpace(parts[1]) &&
                    double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var heat) &&
                    double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var cost) &&
                    double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var co2))
                {
                    results.Add(new SimulatedResult
                    {
                        Time = time,
                        UnitName = parts[1],
                        HeatProduced = heat,
                        Cost = cost,
                        CO2 = co2,
                        ElectricityProduced = double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var ep) ? ep : null,
                        ElectricityConsumed = double.TryParse(parts[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var ec) ? ec : null
                    });
                }
            }

            return results;
        }
    }
}