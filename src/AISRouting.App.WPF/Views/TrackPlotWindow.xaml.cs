using System.Windows;
using AISRouting.App.WPF.ViewModels;

namespace AISRouting.App.WPF.Views
{
    public partial class TrackPlotWindow : Window
    {
        public TrackPlotWindow()
        {
            InitializeComponent();
        }

        public void InitializeWithViewModel(TrackPlotViewModel vm)
        {
            DataContext = vm;
            vm.AttachPlot(PlotControl);
        }
    }
}
