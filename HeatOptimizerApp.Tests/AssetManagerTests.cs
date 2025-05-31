using System;
using System.IO;
using Xunit;
using HeatOptimizerApp.Modules.AssetManager;

public class AssetManagerTests
{
    [Fact]
    public void LoadData_ShouldPopulateUnitsCorrectly()
    {
        // Arrange
        var assetManager = new AssetManager();
        var path = Path.Combine("TestData", "ProductionUnits_Test.csv");
        Console.WriteLine($"Looking for file at: {Path.GetFullPath(path)}");

        // Act
        assetManager.LoadData(path);

        // Assert
        Assert.Equal(3, assetManager.Units.Count);

        Assert.Equal("GB1", assetManager.Units[0].Name);
        Assert.Equal(4.0, assetManager.Units[0].MaxHeat);
        Assert.Equal(520, assetManager.Units[0].ProductionCost);
        Assert.Equal(175, assetManager.Units[0].CO2Emission);
        Assert.Equal(0.9, assetManager.Units[0].GasConsumption);
        Assert.Null(assetManager.Units[0].OilConsumption);
        Assert.Null(assetManager.Units[0].MaxElectricity);
    }
}
