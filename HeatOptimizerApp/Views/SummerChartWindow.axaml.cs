using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore.SkiaSharpView;

namespace HeatOptimizerApp.Views
{
    public partial class SummerChartWindow : Window
    {
        // Parameterless constructor required by Avalonia runtime
        public SummerChartWindow()
        {
            InitializeComponent();
        }

        // Your existing constructor with series and labels
        public SummerChartWindow(IEnumerable<ISeries> series, List<string> labels)
        {
            InitializeComponent();

            var chart = new CartesianChart
            {
                Series = series.ToArray(),
                XAxes = new[] {
                    new Axis
                    {
                        Labels = labels,
                        Name = "Hour"
                    }
                },
                YAxes = new[] {
                    new Axis
                    {
                        Name = "Heat Produced (MW)"
                    }
                }
            };

            Content = chart;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}