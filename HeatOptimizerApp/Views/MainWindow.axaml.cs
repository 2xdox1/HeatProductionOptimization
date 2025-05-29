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

            var summaries = controller.GetUnits()
                .Select(u => $"{u.Name} — {u.MaxHeat} MW, {u.ProductionCost} DKK/MWh")
                .ToList();

            UnitList.ItemsSource = summaries;
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selected = UnitList.SelectedItem as string;

            if (selected != null)
            {
                DetailsBlock.Text = $"Details:\n{selected}";
            }
            else
            {
                DetailsBlock.Text = "Select a unit to see details...";
            }
        }
    }
}
