using HeatOptimizerApp.Modules.AssetManager;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HeatOptimizerApp.Modules.ResultDataManager
{
    public class ResultDataManager
    {
        private const string ResultFolder = "SavedResults";

        public void SaveResults(string scenarioName, List<ProductionUnit> units)
        {
            Directory.CreateDirectory(ResultFolder);

            var lines = new List<string>
            {
                "Name,MaxHeat,ProductionCost,CO2Emission,GasConsumption,OilConsumption,MaxElectricity"
            };

            foreach (var u in units)
            {
                lines.Add($"{u.Name},{u.MaxHeat},{u.ProductionCost},{u.CO2Emission},{u.GasConsumption},{u.OilConsumption},{u.MaxElectricity}");
            }

            var totalCost = units.Sum(u => u.ProductionCost);
            var totalCO2 = units.Sum(u => u.CO2Emission ?? 0);
            lines.Add("");
            lines.Add($"TOTAL,,{totalCost},{totalCO2},,,");

            var path = System.IO.Path.Combine(ResultFolder, $"{scenarioName}_saved.csv");
            File.WriteAllLines(path, lines);
        }

        public List<ProductionUnit> LoadResults(string scenarioName)
        {
            var path = System.IO.Path.Combine(ResultFolder, $"{scenarioName}_saved.csv");

            if (!File.Exists(path))
                return new List<ProductionUnit>();

            var lines = File.ReadAllLines(path)
                            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("TOTAL") && !l.StartsWith("Name"))
                            .ToList();

            var units = new List<ProductionUnit>();

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                var unit = new ProductionUnit
                {
                    Name = parts[0],
                    MaxHeat = double.Parse(parts[1], CultureInfo.InvariantCulture),
                    ProductionCost = double.Parse(parts[2], CultureInfo.InvariantCulture),
                    CO2Emission = ParseNullable(parts.ElementAtOrDefault(3)),
                    GasConsumption = ParseNullable(parts.ElementAtOrDefault(4)),
                    OilConsumption = ParseNullable(parts.ElementAtOrDefault(5)),
                    MaxElectricity = ParseNullable(parts.ElementAtOrDefault(6))
                };

                units.Add(unit);
            }

            return units;
        }

        private double? ParseNullable(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                return value;

            return null;
        }
    }
}
