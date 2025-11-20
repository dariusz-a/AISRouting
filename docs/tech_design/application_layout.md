# Application Layout

This document covers UI components hierarchy, layout structure, navigation flow, responsive design approach, and reusable component documentation for the AisToXmlRouteConvertor application.

## 1. Overview

AisToXmlRouteConvertor features a single-window desktop interface built with Avalonia UI. The layout emphasizes a linear workflow: folder selection → ship selection → time range selection → processing. All controls are visible simultaneously to provide clear workflow visibility and minimize navigation complexity.

## 2. UI Architecture

### 2.1 Application Structure

```
App.axaml (Application Root)
└── MainWindow.axaml (Single Window)
    ├── MenuBar (future: File, Help menus)
    ├── MainPanel (StackPanel or Grid)
    │   ├── InputFolderSection
    │   ├── ShipSelectionSection
    │   ├── TimeIntervalSection
    │   ├── OptimizationParametersSection (optional, collapsible)
    │   ├── OutputFolderSection
    │   └── ProcessSection
    └── StatusBar (bottom status messages)
```

### 2.2 Window Specifications

**MainWindow**:
- **Title**: "AIS to XML Route Converter"
- **Minimum Size**: 900x700 pixels
- **Default Size**: 1200x800 pixels
- **Resizable**: Yes
- **Maximize**: Yes
- **Icon**: App-specific icon from Assets/Icons/app-icon.ico

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:AisToXmlRouteConvertor.ViewModels"
        x:Class="AisToXmlRouteConvertor.MainWindow"
        Title="AIS to XML Route Converter"
        Width="1200" Height="800"
        MinWidth="900" MinHeight="700"
        Icon="/Assets/Icons/app-icon.ico">
    
    <Window.DataContext>
        <vm:MainViewModel />
    </Window.DataContext>

    <!-- Layout content here -->
</Window>
```

## 3. Component Hierarchy and Layout

### 3.1 Main Layout Structure

The main window uses a vertical `StackPanel` or `DockPanel` for top-to-bottom workflow progression:

```xml
<DockPanel LastChildFill="True">
    <!-- Status bar at bottom -->
    <Border DockPanel.Dock="Bottom" 
            Background="#F0F0F0" 
            Padding="10,5"
            BorderBrush="#CCCCCC" 
            BorderThickness="0,1,0,0">
        <TextBlock Text="{Binding StatusMessage}" 
                   FontSize="12" 
                   Foreground="#333333" />
    </Border>

    <!-- Main content area fills remaining space -->
    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <StackPanel Margin="20" Spacing="20">
            <!-- Input Folder Section -->
            <controls:InputFolderSection />
            
            <!-- Ship Selection Section -->
            <controls:ShipSelectionSection />
            
            <!-- Time Interval Section -->
            <controls:TimeIntervalSection />
            
            <!-- Optimization Parameters Section (collapsible) -->
            <Expander Header="Advanced Optimization Settings" IsExpanded="False">
                <controls:OptimizationParametersSection />
            </Expander>
            
            <!-- Output Folder Section -->
            <controls:OutputFolderSection />
            
            <!-- Process Button Section -->
            <controls:ProcessButtonSection />
        </StackPanel>
    </ScrollViewer>
</DockPanel>
```

### 3.2 Section Spacing and Visual Hierarchy

**Margins and Padding**:
- Outer margin: 20px all sides
- Section spacing: 20px vertical
- Internal padding: 10-15px within sections
- Control spacing: 10px between related controls

**Visual Grouping**:
- Sections enclosed in `Border` with light background (`#F9F9F9`)
- Section headers in bold, 14pt font
- Subtle borders (`#DDDDDD`) for separation

## 4. Detailed Component Layouts

### 4.1 Input Folder Section

**Purpose**: Select the root folder containing MMSI subfolders.

**Components**:
- Section header label
- Path display text box (read-only)
- Browse button
- Validation message area (for errors)

**Layout**:
```xml
<Border Background="#F9F9F9" 
        BorderBrush="#DDDDDD" 
        BorderThickness="1" 
        CornerRadius="5" 
        Padding="15">
    <StackPanel Spacing="10">
        <!-- Header -->
        <TextBlock Text="1. Select Input Folder" 
                   FontSize="14" 
                   FontWeight="Bold" />
        
        <!-- Description -->
        <TextBlock Text="Choose the root folder containing MMSI subfolders with AIS data (CSV and JSON files)."
                   FontSize="12"
                   Foreground="#666666"
                   TextWrapping="Wrap" />
        
        <!-- Folder path and browse button -->
        <Grid ColumnDefinitions="*,Auto">
            <TextBox Grid.Column="0"
                     Text="{Binding InputFolder}"
                     IsReadOnly="True"
                     Watermark="No folder selected"
                     ToolTip.Tip="Select the root folder that contains per-MMSI subfolders with AIS data (one &lt;MMSI&gt;.json and YYYY-MM-DD.csv files)." />
            
            <Button Grid.Column="1"
                    Content="Browse..."
                    Command="{Binding BrowseInputFolderCommand}"
                    Margin="10,0,0,0"
                    MinWidth="100" />
        </Grid>
        
        <!-- Validation message -->
        <TextBlock Text="{Binding InputFolderError}"
                   Foreground="Red"
                   FontSize="12"
                   IsVisible="{Binding !!InputFolderError}" />
    </StackPanel>
</Border>
```

**Behavior**:
- Browse button opens OS folder picker dialog
- After folder selected, automatically scans for MMSI subfolders
- Displays error if no valid MMSI folders found
- Remembers last-used path across sessions

### 4.2 Ship Selection Section

**Purpose**: Display discovered vessels and allow selecting one for processing.

**Components**:
- Section header label
- Data grid (table) showing MMSI, Size, Interval, Length, Width
- Ship static data panel (displays when ship selected)

**Layout**:
```xml
<Border Background="#F9F9F9" 
        BorderBrush="#DDDDDD" 
        BorderThickness="1" 
        CornerRadius="5" 
        Padding="15">
    <StackPanel Spacing="10">
        <!-- Header -->
        <TextBlock Text="2. Select Ship" 
                   FontSize="14" 
                   FontWeight="Bold" />
        
        <!-- Ship table -->
        <DataGrid ItemsSource="{Binding AvailableShips}"
                  SelectedItem="{Binding SelectedShip}"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  GridLinesVisibility="All"
                  Height="250"
                  CanUserResizeColumns="True"
                  CanUserSortColumns="True">
            <DataGrid.Columns>
                <DataGridTextColumn Header="MMSI" 
                                    Binding="{Binding Mmsi}" 
                                    Width="120" />
                <DataGridTextColumn Header="Name" 
                                    Binding="{Binding Name}" 
                                    Width="200" />
                <DataGridTextColumn Header="Size (MB)" 
                                    Binding="{Binding SizeMB, StringFormat={}{0:F2}}" 
                                    Width="100" />
                <DataGridTextColumn Header="Interval [min, max]" 
                                    Binding="{Binding Interval}" 
                                    Width="250" />
                <DataGridTextColumn Header="Length (m)" 
                                    Binding="{Binding Length, StringFormat={}{0:F1}}" 
                                    Width="100" />
                <DataGridTextColumn Header="Width (m)" 
                                    Binding="{Binding Beam, StringFormat={}{0:F1}}" 
                                    Width="100" />
            </DataGrid.Columns>
        </DataGrid>
        
        <!-- Ship static data panel (visible when ship selected) -->
        <Border IsVisible="{Binding !!SelectedShip}"
                Background="#FFFFFF"
                BorderBrush="#CCCCCC"
                BorderThickness="1"
                CornerRadius="3"
                Padding="10">
            <Grid ColumnDefinitions="Auto,*" RowDefinitions="Auto,Auto,Auto,Auto">
                <TextBlock Grid.Row="0" Grid.Column="0" Text="Vessel Name:" FontWeight="Bold" Margin="0,0,10,5" />
                <TextBlock Grid.Row="0" Grid.Column="1" Text="{Binding SelectedShip.Name}" Margin="0,0,0,5" />
                
                <TextBlock Grid.Row="1" Grid.Column="0" Text="MMSI:" FontWeight="Bold" Margin="0,0,10,5" />
                <TextBlock Grid.Row="1" Grid.Column="1" Text="{Binding SelectedShip.Mmsi}" Margin="0,0,0,5" />
                
                <TextBlock Grid.Row="2" Grid.Column="0" Text="Dimensions:" FontWeight="Bold" Margin="0,0,10,5" />
                <TextBlock Grid.Row="2" Grid.Column="1" 
                           Text="{Binding SelectedShip.DimensionsText}" 
                           Margin="0,0,0,5" />
                
                <TextBlock Grid.Row="3" Grid.Column="0" Text="Available Data:" FontWeight="Bold" Margin="0,0,10,0" />
                <TextBlock Grid.Row="3" Grid.Column="1" Text="{Binding SelectedShip.DateRangeText}" />
            </Grid>
        </Border>
    </StackPanel>
</Border>
```

**Data Grid Columns**:
1. **MMSI**: 9-digit identifier (e.g., "205196000")
2. **Name**: Vessel name (e.g., "Alice's Container Ship")
3. **Size (MB)**: Total size of CSV files (e.g., "45.32")
4. **Interval [min, max]**: Date range (e.g., "2025-03-10 to 2025-03-20")
5. **Length (m)**: Vessel length (e.g., "285.0")
6. **Width (m)**: Vessel beam (e.g., "40.0")

**Behavior**:
- Table populated after input folder scan
- Single-selection mode
- Default sort: MMSI ascending
- Rows without CSV files are disabled (greyed out)
- Selecting row displays static data panel and enables time pickers

### 4.3 Time Interval Section

**Purpose**: Select start and end times for filtering AIS data.

**Components**:
- Section header label
- Start time picker (date + time)
- End time picker (date + time)
- Validation message area

**Layout**:
```xml
<Border Background="#F9F9F9" 
        BorderBrush="#DDDDDD" 
        BorderThickness="1" 
        CornerRadius="5" 
        Padding="15"
        IsEnabled="{Binding !!SelectedShip}">
    <StackPanel Spacing="10">
        <!-- Header -->
        <TextBlock Text="3. Select Time Interval" 
                   FontSize="14" 
                   FontWeight="Bold" />
        
        <!-- Start time -->
        <Grid ColumnDefinitions="120,*">
            <TextBlock Grid.Column="0" 
                       Text="Start Time (UTC):" 
                       VerticalAlignment="Center" />
            <DateTimePicker Grid.Column="1"
                            SelectedDateTime="{Binding StartTimeUtc}"
                            MinDate="{Binding SelectedShip.MinDateUtc}"
                            MaxDate="{Binding SelectedShip.MaxDateUtc}" />
        </Grid>
        
        <!-- End time -->
        <Grid ColumnDefinitions="120,*">
            <TextBlock Grid.Column="0" 
                       Text="End Time (UTC):" 
                       VerticalAlignment="Center" />
            <DateTimePicker Grid.Column="1"
                            SelectedDateTime="{Binding EndTimeUtc}"
                            MinDate="{Binding SelectedShip.MinDateUtc}"
                            MaxDate="{Binding SelectedShip.MaxDateUtc}" />
        </Grid>
        
        <!-- Validation message -->
        <TextBlock Text="{Binding TimeIntervalError}"
                   Foreground="Red"
                   FontSize="12"
                   IsVisible="{Binding !!TimeIntervalError}" />
        
        <!-- Info message -->
        <TextBlock Text="Selected interval will filter AIS positions to this time range."
                   FontSize="11"
                   Foreground="#666666"
                   FontStyle="Italic" />
    </StackPanel>
</Border>
```

**Behavior**:
- Disabled until ship selected
- Default values: ship's MinDateUtc and MaxDateUtc
- Validates: Start < End
- Validates: Both within ship's available range
- Real-time validation with inline error messages

**Validation Messages**:
- "Start time must be before End time"
- "Selected time is outside available data range"

### 4.4 Optimization Parameters Section (Advanced)

**Purpose**: Configure track optimization thresholds (optional, collapsible).

**Components**:
- Expander header
- Numeric input fields for thresholds
- Description labels

**Layout**:
```xml
<Expander Header="Advanced Optimization Settings" 
          IsExpanded="False"
          Background="#F9F9F9"
          BorderBrush="#DDDDDD"
          BorderThickness="1"
          CornerRadius="5"
          Padding="15">
    <StackPanel Spacing="10">
        <!-- Min Heading Change -->
        <Grid ColumnDefinitions="250,*">
            <TextBlock Grid.Column="0" 
                       Text="Min Heading Change (degrees):" 
                       VerticalAlignment="Center" />
            <NumericUpDown Grid.Column="1"
                           Value="{Binding OptimizationParams.MinHeadingChangeDeg}"
                           Minimum="0"
                           Maximum="180"
                           Increment="0.1"
                           FormatString="F1" />
        </Grid>
        
        <!-- Min Distance -->
        <Grid ColumnDefinitions="250,*">
            <TextBlock Grid.Column="0" 
                       Text="Min Distance (meters):" 
                       VerticalAlignment="Center" />
            <NumericUpDown Grid.Column="1"
                           Value="{Binding OptimizationParams.MinDistanceMeters}"
                           Minimum="0"
                           Maximum="10000"
                           Increment="1"
                           FormatString="F0" />
        </Grid>
        
        <!-- Min SOG Change -->
        <Grid ColumnDefinitions="250,*">
            <TextBlock Grid.Column="0" 
                       Text="Min Speed Change (knots):" 
                       VerticalAlignment="Center" />
            <NumericUpDown Grid.Column="1"
                           Value="{Binding OptimizationParams.MinSogChangeKnots}"
                           Minimum="0"
                           Maximum="50"
                           Increment="0.1"
                           FormatString="F1" />
        </Grid>
        
        <!-- ROT Threshold -->
        <Grid ColumnDefinitions="250,*">
            <TextBlock Grid.Column="0" 
                       Text="Rate of Turn Threshold (deg/sec):" 
                       VerticalAlignment="Center" />
            <NumericUpDown Grid.Column="1"
                           Value="{Binding OptimizationParams.RotThresholdDegPerSec}"
                           Minimum="0"
                           Maximum="10"
                           Increment="0.1"
                           FormatString="F1" />
        </Grid>
        
        <!-- Reset to defaults -->
        <Button Content="Reset to Defaults"
                Command="{Binding ResetOptimizationParamsCommand}"
                HorizontalAlignment="Left" />
    </StackPanel>
</Expander>
```

**Default Values**:
- Min Heading Change: 0.2 degrees
- Min Distance: 5 meters
- Min Speed Change: 0.2 knots
- Rate of Turn Threshold: 0.2 deg/sec

**Behavior**:
- Collapsed by default (advanced users only)
- Changes apply immediately to next optimization
- Reset button restores default values

### 4.5 Output Folder Section

**Purpose**: Select destination folder for exported XML files.

**Components**:
- Section header label
- Path display text box (read-only)
- Browse button
- Validation message area

**Layout**:
```xml
<Border Background="#F9F9F9" 
        BorderBrush="#DDDDDD" 
        BorderThickness="1" 
        CornerRadius="5" 
        Padding="15">
    <StackPanel Spacing="10">
        <!-- Header -->
        <TextBlock Text="4. Select Output Folder" 
                   FontSize="14" 
                   FontWeight="Bold" />
        
        <!-- Description -->
        <TextBlock Text="Choose where to save the generated XML route files."
                   FontSize="12"
                   Foreground="#666666"
                   TextWrapping="Wrap" />
        
        <!-- Folder path and browse button -->
        <Grid ColumnDefinitions="*,Auto">
            <TextBox Grid.Column="0"
                     Text="{Binding OutputFolder}"
                     IsReadOnly="True"
                     Watermark="No folder selected" />
            
            <Button Grid.Column="1"
                    Content="Browse..."
                    Command="{Binding BrowseOutputFolderCommand}"
                    Margin="10,0,0,0"
                    MinWidth="100" />
        </Grid>
        
        <!-- Validation message -->
        <TextBlock Text="{Binding OutputFolderError}"
                   Foreground="Red"
                   FontSize="12"
                   IsVisible="{Binding !!OutputFolderError}" />
    </StackPanel>
</Border>
```

**Behavior**:
- Browse button opens OS folder picker dialog
- Validates folder is writable before enabling Process button
- Displays error: "Selected folder is not writable. Choose a different folder."
- Remembers last-used path across sessions

### 4.6 Process Button Section

**Purpose**: Initiate AIS data processing and route generation.

**Components**:
- Large Process button
- Progress indicator (during processing)
- Result message area

**Layout**:
```xml
<Border Background="#F9F9F9" 
        BorderBrush="#DDDDDD" 
        BorderThickness="1" 
        CornerRadius="5" 
        Padding="15">
    <StackPanel Spacing="10">
        <!-- Header -->
        <TextBlock Text="5. Generate Route" 
                   FontSize="14" 
                   FontWeight="Bold" />
        
        <!-- Process button -->
        <Button Content="Process!"
                Command="{Binding ProcessCommand}"
                IsEnabled="{Binding CanProcess}"
                HorizontalAlignment="Center"
                FontSize="16"
                FontWeight="Bold"
                Padding="30,10"
                MinWidth="200">
            <Button.Styles>
                <Style Selector="Button:disabled">
                    <Setter Property="Opacity" Value="0.5" />
                </Style>
                <Style Selector="Button:pointerover">
                    <Setter Property="Background" Value="#0078D4" />
                </Style>
            </Button.Styles>
        </Button>
        
        <!-- Progress indicator -->
        <ProgressBar IsIndeterminate="True"
                     IsVisible="{Binding IsProcessing}"
                     Height="4" />
        
        <!-- Disabled reason tooltip -->
        <TextBlock Text="{Binding ProcessDisabledReason}"
                   FontSize="11"
                   Foreground="#999999"
                   HorizontalAlignment="Center"
                   IsVisible="{Binding !CanProcess}"
                   FontStyle="Italic" />
    </StackPanel>
</Border>
```

**Enabled Conditions**:
- Input folder selected with valid MMSI folders
- Ship selected
- Valid time interval (Start < End, within available range)
- Output folder selected and writable

**Disabled Reason Examples**:
- "Select input folder and ship to continue"
- "Invalid time interval"
- "Select output folder to continue"

**Behavior**:
- Disabled until all prerequisites met
- Shows progress bar during processing (indeterminate)
- Displays modal success/error dialog on completion
- Button returns to enabled state after dismissing dialog

### 4.7 Status Bar

**Purpose**: Display application status messages and feedback.

**Components**:
- Read-only text area at bottom of window
- Light background to distinguish from main content

**Layout**:
```xml
<Border DockPanel.Dock="Bottom" 
        Background="#F0F0F0" 
        Padding="10,5"
        BorderBrush="#CCCCCC" 
        BorderThickness="0,1,0,0">
    <TextBlock Text="{Binding StatusMessage}" 
               FontSize="12" 
               Foreground="#333333"
               TextTrimming="CharacterEllipsis" />
</Border>
```

**Message Examples**:
- "Ready"
- "Scanning input folder..."
- "Loaded 1,234 positions"
- "Optimized to 87 waypoints"
- "Track generated successfully: 205196000_20250315T000000_20250315T120000.xml"
- "Error: Selected folder is not writable. Choose a different folder."

## 5. Navigation Flow

### 5.1 Workflow Sequence

The UI guides users through a linear workflow:

```
1. Select Input Folder
   ↓ (automatically scans)
2. Select Ship from table
   ↓ (populates time pickers)
3. Select Time Interval
   ↓ (validates times)
4. (Optional) Adjust Optimization Parameters
   ↓
5. Select Output Folder
   ↓ (validates writability)
6. Press Process! Button
   ↓ (shows progress)
7. View Success/Error Dialog
```

### 5.2 Control Enablement States

**Initial State** (no folders selected):
- Input folder: Enabled (always)
- Ship table: Disabled (empty until scan)
- Time pickers: Disabled
- Optimization params: Disabled
- Output folder: Enabled
- Process button: Disabled ("Select input folder and ship to continue")

**After Input Folder Selected**:
- Ship table: Enabled (populated with MMSIs)
- Other controls remain same state

**After Ship Selected**:
- Time pickers: Enabled (default to ship's min/max dates)
- Optimization params: Enabled
- Process button: Still disabled (need output folder)

**After Output Folder Selected + Valid Time**:
- Process button: Enabled
- Status: "Ready to process"

**During Processing**:
- All controls: Disabled
- Progress bar: Visible (indeterminate)
- Process button: Disabled
- Status: "Processing..."

**After Processing Complete**:
- All controls: Re-enabled
- Dialog shown with result
- Status: Success or error message

### 5.3 Keyboard Navigation

**Tab Order**:
1. Input folder Browse button
2. Ship table (arrow keys to navigate rows)
3. Start time picker
4. End time picker
5. Optimization expander (if expanded, tab through numeric inputs)
6. Output folder Browse button
7. Process button

**Shortcuts**:
- **Ctrl+O**: Open input folder picker
- **Ctrl+S**: Open output folder picker (future)
- **Ctrl+P**: Trigger Process (if enabled)
- **F5**: Refresh ship table scan
- **Esc**: Close dialogs

## 6. Responsive Design Approach

### 6.1 Window Resizing Behavior

**Minimum Window Size**: 900x700 pixels
- Below this size, vertical scrollbar appears
- Ship table shows first 4-5 rows, scrolls vertically

**Large Window Sizes**:
- Sections expand horizontally to fill width
- Ship table grows vertically to show more rows
- Maximum practical height: ~400px for table

**Horizontal Scaling**:
- Text boxes and tables expand to fill available width
- Buttons remain fixed width (centered or left-aligned)
- Section padding scales proportionally

**Vertical Scaling**:
- ScrollViewer enables scrolling for small windows
- Ship table height fixed at 250px (scrolls internally)
- Other sections stack vertically with consistent spacing

### 6.2 Font Scaling

**Base Font Sizes**:
- Section headers: 14pt bold
- Body text: 12pt regular
- Labels: 12pt regular
- Status bar: 12pt regular
- Tooltips: 11pt regular

**Accessibility**:
- Supports OS-level font scaling (Windows Display Settings, macOS Accessibility)
- Minimum contrast ratio: 4.5:1 (WCAG AA)
- No hardcoded pixel fonts (use points)

### 6.3 High DPI Support

**Avalonia Automatic Scaling**:
- Avalonia handles DPI scaling automatically
- Vector-based UI scales crisply
- Icon assets provided in multiple sizes (16x16, 32x32, 48x48, 256x256)

**Testing**:
- 100% scaling (96 DPI): Standard desktop
- 125% scaling (120 DPI): Common laptop setting
- 150% scaling (144 DPI): High-DPI laptop
- 200% scaling (192 DPI): 4K monitors

## 7. Reusable Component Documentation

### 7.1 Folder Picker Control (Reusable)

**Purpose**: Consistent folder selection UI across input/output sections.

**API**:
```csharp
public class FolderPickerControl : UserControl
{
    public static readonly StyledProperty<string?> FolderPathProperty = ...;
    public static readonly StyledProperty<string?> WatermarkProperty = ...;
    public static readonly StyledProperty<ICommand?> BrowseCommandProperty = ...;
    
    public string? FolderPath { get; set; }
    public string? Watermark { get; set; }
    public ICommand? BrowseCommand { get; set; }
}
```

**Usage**:
```xml
<controls:FolderPickerControl 
    FolderPath="{Binding InputFolder}"
    Watermark="No folder selected"
    BrowseCommand="{Binding BrowseInputFolderCommand}" />
```

### 7.2 Validation Message Control (Reusable)

**Purpose**: Consistent error/warning display.

**API**:
```csharp
public class ValidationMessageControl : UserControl
{
    public static readonly StyledProperty<string?> MessageProperty = ...;
    public static readonly StyledProperty<ValidationLevel> LevelProperty = ...;
    
    public string? Message { get; set; }
    public ValidationLevel Level { get; set; } // Error, Warning, Info
}
```

**Usage**:
```xml
<controls:ValidationMessageControl 
    Message="{Binding InputFolderError}"
    Level="Error"
    IsVisible="{Binding !!InputFolderError}" />
```

### 7.3 Ship Data Panel Control (Reusable)

**Purpose**: Display ship static data in consistent format.

**API**:
```csharp
public class ShipDataPanelControl : UserControl
{
    public static readonly StyledProperty<ShipStaticData?> ShipDataProperty = ...;
    
    public ShipStaticData? ShipData { get; set; }
}
```

**Usage**:
```xml
<controls:ShipDataPanelControl 
    ShipData="{Binding SelectedShip}"
    IsVisible="{Binding !!SelectedShip}" />
```

## 8. Styling and Theming

### 8.1 Color Palette

**Primary Colors**:
- **Background**: `#FFFFFF` (white)
- **Section Background**: `#F9F9F9` (light grey)
- **Border**: `#DDDDDD` (medium grey)
- **Text**: `#333333` (dark grey)
- **Disabled Text**: `#999999` (light grey)

**Accent Colors**:
- **Primary Button**: `#0078D4` (blue, Windows style)
- **Button Hover**: `#005A9E` (darker blue)
- **Error**: `#E81123` (red)
- **Warning**: `#FFB900` (yellow)
- **Info**: `#0078D4` (blue)
- **Success**: `#107C10` (green)

### 8.2 Custom Styles (CustomStyles.axaml)

```xml
<Styles xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Section header style -->
    <Style Selector="TextBlock.SectionHeader">
        <Setter Property="FontSize" Value="14" />
        <Setter Property="FontWeight" Value="Bold" />
        <Setter Property="Margin" Value="0,0,0,10" />
    </Style>
    
    <!-- Section border style -->
    <Style Selector="Border.Section">
        <Setter Property="Background" Value="#F9F9F9" />
        <Setter Property="BorderBrush" Value="#DDDDDD" />
        <Setter Property="BorderThickness" Value="1" />
        <Setter Property="CornerRadius" Value="5" />
        <Setter Property="Padding" Value="15" />
        <Setter Property="Margin" Value="0,0,0,20" />
    </Style>
    
    <!-- Primary button style -->
    <Style Selector="Button.Primary">
        <Setter Property="Background" Value="#0078D4" />
        <Setter Property="Foreground" Value="White" />
        <Setter Property="FontWeight" Value="Bold" />
        <Setter Property="Padding" Value="20,10" />
    </Style>
    
    <Style Selector="Button.Primary:pointerover">
        <Setter Property="Background" Value="#005A9E" />
    </Style>
    
    <!-- Error text style -->
    <Style Selector="TextBlock.Error">
        <Setter Property="Foreground" Value="#E81123" />
        <Setter Property="FontSize" Value="12" />
    </Style>
</Styles>
```

### 8.3 Future Theme Support

**Phase 2 Enhancement**:
- Light theme (default, current design)
- Dark theme (inverted color palette)
- High-contrast theme (accessibility)

**Implementation**:
```csharp
public enum AppTheme { Light, Dark, HighContrast }

public void ApplyTheme(AppTheme theme)
{
    var themeStyles = theme switch
    {
        AppTheme.Light => new Uri("avares://AisToXmlRouteConvertor/Assets/Styles/LightTheme.axaml"),
        AppTheme.Dark => new Uri("avares://AisToXmlRouteConvertor/Assets/Styles/DarkTheme.axaml"),
        AppTheme.HighContrast => new Uri("avares://AisToXmlRouteConvertor/Assets/Styles/HighContrastTheme.axaml"),
        _ => throw new ArgumentException("Unknown theme")
    };
    
    Application.Current.Styles.Add(new StyleInclude(themeStyles));
}
```

## 9. Accessibility Features

### 9.1 Screen Reader Support

**ARIA Labels**:
- All interactive controls have descriptive labels
- Table columns have header announcements
- Progress indicators announce state changes

**Implementation**:
```xml
<Button Content="Browse..."
        Command="{Binding BrowseInputFolderCommand}"
        AutomationProperties.Name="Browse for input folder"
        AutomationProperties.HelpText="Opens a folder picker dialog to select the root input folder" />
```

### 9.2 Keyboard Accessibility

**Requirements**:
- All functions accessible via keyboard (no mouse-only features)
- Logical tab order through all controls
- Visible focus indicators
- Standard shortcuts (Ctrl+O, Ctrl+S, Ctrl+P)

**Focus Indicators**:
```xml
<Style Selector="Button:focus">
    <Setter Property="BorderBrush" Value="#0078D4" />
    <Setter Property="BorderThickness" Value="2" />
</Style>
```

### 9.3 Color Contrast

**WCAG AA Compliance**:
- Text contrast ratio: Minimum 4.5:1 for body text
- Large text (14pt bold+): Minimum 3:1
- Interactive elements: 3:1 against adjacent colors

**Contrast Ratios**:
- Dark grey on white (`#333333` on `#FFFFFF`): 12.6:1 ✓
- Red error text (`#E81123` on white): 5.0:1 ✓
- Blue button (`#0078D4`) with white text: 4.5:1 ✓

## 10. Dialogs and Modals

### 10.1 Success Dialog

**Trigger**: After successful XML export

**Content**:
- Title: "Success"
- Message: "Track generated successfully: {filename}"
- Button: "OK"

**Layout**:
```csharp
await MessageBox.Show(
    owner: this,
    message: $"Track generated successfully:\n\n{Path.GetFileName(outputPath)}",
    title: "Success",
    buttons: MessageBoxButtons.Ok,
    icon: MessageBoxIcon.Information);
```

### 10.2 Error Dialog

**Trigger**: Processing failure

**Content**:
- Title: "Error"
- Message: "Processing failed: {error details}"
- Button: "OK"

**Layout**:
```csharp
await MessageBox.Show(
    owner: this,
    message: $"Processing failed:\n\n{errorMessage}",
    title: "Error",
    buttons: MessageBoxButtons.Ok,
    icon: MessageBoxIcon.Error);
```

### 10.3 Folder Picker Dialog

**Trigger**: Browse button clicks

**OS-Native Dialogs**:
- Windows: FolderBrowserDialog
- macOS: NSOpenPanel
- Linux: GTK file chooser

**Implementation**:
```csharp
var dialog = new OpenFolderDialog
{
    Title = "Select Input Folder",
    Directory = _lastUsedPath ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
};

string? result = await dialog.ShowAsync(this);
if (!string.IsNullOrEmpty(result))
{
    InputFolder = result;
    await ScanVesselsAsync();
}
```

## 11. Summary

The AisToXmlRouteConvertor application layout uses a single-window design with vertically stacked sections guiding users through a linear workflow. All controls remain visible to provide clear workflow context and minimize navigation complexity. The responsive design adapts to different window sizes with scrolling support for small screens and table expansion for large displays. Reusable components ensure consistency across sections, while accessibility features (keyboard navigation, screen reader support, color contrast) make the application usable for all users. The UI emphasizes clarity with realistic examples (vessel "Alice's Container Ship", MMSI 205196000) and actionable error messages for both positive usage paths and negative error scenarios.
