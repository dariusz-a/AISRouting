using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot;
using ScottPlot.WPF;

namespace AISRouting.App.WPF.ViewModels
{
    public partial class TrackPlotViewModel : ObservableObject
    {
        private WpfPlot? _plotControl;
        private object? _initialScatter;
        private object? _optimizedScatter;

        public void SetData(double[] initialLons, double[] initialLats, double[] optimizedLons, double[] optimizedLats, string title)
        {
            if (_plotControl == null)
                return;

            var plt = _plotControl.Plot;
            plt.Clear();

            plt.Title(title);
            plt.XLabel("Longitude (deg)");
            plt.YLabel("Latitude (deg)");
            // Convert input coordinates from radians back to degrees using: degrees = radians / PI * 180
            double[] ToDegrees(double[] arr) => arr == null ? Array.Empty<double>() : arr.Select(r => r / Math.PI * 180.0).ToArray();

            var optimizedLonsDeg = ToDegrees(optimizedLons);
            var optimizedLatsDeg = ToDegrees(optimizedLats);
            try
            {
                var legend = plt.Legend;
                legend?.GetType().GetProperty("IsVisible")?.SetValue(legend, true);
            }
            catch { }

            _initialScatter = plt.Add.Scatter(initialLons, initialLats);
            // set appearance via reflection to avoid compile-time dependency on plottable types
            try
            {
                var tInit = _initialScatter!.GetType();
                tInit.GetProperty("Color")?.SetValue(_initialScatter, ScottPlot.Colors.LightBlue);
                tInit.GetProperty("MarkerSize")?.SetValue(_initialScatter, 3);
                tInit.GetProperty("Label")?.SetValue(_initialScatter, $"Initial ({initialLons.Length} pts)");
            }
            catch { }

            _optimizedScatter = plt.Add.Scatter(optimizedLonsDeg, optimizedLatsDeg);
            try
            {
                var tOpt = _optimizedScatter!.GetType();
                tOpt.GetProperty("Color")?.SetValue(_optimizedScatter, ScottPlot.Colors.Red);
                tOpt.GetProperty("MarkerSize")?.SetValue(_optimizedScatter, 8);
                tOpt.GetProperty("Label")?.SetValue(_optimizedScatter, $"Optimized ({optimizedLons.Length} pts)");
            }
            catch { }

            // Enable pan/zoom is handled by WpfPlot defaults; just refresh
            _plotControl.Refresh();
        }

        public void AttachPlot(WpfPlot plot)
        {
            _plotControl = plot;
        }
    }
}
