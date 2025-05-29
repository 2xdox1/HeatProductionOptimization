using Avalonia;
using HeatOptimizerApp.Modules.Core;
using System;

namespace HeatOptimizerApp;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var controller = new ProjectController();
        controller.RunProject();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
