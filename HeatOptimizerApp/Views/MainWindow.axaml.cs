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

            var names = controller.GetUnits()
                                  .Select(u => u.Name)
                                  .ToList();

            UnitList.ItemsSource = names;
        }
    }
}
