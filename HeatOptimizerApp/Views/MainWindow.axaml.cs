using Avalonia.Controls;
using Avalonia.Interactivity;
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
            LoadScenario1(null, null); // Default view
        }

        private void LoadScenario1(object? sender, RoutedEventArgs? e)
        {
            var units = controller.GetUnits()
                .Where(u => u.Name == "GB1" || u.Name == "GB2" || u.Name == "OB1")
                .Select(u => $"{u.Name} — {u.MaxHeat} MW, {u.ProductionCost} DKK/MWh")
                .ToList();

            UnitList.ItemsSource = units;
            DetailsBlock.Text = "Scenario 1 loaded.";
        }

        private void LoadScenario2(object? sender, RoutedEventArgs? e)
        {
            var units = controller.GetUnits()
                .Where(u => u.Name == "GB1" || u.Name == "OB1" || u.Name == "GM1" || u.Name == "HP1")
                .Select(u => $"{u.Name} — {u.MaxHeat} MW, {u.ProductionCost} DKK/MWh")
                .ToList();

            UnitList.ItemsSource = units;
            DetailsBlock.Text = "Scenario 2 loaded.";
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
    }
}
