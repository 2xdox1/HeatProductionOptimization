using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using HeatOptimizerApp.Modules.Core;
using HeatOptimizerApp.Modules.ResultDataManager;
using HeatOptimizerApp.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace HeatOptimizerApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly ProjectController controller;
        private readonly ResultDataManager resultManager = new();
        private string currentScenario = "Scenario1";

        public MainWindow()
        {
            InitializeComponent();
            controller = new ProjectController();
            controller.RunProject();
            LoadScenario1(null, null);
        }

        private void LoadScenario1(object? sender, RoutedEventArgs? e)
        {
            currentScenario = "Scenario1";
            ReloadScenario(null, null);
        }

        private void LoadScenario2(object? sender, RoutedEventArgs? e)
        {
            currentScenario = "Scenario2";
            ReloadScenario(null, null);
        }

        private void ReloadScenario(object? sender, RoutedEventArgs? e)
        {
            var allUnits = controller.GetUnits();
            var scenarioUnits = currentScenario switch
            {
                "Scenario1" => allUnits.Where(u => u.Name is "GB1" or "GB2" or "OB1"),
                "Scenario2" => allUnits.Where(u => u.Name is "GB1" or "OB1" or "GM1" or "HP1"),
                _ => Enumerable.Empty<Modules.AssetManager.ProductionUnit>()
            };

            if (ElectricOnlyCheck.IsChecked == true)
                scenarioUnits = scenarioUnits.Where(u => u.MaxElectricity.HasValue && u.MaxElectricity > 0);

            var list = scenarioUnits.ToList();

            var sortBy = (SortCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
            list = sortBy switch
            {
                "Production Cost" => list.OrderBy(u => u.ProductionCost).ToList(),
                "CO₂ Emission" => list.OrderBy(u => u.CO2Emission ?? double.MaxValue).ToList(),
                _ => list.OrderBy(u => u.Name).ToList()
            };

            UnitList.ItemsSource = list
                .Select(u => $"{u.Name} — {u.MaxHeat} MW, {u.ProductionCost} DKK/MWh")
                .ToList();

            var totalCost = list.Sum(u => u.ProductionCost);
            var totalCO2 = list.Sum(u => u.CO2Emission ?? 0);

            ScenarioSummaryBlock.Text = $"Scenario total: {totalCost} DKK/MWh — {totalCO2} kg CO₂/MWh";
            DetailsBlock.Text = $"{currentScenario} loaded.";
            DrawChart(list);
        }

        private void DrawChart(List<Modules.AssetManager.ProductionUnit> units)
        {
            ChartCanvas.Children.Clear();

            double barHeight = 20;
            double spacing = 10;
            double labelWidth = 100;
            double barAreaWidth = ChartCanvas.Bounds.Width > 0 ? ChartCanvas.Bounds.Width - labelWidth : 400;
            double chartHeight = units.Count * (barHeight + spacing);
            ChartCanvas.Height = chartHeight;

            double maxCost = units.Max(u => u.ProductionCost);
            double maxCO2 = units.Max(u => u.CO2Emission ?? 0);
            double maxValue = Math.Max(maxCost, maxCO2);

            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                double top = i * (barHeight + spacing);

                double costWidth = (unit.ProductionCost / maxValue) * barAreaWidth * 0.5;
                double co2Width = ((unit.CO2Emission ?? 0) / maxValue) * barAreaWidth * 0.5;

                var costBar = new Rectangle
                {
                    Width = costWidth,
                    Height = barHeight,
                    Fill = Brushes.SteelBlue
                };
                Canvas.SetLeft(costBar, 0);
                Canvas.SetTop(costBar, top);
                ToolTip.SetTip(costBar, $"Cost: {unit.ProductionCost} DKK/MWh");
                ChartCanvas.Children.Add(costBar);

                var co2Bar = new Rectangle
                {
                    Width = co2Width,
                    Height = barHeight,
                    Fill = Brushes.OrangeRed
                };
                Canvas.SetLeft(co2Bar, costWidth + 5);
                Canvas.SetTop(co2Bar, top);
                ToolTip.SetTip(co2Bar, $"CO₂: {unit.CO2Emission ?? 0} kg/MWh");
                ChartCanvas.Children.Add(co2Bar);

                var label = new TextBlock
                {
                    Text = $"{unit.Name} | {unit.ProductionCost} DKK | {unit.CO2Emission ?? 0} kg",
                    FontSize = 11,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(label, costWidth + co2Width + 10);
                Canvas.SetTop(label, top + 2);
                ChartCanvas.Children.Add(label);
            }
        }

        private async void ExportEvaluationSummary(object? sender, RoutedEventArgs? e)
        {
            var text = EvaluationSummaryBlock.Text;

            if (string.IsNullOrWhiteSpace(text) || text.Contains("Summary will appear here"))
            {
                DetailsBlock.Text = "Nothing to export yet.";
                return;
            }

            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Evaluation Summary",
                SuggestedFileName = "evaluation_summary.txt",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt" } }
                }
            });

            if (file != null)
            {
                await using var stream = await file.OpenWriteAsync();
                using var writer = new StreamWriter(stream);
                await writer.WriteAsync(text);
                DetailsBlock.Text = $"Evaluation summary saved to: {file.Name} ✔";
            }
        }

        private async void OnOpenSavedResult(object? sender, RoutedEventArgs? e)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Saved Result",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                var fileName = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                var loadedUnits = resultManager.LoadResults(fileName);

                UnitList.ItemsSource = loadedUnits
                    .Select(u => $"{u.Name} — {u.MaxHeat} MW, {u.ProductionCost} DKK/MWh")
                    .ToList();

                var totalCost = loadedUnits.Sum(u => u.ProductionCost);
                var totalCO2 = loadedUnits.Sum(u => u.CO2Emission ?? 0);

                ScenarioSummaryBlock.Text = $"Loaded from file: {totalCost} DKK/MWh — {totalCO2} kg CO₂/MWh";
                DetailsBlock.Text = $"Opened saved result: {file.Name}";

                DrawChart(loadedUnits);
            }
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selected = UnitList.SelectedItem as string;
            if (selected == null)
            {
                DetailsBlock.Text = "Select a unit to see details...";
                return;
            }

            var unit = controller.GetUnits().FirstOrDefault(u => selected.StartsWith(u.Name));
            if (unit == null)
            {
                DetailsBlock.Text = "Details not found.";
                return;
            }

            DetailsBlock.Text = $"Details:\n" +
                $"Name: {unit.Name}\n" +
                $"Max Heat: {unit.MaxHeat} MW\n" +
                $"Production Cost: {unit.ProductionCost} DKK/MWh\n" +
                $"CO₂: {unit.CO2Emission ?? 0} kg/MWh\n" +
                $"Gas: {unit.GasConsumption?.ToString("0.0") ?? "–"} MWh\n" +
                $"Oil: {unit.OilConsumption?.ToString("0.0") ?? "–"} MWh\n" +
                $"Max Electricity: {unit.MaxElectricity?.ToString("0.0") ?? "–"} MW";
        }

        private void ReloadTimeSeries(object? sender, RoutedEventArgs? e)
        {
            controller.ReloadTimeSeries();
            DetailsBlock.Text = "Time series reloaded.";
        }

        private void SaveScenarioResults(object? sender, RoutedEventArgs? e)
        {
            var allUnits = controller.GetUnits();
            var scenarioUnits = currentScenario switch
            {
                "Scenario1" => allUnits.Where(u => u.Name is "GB1" or "GB2" or "OB1"),
                "Scenario2" => allUnits.Where(u => u.Name is "GB1" or "OB1" or "GM1" or "HP1"),
                _ => Enumerable.Empty<Modules.AssetManager.ProductionUnit>()
            };

            if (ElectricOnlyCheck.IsChecked == true)
                scenarioUnits = scenarioUnits.Where(u => u.MaxElectricity.HasValue && u.MaxElectricity > 0);

            var list = scenarioUnits.ToList();
            resultManager.SaveResults(currentScenario, list);
            DetailsBlock.Text = $"Saved scenario to: SavedResults/{currentScenario}_saved.csv";
        }

        private void CompareScenarios(object? sender, RoutedEventArgs? e)
        {
            var allUnits = controller.GetUnits();
            var s1 = allUnits.Where(u => u.Name is "GB1" or "GB2" or "OB1").ToList();
            var s2 = allUnits.Where(u => u.Name is "GB1" or "OB1" or "GM1" or "HP1").ToList();

            var cost1 = s1.Sum(u => u.ProductionCost);
            var cost2 = s2.Sum(u => u.ProductionCost);
            var co21 = s1.Sum(u => u.CO2Emission ?? 0);
            var co22 = s2.Sum(u => u.CO2Emission ?? 0);

            EvaluationSummaryBlock.Text =
                $"Scenario 1: Cost = {cost1:0.0} DKK, CO₂ = {co21:0.0} kg\n" +
                $"Scenario 2: Cost = {cost2:0.0} DKK, CO₂ = {co22:0.0} kg\n" +
                $"→ Scenario {(cost2 < cost1 && co22 < co21 ? "2 is more efficient overall" : "comparison mixed")}";

            (DataContext as MainWindowViewModel)?.GenerateScenarioComparisonChart();
        }

        private void ExportComparisonCsv(object? sender, RoutedEventArgs? e) { }

        private void OnShowWinterChart(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;
            var chartWindow = new WinterChartWindow(vm.WinterSeries, vm.HourLabels);
            chartWindow.Show();
        }

        private void OnShowSummerChart(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;
            var chartWindow = new SummerChartWindow(vm.SummerSeries, vm.HourLabels);
            chartWindow.Show();
        }

    }
}