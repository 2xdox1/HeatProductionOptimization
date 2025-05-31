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
}
