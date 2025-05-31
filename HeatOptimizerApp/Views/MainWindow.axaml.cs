using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using HeatOptimizerApp.Modules.Core;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HeatOptimizerApp.Modules.ResultDataManager;


namespace HeatOptimizerApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly ProjectController controller;
        private readonly Modules.ResultDataManager.ResultDataManager resultManager = new();
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

        private void CompareScenarios(object? sender, RoutedEventArgs? e)
        {
            ComparisonCanvas.Children.Clear();

            var allUnits = controller.GetUnits();
            var s1 = allUnits.Where(u => u.Name is "GB1" or "GB2" or "OB1");
            var s2 = allUnits.Where(u => u.Name is "GB1" or "OB1" or "GM1" or "HP1");

            var cost1 = s1.Sum(u => u.ProductionCost);
            var cost2 = s2.Sum(u => u.ProductionCost);
            var co21 = s1.Sum(u => u.CO2Emission ?? 0);
            var co22 = s2.Sum(u => u.CO2Emission ?? 0);

            double canvasHeight = ComparisonCanvas.Bounds.Height > 0 ? ComparisonCanvas.Bounds.Height : 120;
            double barWidth = 40;
            double maxValue = new[] { cost1, cost2, co21, co22 }.Max();

            var bars = new[]
            {
                (Label: "S1", Value: cost1, X: 30, Color: Brushes.SteelBlue),
                (Label: "S1", Value: co21, X: 80, Color: Brushes.OrangeRed),
                (Label: "S2", Value: cost2, X: 150, Color: Brushes.CornflowerBlue),
                (Label: "S2", Value: co22, X: 200, Color: Brushes.Tomato)
            };

            foreach (var bar in bars)
            {
                double height = (bar.Value / maxValue) * (canvasHeight - 20);
                var rect = new Rectangle
                {
                    Width = barWidth,
                    Height = height,
                    Fill = bar.Color
                };
                Canvas.SetLeft(rect, bar.X);
                Canvas.SetTop(rect, canvasHeight - height - 10);
                ToolTip.SetTip(rect, $"{bar.Label} — {bar.Value:0.0}");
                ComparisonCanvas.Children.Add(rect);
            }

            for (int i = 0; i <= 4; i++)
            {
                double val = maxValue * i / 4;
                double y = canvasHeight - (val / maxValue * (canvasHeight - 20)) - 10;
                var label = new TextBlock
                {
                    Text = val.ToString("0"),
                    FontSize = 10
                };
                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, y);
                ComparisonCanvas.Children.Add(label);
            }

            EvaluationSummaryBlock.Text =
                $"Scenario 1: Cost = {cost1} DKK, CO₂ = {co21} kg\n" +
                $"Scenario 2: Cost = {cost2} DKK, CO₂ = {co22} kg\n" +
                $"→ Scenario {(cost2 < cost1 && co22 < co21 ? "2 is more efficient overall" : "comparison mixed")}";
        }

        private async void ExportEvaluationSummary(object? sender, RoutedEventArgs? e)
        {
            var text = EvaluationSummaryBlock.Text;

            if (string.IsNullOrWhiteSpace(text) || text.Contains("Summary will appear here"))
            {
                DetailsBlock.Text = "Nothing to export yet.";
                return;
            }

#pragma warning disable CS0618
            var saveDialog = new SaveFileDialog
            {
                Title = "Export Evaluation Summary",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Text Files", Extensions = { "txt" } }
                }
            };
#pragma warning restore CS0618

            var path = await saveDialog.ShowAsync(this);
            if (!string.IsNullOrWhiteSpace(path))
            {
                File.WriteAllText(path, text);
                DetailsBlock.Text = $"Evaluation summary saved to: {path} ✔";
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

        private void ExportToCsv(object? sender, RoutedEventArgs? e)
        {
            // CSV export already exists and works.
        }

        private void ExportComparisonCsv(object? sender, RoutedEventArgs? e)
        {
            // Optional: implement later if time allows
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

        private async void OnOpenSavedResult(object? sender, RoutedEventArgs? e)
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Open saved scenario CSV",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "CSV Files", Extensions = { "csv" } }
                }
            };

            var result = await openDialog.ShowAsync(this);

            if (result != null && result.Length > 0)
            {
                var path = result[0];
                var loadedUnits = new ResultDataManager().LoadResults(System.IO.Path.GetFileNameWithoutExtension(path));

                UnitList.ItemsSource = loadedUnits
                    .Select(u => $"{u.Name} — {u.MaxHeat} MW, {u.ProductionCost} DKK/MWh")
                    .ToList();

                var totalCost = loadedUnits.Sum(u => u.ProductionCost);
                var totalCO2 = loadedUnits.Sum(u => u.CO2Emission ?? 0);

                ScenarioSummaryBlock.Text = $"Loaded from file: {totalCost} DKK/MWh — {totalCO2} kg CO₂/MWh";
                DetailsBlock.Text = $"Opened saved result: {System.IO.Path.GetFileName(path)}";

                DrawChart(loadedUnits);
            }
        }
        private void ReloadTimeSeries(object? sender, RoutedEventArgs? e)
        {
            controller.ReloadTimeSeries();
            DetailsBlock.Text = "Time series reloaded.";
        }
    }
}
