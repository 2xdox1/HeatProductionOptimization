using HeatOptimizerApp.Interfaces;
using HeatOptimizerApp.Models;
using HeatOptimizerApp.Modules.AssetManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HeatOptimizerApp.Modules.Optimizer
{
    public class Optimizer : IDataManager
    {
        private List<SimulatedResult> _results = new();
        private List<ProductionUnit> _units = new();
        private List<(DateTime time, double demand)> _heatDemand = new();

        public void LoadData(string path)
        {
            string demandFullPath = Path.GetFullPath(path);
            string demandDir = Path.GetDirectoryName(demandFullPath)!;
            string unitPath = Path.Combine(demandDir, "ProductionUnits.csv");

            Console.WriteLine("📂 Loading units from: " + unitPath);

            if (!File.Exists(unitPath))
                throw new FileNotFoundException("❌ Could not find ProductionUnits.csv next to the heat demand file.", unitPath);

            _units = File.ReadAllLines(unitPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Select(parts => new ProductionUnit
                {
                    Name = parts[0],
                    MaxHeat = double.Parse(parts[1]),
                    ProductionCost = double.Parse(parts[2]),
                    CO2Emission = double.TryParse(parts[3], out var co2) ? co2 : 0,
                    GasConsumption = double.TryParse(parts[4], out var gas) ? gas : null,
                    OilConsumption = double.TryParse(parts[5], out var oil) ? oil : null,
                    MaxElectricity = double.TryParse(parts[6], out var elec) ? elec : null,
                })
                .ToList();

            var lines = File.ReadAllLines(path).Skip(1); // Skip heat demand header
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (DateTime.TryParse(parts[0], out var time) && double.TryParse(parts[1], out var demand))
                {
                    _heatDemand.Add((time, demand));
                }
            }

            Console.WriteLine($"✅ Loaded {_heatDemand.Count} demand values and {_units.Count} units.");
        }

        public void RunOptimization()
        {
            Console.WriteLine("Running basic optimization logic...");

            foreach (var (time, demand) in _heatDemand)
            {
                double remaining = demand;
                foreach (var unit in _units.OrderBy(u => u.ProductionCost))
                {
                    if (remaining <= 0) break;

                    double contribution = Math.Min(unit.MaxHeat, remaining);

                    _results.Add(new SimulatedResult
                    {
                        Time = time,
                        UnitName = unit.Name,
                        HeatProduced = contribution,
                        Cost = contribution * unit.ProductionCost,
                        CO2 = contribution * (unit.CO2Emission ?? 0),
                        ElectricityProduced = unit.MaxElectricity > 0 ? unit.MaxElectricity * (contribution / unit.MaxHeat) : null,
                        ElectricityConsumed = (unit.MaxElectricity.HasValue && unit.MaxElectricity < 0)
                            ? Math.Abs(unit.MaxElectricity.Value) * (contribution / unit.MaxHeat)
                            : null
                    });

                    remaining -= contribution;
                }
            }

            Console.WriteLine("Optimization complete.");
        }

        public void SaveData(string relativePath)
        {
            string fullPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "HeatOptimizerApp", relativePath);
            fullPath = Path.GetFullPath(fullPath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            var lines = new List<string> { "Time,Unit,HeatProduced,Cost,CO2,ElectricityProduced,ElectricityConsumed" };

            foreach (var r in _results)
            {
                lines.Add($"{r.Time},{r.UnitName},{r.HeatProduced},{r.Cost},{r.CO2},{r.ElectricityProduced?.ToString(CultureInfo.InvariantCulture) ?? ""},{r.ElectricityConsumed?.ToString(CultureInfo.InvariantCulture) ?? ""}");
            }

            File.WriteAllLines(fullPath, lines);

            Console.WriteLine($" Saved results to: {fullPath}");
        }

    }
}