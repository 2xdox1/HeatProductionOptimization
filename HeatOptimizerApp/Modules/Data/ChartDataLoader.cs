using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Linq;

namespace HeatOptimizerApp.Modules.Data;

public static class ChartDataLoader
{
    public static ISeries[] GetSeries(string season, string scale, string scenario)
    {
        double[] data;

        if (scenario == "Scenario1")
        {
            data = scale switch
            {
                "Hourly" => Enumerable.Range(0, 24).Select(i => Math.Sin(i * 0.3) * 2 + 5).ToArray(),
                "Daily" => Enumerable.Range(1, 14).Select(i => (double)(i * 2)).ToArray(),
                _ => Array.Empty<double>()
            };
        }
        else // Scenario2
        {
            data = scale switch
            {
                "Hourly" => Enumerable.Range(0, 24).Select(i => Math.Cos(i * 0.3) * 2 + 3).ToArray(),
                "Daily" => Enumerable.Range(1, 14).Select(i => (double)(20 - i)).ToArray(),
                _ => Array.Empty<double>()
            };
        }

        return new ISeries[]
        {
        new ColumnSeries<double>
        {
            Name = $"{scenario} - {season} {scale}",
            Values = data
        }
        };
    }

}