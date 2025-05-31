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
using Windows.Services.Maps;
using Windows.Devices.Geolocation;
using BingMapsRESTToolkit;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Core;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GPS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MapStyle _AppMapStyle;
        public MainPage()
        {
            this.InitializeComponent();
            GeographicalData.BingKey = AppMap.MapServiceToken;
            if (_AppMapStyle != null)
            {
                var bt = SateliteView;
                AppMap.Style = _AppMapStyle;
                if (AppMap.Style == MapStyle.Road)
                {
                    SateliteView.Label = "Satelita";
                    (bt.Icon as FontIcon).Glyph = "S";
                }
                else
                {
                    SateliteView.Label = "Mapa";
                    (bt.Icon as FontIcon).Glyph = "M";
                }
            }
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
            _AppMapStyle = AppMap.Style;
            
        }
        private void onCoodrinatesClicked(object sender, RoutedEventArgs e)
        {
            //Handle Coodrinates button click
            Frame.Navigate(typeof(Coordinates));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (GeographicalData.EndPointDescription == null)
                return;

            try
            {
                var pointerStart = new MapIcon()
                {
                    Location = new Geopoint(GeographicalData.StartingPoint),
                    Title = "Here you are"
                };
                AppMap.MapElements.Add(pointerStart);

                var pointerEnd = new MapIcon()
                {
                    Location = new Geopoint(GeographicalData.EndPoint),
                    Title = GeographicalData.EndPointDescription
                };

                AppMap.MapElements.Add(pointerEnd);

                MapPolyline polyline = new MapPolyline()
                {
                    StrokeColor = Windows.UI.Colors.Black,
                    StrokeThickness = 3,
                    StrokeDashed = true,
                    Path = new Geopath(new List<BasicGeoposition>
                    {
                        GeographicalData.StartingPoint,
                        GeographicalData.EndPoint
                    })
                };

                AppMap.MapElements.Add(polyline);

            

                AppMap.TrySetViewAsync(pointerStart.Location, 8);

            
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.ToString());
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Error",
                    Content = "An error occurred while setting up the map: " + ex.Message,
                    PrimaryButtonText = "OK"
                };
                contentDialog.PrimaryButtonClick += (s, args) => { CoreApplication.Exit(); };
                contentDialog.ShowAsync();
            }
            Trasa();

            base.OnNavigatedTo(e);
        }

        private async void Trasa()
        {
            var routeRequest = new RouteRequest()
            {
                BingMapsKey = GeographicalData.BingKey,
                Culture = "pl",
                Waypoints = new List<SimpleWaypoint> 
                { 
                    new SimpleWaypoint(GeographicalData.StartingPoint.Latitude, GeographicalData.StartingPoint.Longitude),
                    new SimpleWaypoint(GeographicalData.EndPoint.Latitude, GeographicalData.EndPoint.Longitude)
                },
                RouteOptions = new RouteOptions()
                {
                    RouteAttributes = new List<RouteAttributeType>
                    {
                        RouteAttributeType.RoutePath                    
                    }
                }
            };
            var response = await ServiceManager.GetResponseAsync(routeRequest);
            if (response != null && response.ResourceSets != null && response.ResourceSets.Length > 0)
            {
                Route route = (Route)response.ResourceSets[0].Resources[0];
                if (route.RoutePath.Line.Coordinates != null)
                {
                    var path = route.RoutePath.Line.Coordinates.Select(coord => new BasicGeoposition
                    {
                        Latitude = coord[0],
                        Longitude = coord[1]
                    }).ToList();

                    var geopath = new Geopath(path);

                    var polyline = new MapPolyline
                    {
                        Path = geopath,
                        StrokeColor = Windows.UI.Colors.Blue,
                        StrokeThickness = 5,
                        StrokeDashed = false
                    };

                    AppMap.MapElements.Add(polyline);

                    var northwest = new BasicGeoposition { Latitude = route.BoundingBox[2], Longitude = route.BoundingBox[1] }; // North, West
                    var southeast = new BasicGeoposition { Latitude = route.BoundingBox[0], Longitude = route.BoundingBox[3] }; // South, East

                    var box = new GeoboundingBox(northwest, southeast);
                    var border = new Thickness(50, 50, 50, 50);

                    await AppMap.TrySetViewBoundsAsync(
                        box,
                        border,
                        MapAnimationKind.Default
                    );


                    var content = route.RouteLegs[0].ItineraryItems
                        .Take(5)
                        .Select(item => item.Instruction.Text)
                        .ToList();

                    var contentText = string.Join("\n", content);

                    ContentDialog contentDialog = new ContentDialog()
                    {
                        Title = "Distance: " + route.TravelDistance + " " + route.DistanceUnit,
                        Content = "Travel Time: " + Math.Floor(route.TravelDuration/3600) + "h " + Math.Round((route.TravelDuration%3600)/60) + "min " + "\n" + contentText,
                        PrimaryButtonText = "OK"
                    };
                    await contentDialog.ShowAsync();

                }
            }
        }
    }
}
