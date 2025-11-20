Create Windows application for AIS data processing. <APP>.

AIS data contains vessel position reports and voyage data in compressed (zip) cvs format per day. This data is processed using pre-processing tool and written as "source data".

"Source data" is located in a specific folder and should be selected by user at runtime.
In that specific folder, there are subfolders generated for multiple vessels and per vessel.
Each vessel folder has one JSON file with vessel related static data, and multiple processed CSV files with AIS data classified with date information. 
JSON file contains "ship static data" and defined in file `<MMSI>_static.json`
CSV file contains "ship AIS data" with is defined in file `<MMSI>_<date>.csv`

Application needs to process the each "ship static data" in the source folder one by one.
Minimum and maximum dates per ship using "ship AIS data" file names should be also processed.
Collected static data in addition to minimum and maximum available dates should be stored in a dedicated data structure in the code.
User should have ability to filter this data by ships MMSI, length, width, ship type, date range.

After selection of the specific parameters user should be able to:
 - filter "ship AIS data" to reduce number of position reports
 - create a route with waypoints for one of the ships, please use template for waypoint creation from `route_waypoint_template.xml`
 - save created route in XML format to specific folder 
  
 