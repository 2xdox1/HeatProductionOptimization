using HeatOptimizerApp.Modules.Optimizer;

class Program
{
    static void Main(string[] args)
    {
        var optimizer = new Optimizer();
        optimizer.LoadData("../../../HeatOptimizerApp/Data/winter.csv");
        optimizer.RunOptimization();
        optimizer.SaveData("../../../HeatOptimizerApp/SavedResults/scenario1_simulated.csv");

        Console.WriteLine("Winter simulation complete.");
    }
}
