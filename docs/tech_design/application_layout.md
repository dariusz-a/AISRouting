# Application Layout and UI Components

This document covers the UI components hierarchy, layout structure, navigation flow, responsive design approach, and reusable component documentation for the AISRouting WPF application.

## Main Window Layout

### Overall Structure

```
┌─────────────────────────────────────────────────────────────┐
│  AISRouting - Main Window                          [_][□][X] │
├─────────────────────────────────────────────────────────────┤
│  ┌─ Input Configuration ──────────────────────────────────┐ │
│  │                                                          │ │
│  │  Input Folder:  [C:\AISData\..................] [Browse]│ │
│  │                                                          │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌─ Vessel Selection ─────────────────────────────────────┐ │
│  │                                                          │ │
│  │  Select Vessel: [Sea Explorer (205196000)      ▼]       │ │
│  │                                                          │ │
│  │  Ship Static Data:                                      │ │
│  │  ┌────────────────────────────────────────────────────┐ │ │
│  │  │ MMSI: 205196000                                    │ │ │
│  │  │ Name: Sea Explorer                                 │ │ │
│  │  │ Length: 180.5 m                                    │ │ │
│  │  │ Beam: 32.2 m                                       │ │ │
│  │  │ Draught: 8.5 m                                     │ │ │
│  │  │ Available Date Range: 2024-01-01 to 2024-01-31    │ │ │
│  │  └────────────────────────────────────────────────────┘ │ │
│  │                                                          │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌─ Time Interval Selection ──────────────────────────────┐ │
│  │                                                          │ │
│  │  Start Time: [2024-01-15] [06:00:00]                   │ │
│  │  Stop Time:  [2024-01-15] [18:00:00]                   │ │
│  │                                                          │ │
│  │                              [Create Track]             │ │
│  │                                                          │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌─ Track Results ────────────────────────────────────────┐ │
│  │                                                          │ │
│  │  Generated Waypoints (50):                              │ │
│  │  ┌────────────────────────────────────────────────────┐ │ │
│  │  │ # │ Time            │ Lat      │ Lon      │ Speed │ │ │
│  │  │ 1 │ 06:00:00        │ 55.1234  │ 12.3456  │ 12.3  │ │ │
│  │  │ 2 │ 06:05:23        │ 55.1456  │ 12.3678  │ 12.5  │ │ │
│  │  │ 3 │ 06:12:45        │ 55.1678  │ 12.3890  │ 11.8  │ │ │
│  │  │ ...                                                 │ │ │
│  │  └────────────────────────────────────────────────────┘ │ │
│  │                                                          │ │
│  │  Output Folder: [C:\Routes\....................] [Browse]│ │
│  │                                        [Export to XML]   │ │
│  │                                                          │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                               │
│  Status: Ready                                                │
└─────────────────────────────────────────────────────────────┘
```

### Layout Hierarchy

```
MainWindow
├── Grid (main container)
│   ├── MenuBar (future: File, Help menus)
│   ├── InputConfigurationPanel
│   │   ├── Label "Input Folder"
│   │   ├── TextBox (input folder path, read-only)
│   │   └── Button "Browse"
│   ├── VesselSelectionPanel
│   │   ├── Label "Select Vessel"
│   │   ├── ComboBox (vessel list)
│   │   └── StaticDataTextBox (multi-line, read-only)
│   ├── TimeIntervalPanel
│   │   ├── Label "Start Time"
│   │   ├── DatePicker (start date)
│   │   ├── TimePicker (start time, seconds resolution)
│   │   ├── Label "Stop Time"
│   │   ├── DatePicker (stop date)
│   │   ├── TimePicker (stop time, seconds resolution)
│   │   └── Button "Create Track"
│   ├── TrackResultsPanel
│   │   ├── Label "Generated Waypoints"
│   │   ├── DataGrid (waypoint list)
│   │   ├── Label "Output Folder"
│   │   ├── TextBox (output folder path, read-only)
│   │   ├── Button "Browse"
│   │   └── Button "Export to XML"
│   └── StatusBar
│       └── StatusBarItem (status messages)
```

## XAML Implementation

### MainWindow.xaml

```xml
<Window x:Class="AISRouting.App.WPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:AISRouting.App.WPF.ViewModels"
        Title="AISRouting" Height="900" Width="1200" MinHeight="700" MinWidth="1000"
        WindowStartupLocation="CenterScreen">
    
    <Window.DataContext>
        <vm:MainViewModel />
    </Window.DataContext>

    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />  <!-- Input Configuration -->
            <RowDefinition Height="Auto" />  <!-- Vessel Selection -->
            <RowDefinition Height="Auto" />  <!-- Time Interval -->
            <RowDefinition Height="*" />     <!-- Track Results (expandable) -->
            <RowDefinition Height="Auto" />  <!-- Status Bar -->
        </Grid.RowDefinitions>

        <!-- Input Configuration Panel -->
        <GroupBox Grid.Row="0" Header="Input Configuration" Margin="0,0,0,10">
            <Grid Margin="10">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto" />
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                </Grid.ColumnDefinitions>

                <TextBlock Grid.Column="0" Text="Input Folder:" 
                           VerticalAlignment="Center" Margin="0,0,10,0" />
                <TextBox Grid.Column="1" Text="{Binding InputFolderPath}" 
                         IsReadOnly="True" VerticalContentAlignment="Center" />
                <Button Grid.Column="2" Content="Browse..." Width="100" Margin="10,0,0,0"
                        Command="{Binding SelectInputFolderCommand}" />
            </Grid>
        </GroupBox>

        <!-- Vessel Selection Panel -->
        <GroupBox Grid.Row="1" Header="Vessel Selection" Margin="0,0,0,10">
            <Grid Margin="10">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                </Grid.RowDefinitions>

                <Grid Grid.Row="0" Margin="0,0,0,10">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>

                    <TextBlock Grid.Column="0" Text="Select Vessel:" 
                               VerticalAlignment="Center" Margin="0,0,10,0" />
                    <ComboBox Grid.Column="1" 
                              ItemsSource="{Binding AvailableVessels}"
                              SelectedItem="{Binding SelectedVessel}"
                              DisplayMemberPath="Name" />
                </Grid>

                <GroupBox Grid.Row="1" Header="Ship Static Data">
                    <TextBox Text="{Binding StaticDataDisplay, Mode=OneWay}"
                             IsReadOnly="True" TextWrapping="Wrap"
                             VerticalScrollBarVisibility="Auto"
                             Height="120" FontFamily="Consolas" />
                </GroupBox>
            </Grid>
        </GroupBox>

        <!-- Time Interval Panel -->
        <GroupBox Grid.Row="2" Header="Time Interval Selection" Margin="0,0,0,10">
            <Grid Margin="10">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                </Grid.RowDefinitions>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto" />
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>

                <TextBlock Grid.Row="0" Grid.Column="0" Text="Start Time:" 
                           VerticalAlignment="Center" Margin="0,0,10,0" />
                <DatePicker Grid.Row="0" Grid.Column="1" 
                            SelectedDate="{Binding TimeInterval.StartDate}" 
                            Margin="0,0,10,0" />
                <TextBlock Grid.Row="0" Grid.Column="2" Text="Time:" 
                           VerticalAlignment="Center" Margin="0,0,10,0" />
                <TextBox Grid.Row="0" Grid.Column="3" 
                         Text="{Binding TimeInterval.StartTime, StringFormat=HH:mm:ss}" />

                <TextBlock Grid.Row="1" Grid.Column="0" Text="Stop Time:" 
                           VerticalAlignment="Center" Margin="0,10,10,0" />
                <DatePicker Grid.Row="1" Grid.Column="1" 
                            SelectedDate="{Binding TimeInterval.StopDate}" 
                            Margin="0,10,10,0" />
                <TextBlock Grid.Row="1" Grid.Column="2" Text="Time:" 
                           VerticalAlignment="Center" Margin="0,10,10,0" />
                <TextBox Grid.Row="1" Grid.Column="3" 
                         Text="{Binding TimeInterval.StopTime, StringFormat=HH:mm:ss}"
                         Margin="0,10,0,0" />

                <Button Grid.Row="2" Grid.Column="3" Content="Create Track" 
                        HorizontalAlignment="Right" Width="150" Height="35"
                        Margin="0,15,0,0"
                        Command="{Binding CreateTrackCommand}" />
            </Grid>
        </GroupBox>

        <!-- Track Results Panel -->
        <GroupBox Grid.Row="3" Header="Track Results">
            <Grid Margin="10">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="*" />
                    <RowDefinition Height="Auto" />
                </Grid.RowDefinitions>

                <TextBlock Grid.Row="0" Margin="0,0,0,10">
                    <Run Text="Generated Waypoints (" />
                    <Run Text="{Binding GeneratedWaypoints.Count, Mode=OneWay}" />
                    <Run Text="):" />
                </TextBlock>

                <DataGrid Grid.Row="1" 
                          ItemsSource="{Binding GeneratedWaypoints}"
                          AutoGenerateColumns="False" 
                          IsReadOnly="True"
                          CanUserAddRows="False"
                          CanUserDeleteRows="False"
                          SelectionMode="Single"
                          GridLinesVisibility="All">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="#" Binding="{Binding Index}" Width="50" />
                        <DataGridTextColumn Header="Time" Binding="{Binding Time, StringFormat=HH:mm:ss}" Width="100" />
                        <DataGridTextColumn Header="Latitude" Binding="{Binding Lat, StringFormat=F6}" Width="120" />
                        <DataGridTextColumn Header="Longitude" Binding="{Binding Lon, StringFormat=F6}" Width="120" />
                        <DataGridTextColumn Header="Speed (kts)" Binding="{Binding Speed, StringFormat=F1}" Width="100" />
                        <DataGridTextColumn Header="Heading" Binding="{Binding Heading}" Width="80" />
                    </DataGrid.Columns>
                </DataGrid>

                <Grid Grid.Row="2" Margin="0,10,0,0">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>

                    <TextBlock Grid.Column="0" Text="Output Folder:" 
                               VerticalAlignment="Center" Margin="0,0,10,0" />
                    <TextBox Grid.Column="1" Text="{Binding OutputFolderPath}" 
                             IsReadOnly="True" VerticalContentAlignment="Center" />
                    <Button Grid.Column="2" Content="Browse..." Width="100" 
                            Margin="10,0,10,0"
                            Command="{Binding SelectOutputFolderCommand}" />
                    <Button Grid.Column="3" Content="Export to XML" Width="150" Height="35"
                            Command="{Binding ExportRouteCommand}" />
                </Grid>
            </Grid>
        </GroupBox>

        <!-- Status Bar -->
        <StatusBar Grid.Row="4" Margin="0,10,0,0">
            <StatusBarItem>
                <TextBlock Text="{Binding StatusMessage}" />
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```

## UI Component Details

### Input Configuration Panel

**Purpose**: Allow user to select root folder containing vessel data

**Components:**
- **Label**: "Input Folder"
- **TextBox**: Display selected path (read-only, data-bound to `InputFolderPath`)
- **Button**: "Browse..." (Command: `SelectInputFolderCommand`)

**Behavior:**
1. User clicks "Browse..."
2. Folder browser dialog opens (via `IFolderDialogService`)
3. On selection, path displayed in TextBox
4. ViewModel triggers vessel discovery via `ISourceDataScanner`
5. Vessel combo box populated

**Validation:**
- Path must exist and be readable
- Must contain at least one MMSI subfolder
- If invalid, show error in status bar

### Vessel Selection Panel

**Purpose**: Choose vessel and view its static metadata

**Components:**
- **ComboBox**: Vessel list (data-bound to `AvailableVessels` collection)
  - DisplayMemberPath: "Name" (shows vessel name or MMSI)
  - SelectedItem: `SelectedVessel` (two-way binding)
- **GroupBox**: "Ship Static Data"
- **TextBox**: Multi-line, read-only (data-bound to `StaticDataDisplay`)

**Behavior:**
1. Combo populated after folder selection
2. User selects vessel
3. Static data displayed:
   ```
   MMSI: 205196000
   Name: Sea Explorer
   Length: 180.5 m
   Beam: 32.2 m
   Draught: 8.5 m
   Available Date Range: 2024-01-01 to 2024-01-31
   ```
4. Time interval pickers enabled and defaulted

**Formatting:**
- Static data: Fixed-width font (Consolas) for alignment
- Missing fields show "N/A" or fallback values

### Time Interval Selection Panel

**Purpose**: Set start/stop times with second resolution

**Components:**
- **DatePicker**: Start date (bound to `TimeInterval.StartDate`)
- **TextBox/TimePicker**: Start time with seconds (bound to `TimeInterval.StartTime`)
- **DatePicker**: Stop date (bound to `TimeInterval.StopDate`)
- **TextBox/TimePicker**: Stop time with seconds (bound to `TimeInterval.StopTime`)
- **Button**: "Create Track" (Command: `CreateTrackCommand`)

**Behavior:**
1. Defaults set when vessel selected:
   - Start: First CSV filename timestamp
   - Stop: Last CSV filename timestamp + 24 hours
2. User can adjust date/time
3. Validation: Start < Stop (CanExecute on command)
4. On "Create Track", progress indicator shown
5. Waypoints generated and displayed

**Time Format**: `HH:mm:ss` (24-hour with seconds)

### Track Results Panel

**Purpose**: Display generated waypoints and export controls

**Components:**
- **TextBlock**: "Generated Waypoints (count)"
- **DataGrid**: Waypoint list (columns: #, Time, Latitude, Longitude, Speed, Heading)
- **TextBox**: Output folder path (read-only)
- **Button**: "Browse..." (select output folder)
- **Button**: "Export to XML" (Command: `ExportRouteCommand`)

**DataGrid Columns:**
- **#**: Waypoint index (1-based)
- **Time**: HH:mm:ss format
- **Latitude**: Decimal degrees (6 decimal places)
- **Longitude**: Decimal degrees (6 decimal places)
- **Speed**: Knots (1 decimal place)
- **Heading**: Degrees (integer)

**Behavior:**
1. DataGrid populated after track creation
2. User reviews waypoints
3. User selects output folder
4. User clicks "Export to XML"
5. If file exists, conflict resolution dialog shown
6. Export completes, success message in status bar

### Status Bar

**Purpose**: Display status messages and feedback

**Components:**
- **StatusBarItem**: Text display (bound to `StatusMessage`)

**Messages:**
- `"Ready"` (idle)
- `"Scanning input folder..."` (progress)
- `"Loading position data..."` (progress)
- `"Generating optimized track..."` (progress)
- `"Track created: 50 waypoints from 720 records"` (success)
- `"Route exported to C:\Routes\205196000-20240115T060000-20240115T180000.xml"` (success)
- `"Error: No CSV files found in selected folder"` (error)

## Reusable UI Components

### FolderBrowserButton (Custom Control)

**Purpose**: Encapsulate folder selection logic

**XAML:**
```xml
<UserControl x:Class="AISRouting.App.WPF.Controls.FolderBrowserButton"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="Auto" />
        </Grid.ColumnDefinitions>
        
        <TextBox Grid.Column="0" Text="{Binding SelectedPath, Mode=TwoWay}" 
                 IsReadOnly="True" />
        <Button Grid.Column="1" Content="Browse..." Command="{Binding BrowseCommand}" />
    </Grid>
</UserControl>
```

**Usage:**
```xml
<controls:FolderBrowserButton SelectedPath="{Binding InputFolderPath}" />
```

### TimePicker (Custom Control or Third-Party)

**Purpose**: Time selection with seconds resolution

**Options:**
- Use WPF Extended Toolkit: `xceed:TimePicker`
- Custom TextBox with validation: `HH:mm:ss` format
- MaskedTextBox with format mask

**Recommendation**: Custom TextBox with regex validation

### Value Converters

**BoolToVisibilityConverter:**
```csharp
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }
}
```

**DateTimeToStringConverter:**
```csharp
public class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        return string.Empty;
    }
}
```

## Navigation Flow

### User Workflow

```
1. Launch Application
   ↓
2. Select Input Folder
   ↓
3. Vessel List Populated
   ↓
4. Select Vessel
   ↓
5. Static Data Displayed + Time Defaults Set
   ↓
6. Adjust Time Interval (optional)
   ↓
7. Click "Create Track"
   ↓
8. Waypoints Generated and Displayed
   ↓
9. Select Output Folder
   ↓
10. Click "Export to XML"
    ↓
11. File Saved (or conflict resolved)
    ↓
12. Success Message Shown
```

### Error Handling UI Flow

**Scenario: No CSV Files Found**
```
User selects input folder
   ↓
Scanner finds no CSV files
   ↓
Status bar shows: "Error: No CSV files found in selected folder"
   ↓
Vessel combo remains empty
   ↓
Create Track button disabled
```

**Scenario: Export Filename Conflict**
```
User clicks "Export to XML"
   ↓
File already exists
   ↓
Dialog: "File exists. Overwrite / Append suffix / Cancel?"
   ↓
User chooses option
   ↓
Export completes or cancels
```

## Responsive Design

### Window Sizing

- **Minimum Size**: 1000x700 pixels
- **Default Size**: 1200x900 pixels
- **Resizable**: Yes
- **Start Position**: Center screen

### Layout Behavior

**On Resize:**
- Input/Vessel/Time panels: Fixed height, stretch horizontally
- Track Results DataGrid: Expands vertically to fill available space
- Horizontal scrollbar appears if window too narrow

**Column Behavior:**
- TextBoxes stretch to fill available width
- Buttons maintain fixed width
- DataGrid columns auto-size or fixed width

## Styling and Themes

### Global Styles (Styles.xaml)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <!-- Button Style -->
    <Style TargetType="Button">
        <Setter Property="Padding" Value="10,5" />
        <Setter Property="Margin" Value="5" />
        <Setter Property="FontSize" Value="14" />
    </Style>

    <!-- TextBox Style -->
    <Style TargetType="TextBox">
        <Setter Property="Padding" Value="5" />
        <Setter Property="FontSize" Value="14" />
    </Style>

    <!-- GroupBox Style -->
    <Style TargetType="GroupBox">
        <Setter Property="Padding" Value="10" />
        <Setter Property="BorderBrush" Value="Gray" />
        <Setter Property="BorderThickness" Value="1" />
    </Style>

    <!-- DataGrid Style -->
    <Style TargetType="DataGrid">
        <Setter Property="AlternatingRowBackground" Value="LightGray" />
        <Setter Property="RowHeight" Value="25" />
        <Setter Property="HeadersVisibility" Value="Column" />
    </Style>
</ResourceDictionary>
```

### Color Scheme

- **Background**: White / Light Gray
- **Borders**: Gray (#808080)
- **Highlight**: Blue (#0078D4) (Windows accent color)
- **Error**: Red (#D13438)
- **Success**: Green (#107C10)

## Accessibility

### Keyboard Navigation

- **Tab Order**: Input folder → Vessel combo → Start date → Start time → Stop date → Stop time → Create Track → Export
- **Accelerator Keys**: Alt+B (Browse input), Alt+E (Export)
- **Enter Key**: Submit button in focus (Create Track or Export)

### Screen Reader Support

- Labels associated with controls via x:Name
- ARIA properties for custom controls
- Status bar messages announced

## Future Enhancements

1. **Map Visualization**: Add map control to display track preview
2. **Tabs/Pages**: Separate input, track generation, and export into tabs
3. **Dark Mode**: Toggle between light/dark themes
4. **Progress Bar**: Visual progress indicator for long operations
5. **Tooltips**: Hover help for all controls
6. **Undo/Redo**: Allow adjusting optimization parameters and regenerating

## Example Screenshot Descriptions

### Initial State
- All panels visible
- Input folder empty, Browse button enabled
- Vessel combo empty and disabled
- Time pickers disabled
- Create Track button disabled
- DataGrid empty
- Export button disabled

### After Folder Selection
- Input folder path displayed
- Vessel combo populated and enabled
- Other controls still disabled until vessel selected

### After Vessel Selection
- Static data displayed
- Time pickers enabled with defaults
- Create Track button enabled

### After Track Creation
- DataGrid populated with waypoints
- Export controls enabled
- Status shows success message

## References

- WPF Layout: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/layout
- WPF Controls: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/
- WPF Data Binding: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/data-binding-overview
