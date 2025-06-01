using CommunityToolkit.Mvvm.ComponentModel;
using HeatOptimizerApp.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HeatOptimizerApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
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

    // Collection of production units (placeholder, populate as needed)
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

    // Heat demand data loaded from CSV for winter
    private List<TimeSeriesPoint> winterHeatDemandData = new();
    public List<TimeSeriesPoint> WinterHeatDemandData
    {
        get => winterHeatDemandData;
        set
        {
            winterHeatDemandData = value;
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
            summerHeatDemandData = value;
            OnPropertyChanged(nameof(SummerHeatDemandData));
            UpdateChart();
        }
    }

    public MainWindowViewModel()
    {
        // Initialize toggles mutually exclusive
        IsSummer = !IsWinter;
        IsDaily = !IsHourly;

        // Initialize axes and chart series with default/mock data
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

    partial void OnSelectedScenarioChanged(string value)
    {
        UpdateChart();
    }

    // Update the main chart using loaded winter or summer heat demand data or fallback mock data
    public void UpdateChart()
    {
        IEnumerable<double> values;

        if (IsWinter && WinterHeatDemandData != null && WinterHeatDemandData.Count > 0)
        {
            values = WinterHeatDemandData.OrderBy(p => p.Hour).Select(p => p.Value);
        }
        else if (!IsWinter && SummerHeatDemandData != null && SummerHeatDemandData.Count > 0)
        {
            values = SummerHeatDemandData.OrderBy(p => p.Hour).Select(p => p.Value);
        }
        else
        {
            // Fallback mock data (simple sine wave or fixed data)
            values = Enumerable.Range(0, 24).Select(i => 10 + 5 * Math.Sin(i / 24.0 * 2 * Math.PI));
        }

        ChartSeries.Clear();

        ChartSeries.Add(new ColumnSeries<double>
        {
            Name = $"{SelectedScenario} - {(IsWinter ? "Winter" : "Summer")} - {(IsHourly ? "Hourly" : "Daily")}",
            Values = values.ToArray(),
            MaxBarWidth = 20
        });

        XAxes = new List<Axis>
        {
            new Axis
            {
                Name = IsHourly ? "Hour" : "Day",
                Labels = IsHourly
                    ? Enumerable.Range(0, 24).Select(i => i.ToString()).ToList()
                    : Enumerable.Range(1, 14).Select(i => $"Day {i}").ToList()
            }
        };

        YAxes = new List<Axis>
        {
            new Axis
            {
                Name = "Heat Produced (MW)",
                MinLimit = 0,
                MaxLimit = CalculateMaxLimit(IsWinter ? WinterHeatDemandData : SummerHeatDemandData),
                MinStep = 5 // grid line spacing for denser grid
            }
        };

        ChartTitle = $"{SelectedScenario}: {(IsWinter ? "Winter" : "Summer")} — {(IsHourly ? "Hourly" : "Daily")} Heat Production";
        OnPropertyChanged(nameof(ChartTitle));  // Notify UI immediately about title change
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
}