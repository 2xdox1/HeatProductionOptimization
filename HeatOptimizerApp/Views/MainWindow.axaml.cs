using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HeatOptimizerApp.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HeatOptimizerApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void LoadScenario1(object? sender, RoutedEventArgs? e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectedScenario = "Scenario1";
        }
    }

    private void LoadScenario2(object? sender, RoutedEventArgs? e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectedScenario = "Scenario2";
        }
    }

    private void CompareScenarios(object? sender, RoutedEventArgs? e)
    {
        (DataContext as MainWindowViewModel)?.GenerateScenarioComparisonChart();
    }

    private void SaveScenarioResults(object? sender, RoutedEventArgs? e)
    {
        // Optional: Hook into save functionality
    }

    private void OnOpenSavedResult(object? sender, RoutedEventArgs? e)
    {
        // Optional: Hook into file open functionality
    }

    private void ReloadTimeSeries(object? sender, RoutedEventArgs? e)
    {
        // Optional: Hook into time series reload logic
    }

    private void ReloadScenario(object? sender, RoutedEventArgs? e)
    {
        // Optional: Hook into list filter + sorting
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Optional: Handle list selection change
    }

    private async void ExportEvaluationSummary(object? sender, RoutedEventArgs? e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        var text = vm.ChartTitle + "\n\n(This is a placeholder. Customize output if needed.)";

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Evaluation Summary",
            SuggestedFileName = "evaluation_summary.txt",
            FileTypeChoices = new[] { new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt" } } }
        });

        if (file != null)
        {
            await using var stream = await file.OpenWriteAsync();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(text);
        }
    }

    private void ExportComparisonCsv(object? sender, RoutedEventArgs? e)
    {
        // Optional: Implement CSV export for comparison chart
    }
}