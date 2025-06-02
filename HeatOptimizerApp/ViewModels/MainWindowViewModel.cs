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

    private Dictionary<string, List<SimulatedResult>> ScenarioData { get; set; } = new();

    public ICommand SaveScenarioResultsCommand { get; }
    public ICommand LoadSavedResultCommand { get; }
    public ICommand ReloadTimeSeriesCommand { get; }
    public ICommand LoadWinterDataCommand { get; }
    public ICommand LoadSummerDataCommand { get; }


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
        // Prevent LiveCharts from crashing by setting empty axes early
        SimulatedSeries = Array.Empty<ISeries>();
        SimulatedXAxis = new Axis[] { new Axis { Labels = new[] { " " }, Name = "Time" } };
        SimulatedYAxis = new Axis[] { new Axis { Name = "Heat (MWh)" } };

        // Initialize controller and preload CSVs
        controller.RunProject();

        // NEW: Assign preloaded data into properties that drive the charts
        WinterHeatDemandData = controller.WinterHeatDemand
            .Select((val, i) => new TimeSeriesPoint { Hour = i, Value = val }).ToList();

        SummerHeatDemandData = controller.SummerHeatDemand
            .Select((val, i) => new TimeSeriesPoint { Hour = i, Value = val }).ToList();

        // Initialize toggles mutually exclusive
        IsSummer = !IsWinter;
        IsDaily = !IsHourly;

        LoadScenarioUnits(); // Load initial scenario units
        UpdateChart();

        SaveScenarioResultsCommand = new RelayCommand(() => Console.WriteLine("Save scenario results clicked"));
        LoadSavedResultCommand = new RelayCommand(() => Console.WriteLine("Load saved result clicked"));
        ReloadTimeSeriesCommand = new RelayCommand(() => Console.WriteLine("Reload time series clicked"));
        LoadWinterDataCommand = new RelayCommand(() => Console.WriteLine("Load winter data clicked"));
        LoadSummerDataCommand = new RelayCommand(() => Console.WriteLine("Load summer data clicked"));
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
                ? AverageHourly(WinterHeatDemandData).OrderBy(p => p.Hour).Select(p => p.Value)
                : WinterDailyHeatDemandData.OrderBy(p => p.Hour).Select(p => p.Value);
        }
        else
        {
            values = IsHourly && SummerHeatDemandData != null && SummerHeatDemandData.Count > 0
                ? AverageHourly(SummerHeatDemandData).OrderBy(p => p.Hour).Select(p => p.Value)
                : SummerDailyHeatDemandData.OrderBy(p => p.Hour).Select(p => p.Value);
        }

        ChartSeries.Clear();

        int barWidth = IsHourly ? 15 : 30;

        ChartSeries.Add(new ColumnSeries<double>
        {
            Name = $"{SelectedScenario} - {(IsWinter ? "Winter" : "Summer")} - {(IsHourly ? "Hourly" : "Daily")}",
            Values = values.ToArray(),
            MaxBarWidth = barWidth,
            Padding = 0
        });

        var labelCount = values.Count();

        XAxes = new List<Axis>
        {
            new Axis
            {
                Name = IsHourly ? "Hour" : "Day",
                Labels = IsHourly
                    ? Enumerable.Range(0, 24).Select(i => i.ToString()).ToList()
                    : Enumerable.Range(1, labelCount).Select(i => i.ToString()).ToList(),
                MinLimit = -0.5,
                MaxLimit = labelCount - 0.5,
                MinStep = 1,
                LabelsRotation = 0,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 }
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

        double maxVal = (currentData != null && currentData.Count > 0)
            ? currentData.Max(p => p.Value)
            : 50;

        double maxLimit = GetNiceAxisLimit(maxVal);

        YAxes = new List<Axis>
        {
            new Axis
            {
                Name = "Heat Produced (MW)",
                MinLimit = 0,
                MaxLimit = maxLimit,
                MinStep = Math.Ceiling(maxLimit / 6),
                ForceStepToMin = true,
                LabelsPaint = new SolidColorPaint(SKColors.Black, 12),
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 }
            }
        };

        ChartTitle = $"{SelectedScenario}: {(IsWinter ? "Winter" : "Summer")} — {(IsHourly ? "Hourly" : "Daily")} Heat Production";
        OnPropertyChanged(nameof(ChartTitle));
    }

    private double GetNiceAxisLimit(double maxValue)
    {
        if (maxValue < 1)
            return 1;

        if (maxValue < 5)
            return Math.Ceiling(maxValue * 1.2 * 2) / 2; // round to nearest 0.5

        if (maxValue < 20)
            return Math.Ceiling(maxValue * 1.1); // round to next integer

        return Math.Ceiling(maxValue / 5.0) * 5 + 5;
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

    private List<TimeSeriesPoint> AverageHourly(List<TimeSeriesPoint> fullData)
    {
        var hourlyAverages = new List<TimeSeriesPoint>();

        for (int h = 0; h < 24; h++)
        {
            var valuesAtHour = fullData
                .Where(p => p.Hour % 24 == h)
                .Select(p => p.Value)
                .ToList();

            if (valuesAtHour.Any())
            {
                double avg = valuesAtHour.Average();
                hourlyAverages.Add(new TimeSeriesPoint { Hour = h, Value = avg });
            }
        }

        return hourlyAverages;
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
        var path = "SavedResults/scenario2_simulated.csv"; // Adjust if needed
        if (!File.Exists(path))
        {
            Console.WriteLine($"❌ File not found: {path}");
            return;
        }

        var results = SimulatedResultLoader.LoadSimulatedResults(path);

        var grouped = results
            .GroupBy(r => r.Time)
            .OrderBy(g => g.Key)
            .Select(g => g.Sum(r => r.HeatProduced))
            .ToArray();

        var groupedTimestamps = results
            .GroupBy(r => r.Time)
            .OrderBy(g => g.Key)
            .Select(g => g.Key)
            .ToList();

        SimulatedSeries = new ISeries[]
        {
                new LineSeries<double>
                {
                    Values = grouped,
                    Name = "Total Heat Produced",
                    GeometrySize = 4,
                    Stroke = new SolidColorPaint(SKColors.OrangeRed, 2),
                    Fill = null
                }
        };

        SimulatedXAxis = new Axis[]
        {
                new Axis
                {
                    Name = "Time",
                    LabelsRotation = 45,
                    Labeler = value =>
                    {
                        int index = (int)Math.Round(value);
                        if (index >= 0 && index < groupedTimestamps.Count)
                            return groupedTimestamps[index].ToString("HH:mm");
                        return "";
                    },
                    MinStep = 1,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                }
        };

        SimulatedYAxis = new Axis[]
        {
                new Axis
                {
                    Name = "Heat (MWh)",
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                }
        };

        OnPropertyChanged(nameof(SimulatedSeries));
        OnPropertyChanged(nameof(SimulatedXAxis));
        OnPropertyChanged(nameof(SimulatedYAxis));
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
    
    public void SaveScenarioToCsv()
    {
        var scenario = SelectedScenario;
            if (string.IsNullOrEmpty(scenario) || !ScenarioData.TryGetValue(scenario, out var data))
            {
                Console.WriteLine("❌ No scenario data to save.");
                return;
            }

            var path = $"SavedResults/{scenario}_saved.csv";
            var lines = new List<string> { "Time,Unit,HeatProduced,Cost,CO2,ElectricityProduced,ElectricityConsumed" };
            foreach (var r in data)
            {
                lines.Add($"{r.Time},{r.UnitName},{r.HeatProduced},{r.Cost},{r.CO2},{r.ElectricityProduced?.ToString() ?? ""},{r.ElectricityConsumed?.ToString() ?? ""}");
            }

            File.WriteAllLines(path, lines);
            Console.WriteLine($" Scenario saved to {path}");
    }

    public void LoadScenarioFromCsv()
    {
        var scenario = SelectedScenario;
        var path = $"SavedResults/{scenario}_saved.csv";

        if (!File.Exists(path))
        {
            Console.WriteLine($"❌ File not found: {path}");
            return;
        }

       var results = SimulatedResultLoader.LoadSimulatedResults(path);
        ScenarioData[scenario] = results;
        Units = new ObservableCollection<string>(
        results.Select(r => r.UnitName).Distinct());

        OnPropertyChanged(nameof(Units));
        UpdateChart();

        Console.WriteLine($"✅ Loaded scenario data from {path}");
    }
    public void ReloadTimeSeries()
    {
        Console.WriteLine(" Reloading chart from current loaded data...");
        UpdateChart();
    }

}