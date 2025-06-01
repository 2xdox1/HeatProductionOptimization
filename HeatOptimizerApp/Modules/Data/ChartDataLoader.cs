using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;

namespace HeatOptimizerApp.Modules.Data;

public static class ChartDataLoader
{
    public static ISeries[] GetSeries(string season, string scale)
    {
        var data = (season, scale) switch
        {
            ("Winter", "Hourly") => new double[] { 1, 2, 3, 2, 4, 5, 4, 3, 3, 2, 1, 1, 2, 3, 4, 5, 5, 4, 3, 2, 1, 1, 2, 3 },
            ("Summer", "Hourly") => new double[] { 0.5, 0.7, 0.8, 1.0, 0.9, 0.8, 0.7, 0.6, 0.6, 0.7, 0.8, 1.0, 1.1, 0.9, 0.8, 0.7, 0.6, 0.5, 0.5, 0.6, 0.7, 0.8, 0.9, 0.6 },
            ("Winter", "Daily")  => new double[] { 10, 12, 15, 13, 14, 11, 10 },
            ("Summer", "Daily")  => new double[] { 5, 6, 6, 5, 4, 4, 5 },
            _ => Array.Empty<double>()
        };

        return new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = $"{season} - {scale}",
                Values = data
            }
        };
    }
}