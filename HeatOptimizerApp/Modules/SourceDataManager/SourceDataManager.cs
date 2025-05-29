using HeatOptimizerApp.Interfaces;

namespace HeatOptimizerApp.Modules.SourceDataManager
{
    public class SourceDataManager : IDataManager
    {
        public void LoadData(string path)
        {
            // Load time series data for demand and electricity prices
        }

        public void SaveData(string path)
        {
            // Save updated time series data if needed
        }
    }
}
