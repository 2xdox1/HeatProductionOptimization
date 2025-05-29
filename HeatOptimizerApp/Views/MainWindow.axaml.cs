using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using HeatOptimizerApp.Modules.Core;
using System.Linq;

namespace HeatOptimizerApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly ProjectController controller;

        public MainWindow()
        {
            InitializeComponent();
            controller = new ProjectController();
            controller.RunProject();

            LoadScenario1(null, null); // Default on startup
        }

        private void LoadScenario1(object? sender, RoutedEventArgs? e)
        {
            var units = controller.GetUnits()
                .Where(u => u.Name == "GB1" || u.Name == "GB2" || u.Name == "OB1")
                .ToList();

            UnitList.ItemsSource = units
                .Select(u => $"{u.Name} — {u.MaxHeat} MW, {u.ProductionCost} DKK/MWh")
                .ToList();

            var totalCost = units.Sum(u => u.ProductionCost);
            var totalCO2 = units.Sum(u => u.CO2Emission ?? 0);

            ScenarioSummaryBlock.Text = $"Scenario total: {totalCost} DKK/MWh — {totalCO2} kg CO₂/MWh";
            DetailsBlock.Text = "Scenario 1 loaded.";
            DrawChart(units);
        }

        private void LoadScenario2(object? sender, RoutedEventArgs? e)
        {
            var units = controller.GetUnits()
                .Where(u => u.Name == "GB1" || u.Name == "OB1" || u.Name == "GM1" || u.Name == "HP1")
                .ToList();

            UnitList.ItemsSource = units
                .Select(u => $"{u.Name} — {u.MaxHeat} MW, {u.ProductionCost} DKK/MWh")
                .ToList();

            var totalCost = units.Sum(u => u.ProductionCost);
            var totalCO2 = units.Sum(u => u.CO2Emission ?? 0);

            ScenarioSummaryBlock.Text = $"Scenario total: {totalCost} DKK/MWh — {totalCO2} kg CO₂/MWh";
            DetailsBlock.Text = "Scenario 2 loaded.";
            DrawChart(units);
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selected = UnitList.SelectedItem as string;

            if (selected == null)
            {
                DetailsBlock.Text = "Select a unit to see details...";
                return;
            }

            var unit = controller.GetUnits().FirstOrDefault(u =>
                selected.StartsWith(u.Name));

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

        private void DrawChart(System.Collections.Generic.List<Modules.AssetManager.ProductionUnit> units)
        {
            ChartCanvas.Children.Clear();

            double barHeight = 20;
            double spacing = 10;
            double maxCost = units.Max(u => u.ProductionCost);
            double maxCO2 = units.Max(u => u.CO2Emission ?? 0);
            double canvasWidth = ChartCanvas.Bounds.Width > 0 ? ChartCanvas.Bounds.Width : 400;

            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];

                double costWidth = (unit.ProductionCost / maxCost) * canvasWidth * 0.5;
                double co2Width = ((unit.CO2Emission ?? 0) / maxCO2) * canvasWidth * 0.5;

                var costBar = new Rectangle
                {
                    Width = costWidth,
                    Height = barHeight,
                    Fill = Brushes.SteelBlue
                };
                Canvas.SetTop(costBar, i * (barHeight + spacing));
                Canvas.SetLeft(costBar, 0);

                var co2Bar = new Rectangle
                {
                    Width = co2Width,
                    Height = barHeight,
                    Fill = Brushes.OrangeRed
                };
                Canvas.SetTop(co2Bar, i * (barHeight + spacing));
                Canvas.SetLeft(co2Bar, costWidth + 5);

                ChartCanvas.Children.Add(costBar);
                ChartCanvas.Children.Add(co2Bar);
            }
        }
    }
}
