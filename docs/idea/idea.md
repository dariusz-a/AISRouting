# Create Windows application for AIS data processing named "AISRouting".

## General
Input AIS data contains vessel position reports and voyage data in CSV format per day. 
This data is processed using pre-processing tool and written as "source data".

## "Source Data" 
"Source Data" is located in a **input** folder and should be selected by user at runtime.

### Source Data folder Structure
```
./                  # Source Data root
  ├── 983445672/    # MMSI number of first vessel from AIS data
  │   ├── 983445672.json
  │   ├── 2024-01-01.csv
  │   ├── 2024-01-02.csv
  │   ├── 2024-01-03.csv
  │   ├── 2024-01-31.csv
  ├── 334342556/    # MMSI number of second vessel from AIS data
  │   ├── 334342556.json
  │   ├── 2024-01-01.csv
  │   ├── 2024-01-02.csv
  │   ├── 2024-01-03.csv
  │   ├── 2024-01-31.csv
  ├── 453234567/    # MMSI number of third vessel from AIS data
  │   ├── 453234567.json
  │   ├── 2024-01-01.csv
  │   ├── 2024-01-02.csv
  │   ├── 2024-01-03.csv
  │   ├── 2024-01-31.csv
  ├── 689498534/    # MMSI number of fourth vessel from AIS data
  │   ├── 689498534.json
  │   ├── 2024-01-28.csv
  │   ├── 2024-01-29.csv
  │   ├── 2024-01-30.csv
  │   ├── 2024-01-31.csv
  ├── 563456786/    # MMSI number of n-th vessel from AIS data
      ├── 563456786.json
      ├── 2024-01-18.csv
      ├── 2024-01-19.csv
      ├── 2024-01-20.csv
      ├── 2024-01-21.csv
  
```

In that specific folder, there are subfolders generated for multiple vessels named after theirs MMSI-s.
In such a folder there is one JSON file with vessel related static data, and multiple processed CSV files with AIS data. 
JSON file contains "ship static data" and defined in file <MMSI>.json an example is in file `205196000.json` . 
CSV file <dateOnly>.csv contains "ship AIS data", an example is in file `2025-03-15.csv` . 
<dateOnly> is formatted as YYYY-MM-DD. 

### Format OF CSV File
```
public class ShipDataOut
{
    public long? Time { get; set; } // Seconds since T0 = dateOnly(from file's name) at time 00:00:00
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? NavigationalStatusIndex { get; set; }
    public double? ROT { get; set; }
    public double? SOG { get; set; }
    public double? COG { get; set; }
    public int? Heading { get; set; }
    public double? Draught { get; set; }
    public int? DestinationIndex { get; set; }
    public long? EtaSecondsUntil { get; set; } // dt from current time (current time = T0 + Time) to arrival
}
```

## GUI 
Application needs to process each "ship static data" in the source folder one by one.
Minimum and maximum dates per ship using "ship AIS data" file names must be also extracted.
Collected static data and minimum and maximum available dates must be stored in a dedicated data structure in the code C# record ShipStaticData.

### Main workflow for user (what user does before main processing starts)
User must select **input** folder containing "Source data".
User must select **output** folder for export created route.
User must select the ship from drop-down list of available ships

After user selects the ship, on the side text window we can see the ShipStaticData.
The most important of this data is min and max available dates.
There should be Gui element(s) to select start and stop times (seconds resolution) within the [min, max] range, which we call **Time-interval**.

There is a **create track** button, pressing of which triggers the main track generation.

## Track generation (after **create track** button is clicked)
 - Select records corresponding to the selected **Time-interval** from .csv files.
 - Optimize selected data to reduce redundant information, based on significant deviation from previous course. Significant deviation is based on:
    - Heading
    - ROT "rate of turn"
    - Position history
    - Speed  
 - Generated route must be in format as in example file `route_waypoint_template.xml`
  