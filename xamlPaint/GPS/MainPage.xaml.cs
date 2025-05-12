using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Maps;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GPS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void onZoomInClicked(object sender, RoutedEventArgs e)
        {
            AppMap.ZoomLevel++;
            if (AppMap.ZoomLevel > 20)
                AppMap.ZoomLevel = 20;
        }
        private void onZoomOutClicked(object sender, RoutedEventArgs e)
        {
            AppMap.ZoomLevel--;
            if (AppMap.ZoomLevel < 1)
                AppMap.ZoomLevel = 1;
        }
        private void onSateliteClicked(object sender, RoutedEventArgs e)
        {
            var bt = sender as AppBarButton;
            if (AppMap.Style == MapStyle.AerialWithRoads)
            {
                AppMap.Style = MapStyle.Road;
                SateliteView.Label = "Satelita";
                (bt.Icon as FontIcon).Glyph = "S";
            }
            else
            {
                AppMap.Style = MapStyle.AerialWithRoads;
                SateliteView.Label = "Mapa";
                (bt.Icon as FontIcon).Glyph = "M";
            }
        }
        private void onCoodrinatesClicked(object sender, RoutedEventArgs e)
        {
            //Handle Coodrinates button click
            Frame.Navigate(typeof(Coordinates));
        }
    }

    
}
