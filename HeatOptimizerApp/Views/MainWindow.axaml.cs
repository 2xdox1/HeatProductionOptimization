using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HeatOptimizerApp.Modules.SourceDataManager;
using HeatOptimizerApp.ViewModels;
using System.Threading.Tasks;

namespace HeatOptimizerApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly SourceDataManager sourceDataManager = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadScenario1(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SelectedScenario = "Scenario1";
                ReloadScenario();
            }
        }

        private void LoadScenario2(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SelectedScenario = "Scenario2";
                ReloadScenario();
            }
        }

        private void ReloadScenario()
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.LoadScenarioUnits(); // Refresh units based on selected scenario
            }
        }

        private void CompareScenarios(object? sender, RoutedEventArgs e)
        {
            // TODO: Implement scenario comparison logic here
        }

        private void SaveScenarioResults(object? sender, RoutedEventArgs e)
        {
            // TODO: Implement save scenario logic here
        }

        private void OnOpenSavedResult(object? sender, RoutedEventArgs e)
        {
            // TODO: Implement load saved result logic here
        }

        private void ReloadTimeSeries(object? sender, RoutedEventArgs e)
        {
            // TODO: Implement reload time series logic here
        }

        private void ExportEvaluationSummary(object? sender, RoutedEventArgs e)
        {
            // TODO: Implement export evaluation summary here
        }

        private void ExportComparisonCsv(object? sender, RoutedEventArgs e)
        {
            // TODO: Implement export comparison CSV here
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
                var heatDemandData = await Task.Run(() => sourceDataManager.LoadTimeSeries(files[0].Path.LocalPath));
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
                var heatDemandData = await Task.Run(() => sourceDataManager.LoadTimeSeries(files[0].Path.LocalPath));
                if (DataContext is MainWindowViewModel vm)
                {
                    vm.SummerHeatDemandData = heatDemandData;
                    vm.UpdateChart();
                }
            }
        }
    }
}