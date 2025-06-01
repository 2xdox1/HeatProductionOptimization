using System;
using HeatOptimizerApp.Modules.Optimizer;

public static class OptimizerTests
{
    public static void Run()
    {
        var optimizer = new Optimizer();

        optimizer.LoadData("../../../HeatOptimizerApp/Data/winter.csv"); // adjust if needed
        optimizer.RunOptimization();
        optimizer.SaveData("../../../HeatOptimizerApp/SavedResults/scenario1_simulated.csv");

        Console.WriteLine(" Winter optimization completed and saved!");
    }
}