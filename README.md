# Heat Production Optimization - SDU Semester Project

This project is developed as part of the second-semester software engineering course at SDU. The goal is to optimize heat production for the fictional utility company "HeatItOn" in the city of Heatington using cost and CO₂-efficient schedules.

## Project Overview

HeatItOn provides heating to ~1600 buildings through a single district heating network. This system consists of traditional boilers and combined heat and power (CHP) units. Our software automates the scheduling of these units to minimize costs and emissions while ensuring full heat coverage.

## Modules Implemented

- **Asset Manager (AM)** – Manages static data like production unit specifications.
- **Source Data Manager (SDM)** – Handles time series data for heat demand and electricity prices.
- **Result Data Manager (RDM)** – Stores optimization results for production, emissions, and costs.
- **Optimizer (OPT)** – Calculates the most cost-efficient and CO₂-friendly heat production schedule.
- **Data Visualization (DV)** – Graphical display of inputs and results (time series, charts, and summaries).

## Key Features

- 📊 **Scenario support**: Two scenarios with different unit setups.
- 🔁 **Reloadable data**: Modify time series and reload them live.
- 💾 **Save/Load**: Persist configuration and results.
- 🔎 **Chart visualizations**: Heat demand, unit output, emissions, costs, electricity use, and profits.

## How to Run

1. Clone the repository:

    ```bash
    git clone <https://github.com/2xdox1/HeatProductionOptimization>
    ```

2. Open the solution in Visual Studio.
3. Set the main project (usually the UI) as the startup project.
4. Build and run.


## Folder Structure
```bash
/Assets/             ← Static data (unit definitions, images)
/Data/               ← CSV files for heat demand & electricity prices
/Results/            ← Output from optimization
/Modules/
  ├── AssetManager/
  ├── SourceDataManager/
  ├── ResultDataManager/
  ├── Optimizer/
  └── DataVisualization/
  ```
## Testing
This project includes unit tests for each major module using the xUnit testing framework:

- **SourceDataManager**: verifies correct parsing of heat demand and electricity prices
- **AssetManager**: ensures correct loading of production units and skips malformed lines
- **ResultDataManager**: tests CSV export and re-import of scenario results

Tests are designed to cover normal and edge cases, including null values and data integrity

Running Tests
From the root of the repository:
```bash
dotnet test
```
Test data is located in *HeatOptimizerApp.Tests/TestData/. Output results are saved in SavedResults/*.

## Technolies Used
- .NET 9.0
- Avalonia UI
- xUnit for unit testing
- Visual Studio / VS Code (cross-platform support)
- Git + GitHub for version control

## Contributors & Project Info
- Faculty: **Faculty of Engineering**
- Institution: **University of Southern Denmark (Sønderborg)**
- Program of Study: BSc in Engineering (**Software Engineering**)
- Course: **T630006401 – Development of Software Systems**
- Semester: **2nd Semester, Spring 2025**
- Supervisor: **Rita Bentinho**

Team Members:
- **Jakub Cuninka**
- **Adriana Kulczycka**
- **Dorina Petra Nagy**
- **Balazs Istvan Nemeth**
- **Tomass Zarins**

*For a full breakdown of individual contributions, see Final Report, Chapter 5.*