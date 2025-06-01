using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.Drawing;   
using SkiaSharp;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HeatOptimizerApp.Utils;
using HeatOptimizerApp.Models;
using HeatOptimizerApp.Modules.Core;
using HeatOptimizerApp.Modules.AssetManager;
using HeatOptimizerApp.Modules.ResultDataManager;
using CommunityToolkit.Mvvm.ComponentModel;


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

    public ISeries[] SimulatedSeries { get; set; } = Array.Empty<ISeries>();
    public Axis[] SimulatedXAxis { get; set; } = Array.Empty<Axis>();
    public Axis[] SimulatedYAxis { get; set; } = Array.Empty<Axis>();

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

    public List<ProductionUnit> CurrentScenarioUnits { get; set; } = new();

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
        // ❗ Prevent LiveCharts from crashing by setting empty axes early
        SimulatedSeries = Array.Empty<ISeries>();
        SimulatedXAxis = new Axis[] { new Axis { Labels = new[] { " " }, Name = "Time" } };
        SimulatedYAxis = new Axis[] { new Axis { Name = "Heat (MWh)" } };
        
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

        // Store full unit objects
        CurrentScenarioUnits = scenarioUnits.ToList();

        // Add names to UI list
        foreach (var unit in CurrentScenarioUnits)
        {
            Units.Add(unit.Name);
        }

        OnPropertyChanged(nameof(Units));

        // 🧠 Trigger summary update
        UpdateSummary(CurrentScenarioUnits);
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

    private SummaryMetrics _summary = new();
    public SummaryMetrics Summary
    {
        get => _summary;
        set
        {
            _summary = value;
            OnPropertyChanged(nameof(Summary));
        }
    }
    public void UpdateSummary(List<ProductionUnit> units)
    {
        Summary = new SummaryMetrics
        {
            TotalMaxHeat = units.Sum(u => u.MaxHeat),
            TotalCost = units.Sum(u => u.ProductionCost),
            TotalCO2 = units.Sum(u => u.CO2Emission ?? 0),
            TotalGas = units.Sum(u => u.GasConsumption ?? 0),
            TotalOil = units.Sum(u => u.OilConsumption ?? 0),
            TotalElectricityCapacity = units.Sum(u => u.MaxElectricity ?? 0),
        };
    }
    public ICommand LoadScenarioCommand => new RelayCommand(ExecuteLoadScenario);
    public ICommand LoadSimulatedChartCommand => new RelayCommand(LoadSimulatedChart);

    private void ExecuteLoadScenario()
    {
        if (!string.IsNullOrEmpty(SelectedScenario))
            LoadScenarioUnits();
    }

    public ScenarioSummary Scenario1Summary { get; set; } = new();
    public ScenarioSummary Scenario2Summary { get; set; } = new();

    public void GenerateScenarioComparisons()
    {
        var units1 = GetUnitsForScenario1();
        Scenario1Summary = CreateSummary("Scenario 1", units1);

        var units2 = GetUnitsForScenario2();
        Scenario2Summary = CreateSummary("Scenario 2", units2);

        OnPropertyChanged(nameof(Scenario1Summary));
        OnPropertyChanged(nameof(Scenario2Summary));
    }

    private ScenarioSummary CreateSummary(string name, IEnumerable<ProductionUnit> units)
    {
        return new ScenarioSummary
        {
            ScenarioName = name,
            TotalMaxHeat = units.Sum(u => u.MaxHeat),
            TotalCost = units.Sum(u => u.ProductionCost),
            TotalCO2 = units.Sum(u => u.CO2Emission ?? 0),
            TotalGas = units.Sum(u => u.GasConsumption ?? 0),
            TotalOil = units.Sum(u => u.OilConsumption ?? 0),
            TotalElectricityCapacity = units.Sum(u => u.MaxElectricity ?? 0)
        };
    }
        public void LoadSimulatedChart()
        {
            try
            {
                string path = "SavedResults/scenario1_simulated.csv";

                if (!File.Exists(path))
                {
                    Console.WriteLine($"❌ File not found: {path}");
                    SetEmptySimulatedChart("❌ Simulated file not found");
                    return;
                }

                var results = SimulatedResultLoader.LoadSimulatedResults(path);
                if (results == null || results.Count == 0)
                {
                    Console.WriteLine("⚠️ Simulated file was empty or failed to load.");
                    SetEmptySimulatedChart("⚠️ No simulated results available");
                    return;
                }

                Console.WriteLine($"✅ Loaded {results.Count} simulated entries.");

                var grouped = results
                    .GroupBy(r => r.Time)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Sum(r => r.HeatProduced))
                    .ToArray();

                var labels = results
                    .GroupBy(r => r.Time)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Key.ToString("HH:mm"))
                    .ToArray();

                if (grouped.Length == 0 || labels.Length == 0 || grouped.Length != labels.Length)
                {
                    Console.WriteLine($"❌ Inconsistent chart data. grouped: {grouped.Length}, labels: {labels.Length}");
                    SetEmptySimulatedChart("❌ Invalid chart data");
                    return;
                }

                SimulatedSeries = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = grouped,
                        Name = "Total Heat Produced",
                        Fill = null,
                        Stroke = new SolidColorPaint(SKColors.OrangeRed, 2)
                    }
                };

                SimulatedXAxis = new Axis[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = 45,
                        Name = "Time"
                    }
                };

                SimulatedYAxis = new Axis[]
                {
                    new Axis
                    {
                        Name = "Heat (MWh)"
                    }
                };

                ChartTitle = "Simulated Heat Production (Scenario 1)";

                OnPropertyChanged(nameof(ChartTitle));
                OnPropertyChanged(nameof(SimulatedSeries));
                OnPropertyChanged(nameof(SimulatedXAxis));
                OnPropertyChanged(nameof(SimulatedYAxis));
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exception in LoadSimulatedChart:\n" + ex);
                SetEmptySimulatedChart("❌ Exception occurred");
            }
        }

        private void SetEmptySimulatedChart(string message)
        {
            SimulatedSeries = Array.Empty<ISeries>();
            SimulatedXAxis = new[] { new Axis { Labels = new[] { "" }, Name = "Time" } };
            SimulatedYAxis = new[] { new Axis { Name = "Heat (MWh)" } };
            ChartTitle = message;

            OnPropertyChanged(nameof(SimulatedSeries));
            OnPropertyChanged(nameof(SimulatedXAxis));
            OnPropertyChanged(nameof(SimulatedYAxis));
            OnPropertyChanged(nameof(ChartTitle));
        }
}