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
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using System.Net.Http;
using System.Xml.Linq;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GPS
{
    public sealed partial class Coordinates : Page
    {
        public Coordinates()
        {
            this.InitializeComponent();
        }

        private async void onSearchClicked(object sender, RoutedEventArgs e)
        {
            string apiKey = GeographicalData.BingKey;
            string encodedAddress = Uri.EscapeDataString(SelectedAddress.Text);
            string url = $"http://dev.virtualearth.net/REST/v1/Locations?q={encodedAddress}&key={apiKey}&output=xml";
            
            var listonosz = new HttpClient();
            var result = await listonosz.GetAsync(url);

            if (result != null && result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                var contentXML = XDocument.Parse(content);
                
                XNamespace ns = "http://schemas.microsoft.com/search/local/ws/rest/v1";

                var adres = contentXML.Descendants(ns + "Name").FirstOrDefault().Value;
                var adresLat = contentXML.Descendants(ns + "Latitude").FirstOrDefault().Value;
                var adresLOng = contentXML.Descendants(ns + "Longitude").FirstOrDefault().Value;
                GeographicalData.EndPoint = new BasicGeoposition()
                {
                    Latitude = double.Parse(adresLat),
                    Longitude = double.Parse(adresLOng)
                };
                GeographicalData.EndPointDescription = adres;
                GeographicalLength.Text = adresLat;
                GeographicalWidth.Text = adresLOng;
            }
        }

        private void onBackClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void locateMeOnMap()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            Geoposition geoposition = await geolocator.GetGeopositionAsync();
            CurrentCoordinates.Text = "Latitude: " + geoposition.Coordinate.Point.Position.Latitude.ToString(".###") + "\nLongtitude: " + geoposition.Coordinate.Point.Position.Longitude.ToString(".###");

            GeographicalData.StartingPoint = new BasicGeoposition()
            {
                Latitude = geoposition.Coordinate.Point.Position.Latitude,
                Longitude = geoposition.Coordinate.Point.Position.Longitude
            };
        }

        private void onCoordinatesGridLoaded(object sender, RoutedEventArgs e)
        {
            locateMeOnMap();
        }
    }
}
