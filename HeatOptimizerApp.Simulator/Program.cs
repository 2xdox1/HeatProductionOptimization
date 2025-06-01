using HeatOptimizerApp.Modules.Optimizer;

class Program
{
    static void Main(string[] args)
    {
        var optimizer = new Optimizer();
        optimizer.LoadData("HeatOptimizerApp/Data/summer.csv");
        optimizer.RunOptimization();
        optimizer.SaveData("../../../HeatOptimizerApp/SavedResults/scenario2_simulated.csv");

        Console.WriteLine(" Summer simulation complete.");
    }
}