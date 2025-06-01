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
using HeatOptimizerApp.Modules.Data;

namespace HeatOptimizerApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ProductionUnit> units = new();

    [ObservableProperty]
    private ObservableCollection<ISeries> chartSeries = new();

    [ObservableProperty]
    private List<Axis> xAxes = new();

    [ObservableProperty]
    private List<Axis> yAxes = new();

    [ObservableProperty]
    private string chartTitle = "";

    [ObservableProperty]
    private bool isWinter = true;

    [ObservableProperty]
    private bool isHourly = true;
    [ObservableProperty]
    private string selectedScenario = "Scenario1";
    partial void OnSelectedScenarioChanged(string value)
    {
        UpdateMainChart();
    }

    public ObservableCollection<ISeries> ScenarioComparisonSeries { get; set; } = new();

    public List<Axis> ComparisonXAxes { get; set; } = new();
    public List<Axis> ComparisonYAxes { get; set; } = new();
    public List<string> HourLabels { get; set; } = Enumerable.Range(0, 24).Select(i => i.ToString()).ToList();

    private readonly ProjectController controller;

    public MainWindowViewModel()
    {
        controller = new ProjectController();
        controller.RunProject();

        foreach (var unit in controller.GetUnits())
            Units.Add(unit);

        GenerateScenarioComparisonChart();
        UpdateMainChart(); // Initial chart load
    }

    partial void OnIsWinterChanged(bool value)
    {
        if (value) IsSummer = false;
        UpdateMainChart();
    }

    partial void OnIsHourlyChanged(bool value)
    {
        if (value) IsDaily = false;
        UpdateMainChart();
    }

    [ObservableProperty]
    private bool isSummer;

    [ObservableProperty]
    private bool isDaily;

    partial void OnIsSummerChanged(bool value)
    {
        if (value) IsWinter = false;
        UpdateMainChart();
    }

    partial void OnIsDailyChanged(bool value)
    {
        if (value) IsHourly = false;
        UpdateMainChart();
    }
        private void UpdateMainChart()
        {
        ChartSeries = new ObservableCollection<ISeries>(
            ChartDataLoader.GetSeries(
                IsWinter ? "Winter" : "Summer",
                IsHourly ? "Hourly" : "Daily",
                SelectedScenario
            )
        );

        ChartTitle = $"{SelectedScenario}: {(IsWinter ? "Winter" : "Summer")} — {(IsHourly ? "Hourly" : "Daily")} Heat Production";

        XAxes = new()
        {
            new Axis
            {
                Name = IsHourly ? "Hour" : "Day",
                Labels = IsHourly
                    ? Enumerable.Range(0, 24).Select(i => i.ToString()).ToList()
                    : Enumerable.Range(1, 14).Select(i => $"Day {i}").ToList()
            }
        };

        YAxes = new()
        {
            new Axis
            {
                Name = "Heat Produced (MW)"
            }
        };
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

        ComparisonXAxes = new()
        {
            new Axis { Name = "Scenario", Labels = new List<string> { "S1", "S2" } }
        };

        ComparisonYAxes = new()
        {
            new Axis { Name = "Value (per MWh)" }
        };
    }

    private List<HourlyHeatProduction> GenerateMockData(string season, string scale)
    {
        var units = season == "Winter"
            ? new[] { "GB1", "GM1", "OB1" }
            : new[] { "GB2", "HP1", "OB1" };

        var data = new List<HourlyHeatProduction>();
        var rand = new Random(season == "Summer" ? 2 : 1);

        int points = scale == "Hourly" ? 24 : 7;
        for (int hour = 0; hour < points; hour++)
        {
            double total = (season == "Summer" ? 5 : 10) + rand.NextDouble() * 3;
            double u1 = 0.4 + rand.NextDouble() * 0.2;
            double u2 = 0.3 + rand.NextDouble() * 0.1;
            double u3 = 1.0 - u1 - u2;

            data.Add(new HourlyHeatProduction { Hour = hour, UnitName = units[0], HeatProduced = Math.Round(total * u1, 2) });
            data.Add(new HourlyHeatProduction { Hour = hour, UnitName = units[1], HeatProduced = Math.Round(total * u2, 2) });
            data.Add(new HourlyHeatProduction { Hour = hour, UnitName = units[2], HeatProduced = Math.Round(total * u3, 2) });
        }

        return data;
    }
}