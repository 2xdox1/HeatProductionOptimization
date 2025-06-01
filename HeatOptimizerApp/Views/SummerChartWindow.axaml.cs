using Avalonia.Controls;
using HeatOptimizerApp.ViewModels;

namespace HeatOptimizerApp.Views;

public partial class SummerChartWindow : Window
{
    public SummerChartWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}