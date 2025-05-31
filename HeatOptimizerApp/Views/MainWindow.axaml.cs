using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using HeatOptimizerApp.Modules.Core;
using System;
using System.IO;
using System.Linq;
using Avalonia;
using System.Collections.Generic;

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
            LoadScenario1(null, null); // default
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

            // Draw horizontal grid lines
            for (int i = 0; i < units.Count; i++)
            {
                double y = i * (barHeight + spacing);

                var gridLine = new Rectangle
                {
                    Width = ChartCanvas.Bounds.Width,
                    Height = barHeight,
                    Fill = Brushes.LightGray,
                    Opacity = 0.2
                };
                Canvas.SetTop(gridLine, y);
                Canvas.SetLeft(gridLine, 0);
                ChartCanvas.Children.Add(gridLine);
            }

            // Draw bars and labels
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

                // Labels to the right
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

            DetailsBlock.Text = $"Comparison:\n" +
                $"Scenario 1 → Cost: {cost1}, CO₂: {co21}\n" +
                $"Scenario 2 → Cost: {cost2}, CO₂: {co22}\n";

            double canvasHeight = ComparisonCanvas.Bounds.Height > 0 ? ComparisonCanvas.Bounds.Height : 120;
            double barWidth = 40;
            double spacing = 60;
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
        }

        private void ExportToCsv(object? sender, RoutedEventArgs? e)
        {
            // Already complete and unchanged
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
    }
}
