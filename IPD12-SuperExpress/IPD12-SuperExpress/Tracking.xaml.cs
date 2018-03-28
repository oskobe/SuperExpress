using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IPD12_SuperExpress
{
    /// <summary>
    /// Interaction logic for Tracking.xaml
    /// </summary>
    public partial class Tracking : Window
    {
        public Tracking()
        {
            InitializeComponent();
        }

        private void btnTracking_Click(object sender, RoutedEventArgs e)
        {
            MapPolyline polyline = new MapPolyline();
            polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
            polyline.StrokeThickness = 5;
            polyline.Opacity = 0.7;
            polyline.Locations = new LocationCollection() {
        new Location(47.6424, -122.3219),
        new Location(47.8424,-122.1747),
        new Location(47.67856,-122.130994)};

            myMap.Children.Add(polyline);
        }
    }
}
