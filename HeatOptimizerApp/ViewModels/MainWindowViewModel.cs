using CommunityToolkit.Mvvm.ComponentModel;
using HeatOptimizerApp.Models;
using HeatOptimizerApp.Modules.AssetManager;
using HeatOptimizerApp.Modules.Core;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HeatOptimizerApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<ProductionUnit> units = new();

    public ObservableCollection<ISeries> WinterSeries { get; set; } = new();
    public ObservableCollection<ISeries> SummerSeries { get; set; } = new();
    public ObservableCollection<ISeries> ScenarioComparisonSeries { get; set; } = new();

    public List<string> HourLabels { get; set; } = Enumerable.Range(0, 24).Select(i => i.ToString()).ToList();
    public List<string> ComparisonLabels { get; set; } = new() { "S1", "S2" };

    public List<Axis> XAxes { get; set; } = new();
    public List<Axis> YAxes { get; set; } = new();
    public List<Axis> XAxesSummer { get; set; } = new();
    public List<Axis> YAxesSummer { get; set; } = new();
    public List<Axis> XAxesComparison { get; set; } = new();
    public List<Axis> YAxesComparison { get; set; } = new();

    private readonly ProjectController controller;

    public MainWindowViewModel()
    {
        controller = new ProjectController();
        controller.RunProject();

        foreach (var unit in controller.GetUnits())
        {
            Units.Add(unit);
        }

        GenerateMockWinterChart();
        GenerateMockSummerChart();
        GenerateScenarioComparisonChart();
    }

    public void GenerateScenarioComparisonChart()
    {
        var allUnits = controller.GetUnits();
        var scenario1 = allUnits.Where(u => u.Name is "GB1" or "GB2" or "OB1").ToList();
        var scenario2 = allUnits.Where(u => u.Name is "GB1" or "OB1" or "GM1" or "HP1").ToList();

        double cost1 = scenario1.Sum(u => u.ProductionCost);
        double co21 = scenario1.Sum(u => u.CO2Emission ?? 0);
        double cost2 = scenario2.Sum(u => u.ProductionCost);
        double co22 = scenario2.Sum(u => u.CO2Emission ?? 0);

        ScenarioComparisonSeries.Clear();

        ScenarioComparisonSeries.Add(new ColumnSeries<double>
        {
            Name = "Cost",
            Values = new[] { cost1, cost2 },
            MaxBarWidth = 25
        });

        ScenarioComparisonSeries.Add(new ColumnSeries<double>
        {
            Name = "CO₂",
            Values = new[] { co21, co22 },
            MaxBarWidth = 25
        });

        XAxesComparison = new()
        {
            new Axis { Name = "Scenario", Labels = ComparisonLabels }
        };

        YAxesComparison = new()
        {
            new Axis { Name = "Value (per MWh)" }
        };
    }

    private void GenerateMockWinterChart()
    {
        var mockData = GenerateMockData("Winter");
        var units = mockData.Select(d => d.UnitName).Distinct();

        WinterSeries.Clear();

        foreach (var unit in units)
        {
            var values = mockData
                .Where(d => d.UnitName == unit)
                .OrderBy(d => d.Hour)
                .Select(d => d.HeatProduced)
                .ToArray();

            WinterSeries.Add(new ColumnSeries<double>
            {
                Name = unit,
                Values = values,
                MaxBarWidth = 18
            });
        }

        XAxes.Clear();
        XAxes.Add(new Axis { Name = "Hour", Labels = HourLabels });

        YAxes.Clear();
        YAxes.Add(new Axis { Name = "Heat Produced (MW)" });
    }

    private void GenerateMockSummerChart()
    {
        var mockData = GenerateMockData("Summer");
        var units = mockData.Select(d => d.UnitName).Distinct();

        SummerSeries.Clear();

        foreach (var unit in units)
        {
            var values = mockData
                .Where(d => d.UnitName == unit)
                .OrderBy(d => d.Hour)
                .Select(d => d.HeatProduced)
                .ToArray();

            SummerSeries.Add(new ColumnSeries<double>
            {
                Name = unit,
                Values = values,
                MaxBarWidth = 18
            });
        }

        XAxesSummer = new()
        {
            new Axis { Name = "Hour", Labels = HourLabels }
        };

        YAxesSummer = new()
        {
            new Axis { Name = "Heat Produced (MW)" }
        };
    }

    private List<HourlyHeatProduction> GenerateMockData(string season)
    {
        var units = new[] { "GB1", "GM1", "OB1" };
        var data = new List<HourlyHeatProduction>();
        var rand = new Random(season == "Summer" ? 2 : 1); // use different seed for different data

        for (int hour = 0; hour < 24; hour++)
        {
            var total = (season == "Summer" ? 5 : 10) + rand.NextDouble() * 3;
            var gb1 = 0.4 + rand.NextDouble() * 0.2;
            var gm1 = 0.3 + rand.NextDouble() * 0.1;
            var ob1 = 1.0 - gb1 - gm1;

            data.Add(new HourlyHeatProduction { Hour = hour, UnitName = "GB1", HeatProduced = Math.Round(total * gb1, 2) });
            data.Add(new HourlyHeatProduction { Hour = hour, UnitName = "GM1", HeatProduced = Math.Round(total * gm1, 2) });
            data.Add(new HourlyHeatProduction { Hour = hour, UnitName = "OB1", HeatProduced = Math.Round(total * ob1, 2) });
        }

        return data;
    }
}