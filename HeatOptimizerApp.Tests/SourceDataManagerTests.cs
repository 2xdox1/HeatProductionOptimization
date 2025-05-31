using Xunit;
using HeatOptimizerApp.Modules.SourceDataManager;

public class SourceDataManagerTests
{
    [Fact]
    public void LoadData_ShouldPopulateHeatDemandCorrectly()
    {
        // Arrange
        var sdm = new SourceDataManager();
        string path = "TestData/HeatDemand_Winter.csv";  // Make sure the file is here!

        // Act
        sdm.LoadData(path);

        // Assert
        Assert.NotNull(sdm.HeatDemand);
        Assert.Equal(3, sdm.HeatDemand.Count);  // Match the number of data lines in your test file
    }

    [Fact]
    public void LoadData_ShouldPopulateElectricityPricesCorrectly()
    {
        // Arrange
        var sdm = new SourceDataManager();
        string path = "TestData/HeatDemand_Winter.csv";

        // Act
        sdm.LoadData(path);

        // Assert
        Assert.NotNull(sdm.ElectricityPrices);
        Assert.Equal(3, sdm.ElectricityPrices.Count); // Match number of lines in your test CSV

        // Optional: Check the actual values if you want
        Assert.Equal(1190.94, sdm.ElectricityPrices[0], 2); // Rounded to 2 decimal places
        Assert.Equal(1154.55, sdm.ElectricityPrices[1], 2);
        Assert.Equal(1116.22, sdm.ElectricityPrices[2], 2);
    }
}


