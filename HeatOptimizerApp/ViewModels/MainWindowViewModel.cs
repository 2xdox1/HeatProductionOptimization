using CommunityToolkit.Mvvm.ComponentModel;
using HeatOptimizerApp.Modules.AssetManager;
using HeatOptimizerApp.Modules.Core;
using System.Collections.ObjectModel;

namespace HeatOptimizerApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<ProductionUnit> units = new();

    private readonly ProjectController controller;

    public MainWindowViewModel()
    {
        controller = new ProjectController();
        controller.RunProject();

        foreach (var unit in controller.GetUnits())
        {
            Units.Add(unit);
        }
    }
}
