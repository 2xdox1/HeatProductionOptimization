using HeatOptimizerApp.Interfaces;
using System;

namespace HeatOptimizerApp.Modules.DataVisualization
{
    public class DataVisualization : IDataManager
    {
        public void LoadData(string path)
        {
            Console.WriteLine("Loaded data for visualization.");
        }

        public void SaveData(string path)
        {
            Console.WriteLine("Visualization saved.");
        }

        public void DisplayChart()
        {
            Console.WriteLine("Chart displayed.");
        }
    }
}
