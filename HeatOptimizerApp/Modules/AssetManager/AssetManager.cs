using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HeatOptimizerApp.Modules.AssetManager
{
    public class AssetManager : Interfaces.IDataManager
    {
        public List<ProductionUnit> Units { get; private set; } = new List<ProductionUnit>();

        public void LoadData(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"File not found: {path}");
                return;
            }

            var lines = File.ReadAllLines(path);
            Console.WriteLine($"Found {lines.Length} lines in file: {path}");

            foreach (var line in lines)
            {
                Console.WriteLine($"Line: '{line}'"); // Show raw content

                var parts = line.Split(',');

                if (parts.Length < 4)
                {
                    Console.WriteLine("SKIPPED: not enough columns");
                    continue;
                }

                try
                {
                    var unit = new ProductionUnit
                    {
                        Name = parts[0],
                        MaxHeat = double.Parse(parts[1], CultureInfo.InvariantCulture),
                        ProductionCost = double.Parse(parts[2], CultureInfo.InvariantCulture),
                        CO2Emission = ParseDouble(parts.ElementAtOrDefault(3)),
                        GasConsumption = ParseDouble(parts.ElementAtOrDefault(4)),
                        OilConsumption = ParseDouble(parts.ElementAtOrDefault(5)),
                        MaxElectricity = ParseDouble(parts.ElementAtOrDefault(6))
                    };

                    Units.Add(unit);
                    Console.WriteLine($"ADDED: {unit.Name} - MaxHeat: {unit.MaxHeat}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Could not parse line. {ex.Message}");
                }
            }

            Console.WriteLine("Loaded units summary:");
            foreach (var unit in Units)
                Console.WriteLine($"{unit.Name} - Max Heat: {unit.MaxHeat} MW");
        }

        public void SaveData(string path)
        {
            // Not needed for initial version
        }

        private double? ParseDouble(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                return value;

            return null;
        }
    }
}
