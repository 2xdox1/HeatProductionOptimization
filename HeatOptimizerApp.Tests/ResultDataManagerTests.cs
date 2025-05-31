using HeatOptimizerApp.Modules.AssetManager;
using HeatOptimizerApp.Modules.ResultDataManager;
using Xunit;
using System.Collections.Generic;
using System.IO;

public class ResultDataManagerTests
{
    [Fact]
    public void SaveAndLoadResults_ShouldPreserveUnitData()
    {
        // Arrange
        var rdm = new ResultDataManager();
        var scenarioName = "UnitTestScenario";

        var units = new List<ProductionUnit>
        {
            new ProductionUnit { Name = "BoilerA", MaxHeat = 4.0, ProductionCost = 500, CO2Emission = 120, GasConsumption = 0.9, OilConsumption = null, MaxElectricity = 1.5 },
            new ProductionUnit { Name = "BoilerB", MaxHeat = 6.0, ProductionCost = 750, CO2Emission = 180, GasConsumption = null, OilConsumption = 1.2, MaxElectricity = null }
        };

        // Act
        rdm.SaveResults(scenarioName, units);
        var loadedUnits = rdm.LoadResults(scenarioName);

        // Assert
        Assert.Equal(units.Count, loadedUnits.Count);
        Assert.Equal("BoilerA", loadedUnits[0].Name);
        Assert.Equal(4.0, loadedUnits[0].MaxHeat);
        Assert.Equal(500, loadedUnits[0].ProductionCost);
        Assert.Equal(120, loadedUnits[0].CO2Emission);
        Assert.Equal(0.9, loadedUnits[0].GasConsumption);
        Assert.Null(loadedUnits[0].OilConsumption);
        Assert.Equal(1.5, loadedUnits[0].MaxElectricity);
    }
}
