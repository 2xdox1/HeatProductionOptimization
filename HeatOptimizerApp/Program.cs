using Avalonia;
using HeatOptimizerApp.Modules.AssetManager;
using System;

namespace HeatOptimizerApp;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        TestAssetManager();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static void TestAssetManager()
    {
        string path = "./Data/ProductionUnits.csv";
        var assetManager = new AssetManager();
        assetManager.LoadData(path);

        Console.WriteLine("Asset Manager loaded these units:");
        foreach (var unit in assetManager.Units)
        {
            Console.WriteLine($"{unit.Name} ({unit.MaxHeat} MW)");
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
