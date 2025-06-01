using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HeatOptimizerApp.Modules.SourceDataManager;
using HeatOptimizerApp.ViewModels;

namespace HeatOptimizerApp.Views;

public partial class MainWindow : Window
{
    private readonly SourceDataManager sourceDataManager = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void LoadScenario1(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement scenario 1 loading logic
    }

    private void LoadScenario2(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement scenario 2 loading logic
    }

    private void CompareScenarios(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement scenario comparison logic
    }

    private void SaveScenarioResults(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement save scenario logic
    }

    private void OnOpenSavedResult(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement load saved result logic
    }

    private void ReloadTimeSeries(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement reload time series logic
    }

    private void ExportEvaluationSummary(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement export evaluation summary
    }

    private void ExportComparisonCsv(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement export comparison CSV
    }

    private async void LoadWinterData(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Winter Heat Demand CSV",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } }
            }
        });

        if (files.Count > 0)
        {
            var heatDemandData = sourceDataManager.LoadTimeSeries(files[0].Path.LocalPath);
            if (DataContext is MainWindowViewModel vm)
            {
                vm.WinterHeatDemandData = heatDemandData;
                vm.UpdateChart();
            }
        }
    }

    private async void LoadSummerData(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Summer Heat Demand CSV",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } }
            }
        });

        if (files.Count > 0)
        {
            var heatDemandData = sourceDataManager.LoadTimeSeries(files[0].Path.LocalPath);
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SummerHeatDemandData = heatDemandData;
                vm.UpdateChart();
            }
        }
    }
}