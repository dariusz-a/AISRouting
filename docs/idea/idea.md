Create Windows application for AIS data processing. <APP>.

AIS data contains vessel position reports and voyage data in compressed (zip) cvs format per day.
Souce data is located in a specific folder and should be selected by user at runtime.
CSV file contains ship AIS data with is defined in file `AIS_information_CSV_files.txt`
You need to process the each file in the folder one by one.
Collected data from all files should be stored in a local database SQLite.
User should have ability to filter AIS data by ships MMSI, lendth, type, time range.

After selection of the specific parameters user should be able to:
 - filter AIS data to reduce number of position reports
 - create a route with waypoints for one of the ships, please use template for waypoint creation from `route_waypoint_template.xml`
 - save created route in XML format to specific folder 
  
 