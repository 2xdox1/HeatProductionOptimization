using CommunityToolkit.Mvvm.ComponentModel;
using HeatOptimizerApp.Models;
using LiveChartsCore;
using LiveChartsCore.Measure;               // For Axis
using LiveChartsCore.Drawing;               // For SolidColorPaint
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;                            // For SKColors
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HeatOptimizerApp.Modules.AssetManager;
using HeatOptimizerApp.Modules.Core;

namespace HeatOptimizerApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ProjectController controller = new ProjectController();

    // Properties for toggles
    [ObservableProperty]
    private bool isWinter = true;

    [ObservableProperty]
    private bool isSummer;

    [ObservableProperty]
    private bool isHourly = true;

    [ObservableProperty]
    private bool isDaily;

    // Scenario selection (e.g., "Scenario1", "Scenario2")
    [ObservableProperty]
    private string selectedScenario = "Scenario1";

    [ObservableProperty]
    private string selectedSortOption = "Name";

    // Collection of production units (strings of names)
    [ObservableProperty]
    private ObservableCollection<string> units = new();

    // Selected unit details string
    [ObservableProperty]
    private string selectedUnitDetails = "Select a unit to see details...";

    // Chart data
    [ObservableProperty]
    private ObservableCollection<ISeries> chartSeries = new();

    [ObservableProperty]
    private List<Axis> xAxes = new();

    [ObservableProperty]
    private List<Axis> yAxes = new();

    [ObservableProperty]
    private string chartTitle = "";

    [ObservableProperty]
    private string? selectedUnitName;

    [ObservableProperty]
    private bool filterElectricOnly = false;


    // Heat demand data loaded from CSV for winter
    private List<TimeSeriesPoint> winterHeatDemandData = new();
    public List<TimeSeriesPoint> WinterHeatDemandData
    {
        get => winterHeatDemandData;
        set
        {
            winterHeatDemandData = value ?? new List<TimeSeriesPoint>();
            WinterDailyHeatDemandData = AggregateDaily(winterHeatDemandData);
            OnPropertyChanged(nameof(WinterHeatDemandData));
            UpdateChart();
        }
    }

    // Heat demand data loaded from CSV for summer
    private List<TimeSeriesPoint> summerHeatDemandData = new();
    public List<TimeSeriesPoint> SummerHeatDemandData
    {
        get => summerHeatDemandData;
        set
        {
            summerHeatDemandData = value ?? new List<TimeSeriesPoint>();
            SummerDailyHeatDemandData = AggregateDaily(summerHeatDemandData);
            OnPropertyChanged(nameof(SummerHeatDemandData));
            UpdateChart();
        }
    }

    // Daily aggregated heat demand for winter
    private List<TimeSeriesPoint> winterDailyHeatDemandData = new();
    public List<TimeSeriesPoint> WinterDailyHeatDemandData
    {
        get => winterDailyHeatDemandData;
        set
        {
            winterDailyHeatDemandData = value;
            OnPropertyChanged(nameof(WinterDailyHeatDemandData));
            UpdateChart();
        }
    }

    // Daily aggregated heat demand for summer
    private List<TimeSeriesPoint> summerDailyHeatDemandData = new();
    public List<TimeSeriesPoint> SummerDailyHeatDemandData
    {
        get => summerDailyHeatDemandData;
        set
        {
            summerDailyHeatDemandData = value;
            OnPropertyChanged(nameof(SummerDailyHeatDemandData));
            UpdateChart();
        }
    }

    public MainWindowViewModel()
    {
        // Initialize controller
        controller.RunProject();

        // Initialize toggles mutually exclusive
        IsSummer = !IsWinter;
        IsDaily = !IsHourly;

        LoadScenarioUnits(); // Load initial scenario units

        UpdateChart();
    }

    // Ensure mutual exclusivity for toggles and update chart accordingly
    partial void OnIsWinterChanged(bool value)
    {
        if (value) IsSummer = false;
        UpdateChart();
    }

    partial void OnIsSummerChanged(bool value)
    {
        if (value) IsWinter = false;
        UpdateChart();
    }

    partial void OnIsHourlyChanged(bool value)
    {
        if (value) IsDaily = false;
        UpdateChart();
    }

    partial void OnIsDailyChanged(bool value)
    {
        if (value) IsHourly = false;
        UpdateChart();
    }

    // Update the main chart using loaded winter or summer heat demand data or fallback mock data
    public void UpdateChart()
    {
        IEnumerable<double> values;

        if (IsWinter)
        {
            values = IsHourly && WinterHeatDemandData != null && WinterHeatDemandData.Count > 0
                ? WinterHeatDemandData.OrderBy(p => p.Hour).Select(p => p.Value)
                : WinterDailyHeatDemandData.OrderBy(p => p.Hour).Select(p => p.Value);
        }
        else
        {
            values = IsHourly && SummerHeatDemandData != null && SummerHeatDemandData.Count > 0
                ? SummerHeatDemandData.OrderBy(p => p.Hour).Select(p => p.Value)
                : SummerDailyHeatDemandData.OrderBy(p => p.Hour).Select(p => p.Value);
        }

        ChartSeries.Clear();

        int barWidth = IsHourly ? 15 : 30;

        ChartSeries.Add(new ColumnSeries<double>
        {
            Name = $"{SelectedScenario} - {(IsWinter ? "Winter" : "Summer")} - {(IsHourly ? "Hourly" : "Daily")}",
            Values = values.ToArray(),
            MaxBarWidth = barWidth
        });

        XAxes = new List<Axis>
        {
            new Axis
            {
                Name = IsHourly ? "Hour" : "Day",
                Labels = IsHourly
                    ? Enumerable.Range(0, 24).Select(i => i.ToString()).ToList()
                    : Enumerable.Range(1, (IsWinter ? WinterDailyHeatDemandData.Count : SummerDailyHeatDemandData.Count))
                        .Select(i => $"Day {i}").ToList()
            }
        };

        // Determine current data set for Y axis
        List<TimeSeriesPoint> currentData;
        if (IsWinter)
        {
            currentData = IsHourly ? WinterHeatDemandData ?? new List<TimeSeriesPoint>() : WinterDailyHeatDemandData ?? new List<TimeSeriesPoint>();
        }
        else
        {
            currentData = IsHourly ? SummerHeatDemandData ?? new List<TimeSeriesPoint>() : SummerDailyHeatDemandData ?? new List<TimeSeriesPoint>();
        }

        // Calculate maxLimit safely (fallback to 50 if null or empty)
        double maxLimit = (currentData != null && currentData.Count > 0)
            ? CalculateMaxLimit(currentData)
            : 50;

        YAxes = new List<Axis>
        {
            new Axis
            {
                Name = "Heat Produced (MW)",
                MinLimit = 0,
                MaxLimit = maxLimit * 1.1,
                MinStep = Math.Max(1, maxLimit / 10),
                ForceStepToMin = true,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 }
            }
        };

        ChartTitle = $"{SelectedScenario}: {(IsWinter ? "Winter" : "Summer")} — {(IsHourly ? "Hourly" : "Daily")} Heat Production";
        OnPropertyChanged(nameof(ChartTitle));
    }

    // Helper method to calculate a nice rounded max limit for the Y axis
    private double CalculateMaxLimit(List<TimeSeriesPoint> data)
    {
        if (data == null || data.Count == 0)
            return 50; // Default max if no data

        var maxVal = data.Max(p => p.Value);

        // Round up to nearest multiple of 5
        return Math.Ceiling(maxVal / 5) * 5;
    }

    // Helper method to aggregate hourly data into daily sums
    private List<TimeSeriesPoint> AggregateDaily(List<TimeSeriesPoint> hourlyData)
    {
        if (hourlyData == null || hourlyData.Count == 0)
            return new List<TimeSeriesPoint>();

        int days = hourlyData.Count / 24;
        var dailyData = new List<TimeSeriesPoint>();

        for (int day = 0; day < days; day++)
        {
            double dailySum = hourlyData
                .Skip(day * 24)
                .Take(24)
                .Sum(p => p.Value);

            dailyData.Add(new TimeSeriesPoint { Hour = day + 1, Value = dailySum });
        }

        return dailyData;
    }

 public void LoadScenarioUnits()
{
    Units.Clear();

    IEnumerable<ProductionUnit> scenarioUnits = SelectedScenario switch
    {
        "Scenario1" => GetUnitsForScenario1(),
        "Scenario2" => GetUnitsForScenario2(),
        _ => Enumerable.Empty<ProductionUnit>()
    };

    // Apply electricity filter if enabled
    if (FilterElectricOnly)
    {
        scenarioUnits = scenarioUnits.Where(u => u.MaxElectricity.HasValue && u.MaxElectricity.Value > 0);
    }

    // Apply sorting
    scenarioUnits = SelectedSortOption switch
    {
        "Production Cost" => scenarioUnits.OrderBy(u => u.ProductionCost),
        "CO₂ Emission" => scenarioUnits.OrderBy(u => u.CO2Emission ?? 0),
        _ => scenarioUnits.OrderBy(u => u.Name), // Default sort by Name
    };

    Units.Clear(); // Clear existing items before adding new ones

    foreach (var unit in scenarioUnits)
    {
        Units.Add(unit.Name);
    }

    OnPropertyChanged(nameof(Units));
}

    private IEnumerable<ProductionUnit> GetUnitsForScenario1()
    {
        return controller.GetUnits().Where(u => u.Name is "GB1" or "GB2" or "OB1");
    }

    private IEnumerable<ProductionUnit> GetUnitsForScenario2()
    {
        return controller.GetUnits().Where(u => u.Name is "GB1" or "OB1" or "GM1" or "HP1");
    }

    partial void OnSelectedUnitNameChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            SelectedUnitDetails = "Select a unit to see details...";
            return;
        }

        // Find the ProductionUnit with the matching name
        var unit = controller.GetUnits().FirstOrDefault(u => u.Name == value);

        if (unit != null)
        {
            SelectedUnitDetails =
                $"Name: {unit.Name}\n" +
                $"Max Heat: {unit.MaxHeat} MW\n" +
                $"Production Cost: {unit.ProductionCost} DKK/MWh\n" +
                $"CO₂ Emission: {unit.CO2Emission ?? 0} kg/MWh\n" +
                $"Gas Consumption: {unit.GasConsumption?.ToString("0.0") ?? "–"} MWh\n" +
                $"Oil Consumption: {unit.OilConsumption?.ToString("0.0") ?? "–"} MWh\n" +
                $"Max Electricity: {unit.MaxElectricity?.ToString("0.0") ?? "–"} MW";
        }
        else
        {
            SelectedUnitDetails = "Details not found.";
        }
    }
    partial void OnFilterElectricOnlyChanged(bool value)
    {
        LoadScenarioUnits();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        LoadScenarioUnits();
    }
}