using HeatOptimizerApp.Interfaces;
using System;

namespace HeatOptimizerApp.Modules.Optimizer
{
    public class Optimizer : IDataManager
    {
        public void LoadData(string path)
        {
            Console.WriteLine("Optimizer loaded data.");
        }

        public void SaveData(string path)
        {
            Console.WriteLine("Optimizer saved optimization results.");
        }

        public void RunOptimization()
        {
            Console.WriteLine("Running basic optimization logic...");
            // just a placeholder, details later
            Console.WriteLine("Optimization complete.");
        }
    }
}
