using Avalonia.Controls;
using Avalonia.Interactivity;

namespace HeatOptimizerApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SaveScenarioResults(object? sender, RoutedEventArgs? e) { }
    private void OnOpenSavedResult(object? sender, RoutedEventArgs? e) { }
    private void ReloadTimeSeries(object? sender, RoutedEventArgs? e) { }
    private void LoadScenario1(object? sender, RoutedEventArgs? e) { }
    private void LoadScenario2(object? sender, RoutedEventArgs? e) { }
    private void ExportEvaluationSummary(object? sender, RoutedEventArgs? e) { }
    private void ExportComparisonCsv(object? sender, RoutedEventArgs? e) { }
}