using Avalonia.Controls;
using HeatOptimizerApp.Modules.Core;
using System.Linq;

namespace HeatOptimizerApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var controller = new ProjectController();
            controller.RunProject();

            var units = controller.GetUnits()
                      .Select(u => $"{u.Name} — {u.MaxHeat} MW, {u.ProductionCost} DKK/MWh")
                      .ToList();
            UnitList.ItemsSource = units;
        }
    }
}
