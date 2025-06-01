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
    public List<string> HourLabels { get; set; } = Enumerable.Range(0, 24).Select(i => i.ToString()).ToList();

    public List<Axis> XAxes { get; set; } = new();
    public List<Axis> YAxes { get; set; } = new();

    private readonly ProjectController controller;

    public MainWindowViewModel()
    {
        controller = new ProjectController();
        controller.RunProject();

        foreach (var unit in controller.GetUnits())
        {
            Units.Add(unit);
        }

        GenerateMockWinterChart(); // show chart by default
    }

    private void GenerateMockWinterChart()
    {
        var mockData = GenerateMockWinterData();
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
        XAxes.Add(new Axis
        {
            Name = "Hour",
            Labels = HourLabels
        });

        YAxes.Clear();
        YAxes.Add(new Axis
        {
            Name = "Heat Produced (MW)"
        });
    }

    private List<HourlyHeatProduction> GenerateMockWinterData()
    {
        var units = new[] { "GB1", "GM1", "OB1" };
        var data = new List<HourlyHeatProduction>();
        var rand = new Random();

        for (int hour = 0; hour < 24; hour++)
        {
            var total = 10 + rand.NextDouble() * 5;
            var gb1 = 0.5 + rand.NextDouble() * 0.2;
            var gm1 = 0.3 + rand.NextDouble() * 0.1;
            var ob1 = 1.0 - gb1 - gm1;

            data.Add(new HourlyHeatProduction { Hour = hour, UnitName = "GB1", HeatProduced = Math.Round(total * gb1, 2) });
            data.Add(new HourlyHeatProduction { Hour = hour, UnitName = "GM1", HeatProduced = Math.Round(total * gm1, 2) });
            data.Add(new HourlyHeatProduction { Hour = hour, UnitName = "OB1", HeatProduced = Math.Round(total * ob1, 2) });
        }

        return data;
    }
}