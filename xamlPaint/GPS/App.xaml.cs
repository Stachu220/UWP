using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace GPS
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    LoadState();
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            SaveState();
            deferral.Complete();
        }

        private void SaveState()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            var composite = new Windows.Storage.ApplicationDataCompositeValue
            {
                ["StartLat"] = GeographicalData.StartingPoint.Latitude,
                ["StartLon"] = GeographicalData.StartingPoint.Longitude,
                ["StartAlt"] = GeographicalData.StartingPoint.Altitude,

                ["EndLat"] = GeographicalData.EndPoint.Latitude,
                ["EndLon"] = GeographicalData.EndPoint.Longitude,
                ["EndAlt"] = GeographicalData.EndPoint.Altitude,

                ["EndPointDescription"] = GeographicalData.EndPointDescription,
                ["MapVersion"] = MainPage._AppMapStyle.ToString()
            };

            localSettings.Values["AppState"] = composite;
        }


        private void LoadState()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.TryGetValue("AppState", out object compositeObject) &&
                compositeObject is ApplicationDataCompositeValue composite)
            {
                if (composite.TryGetValue("StartLat", out object startLat) &&
                    composite.TryGetValue("StartLon", out object startLon) &&
                    composite.TryGetValue("StartAlt", out object startAlt))
                {
                    GeographicalData.StartingPoint = new Windows.Devices.Geolocation.BasicGeoposition
                    {
                        Latitude = (double)startLat,
                        Longitude = (double)startLon,
                        Altitude = (double)startAlt
                    };
                }

                if (composite.TryGetValue("EndLat", out object endLat) &&
                    composite.TryGetValue("EndLon", out object endLon) &&
                    composite.TryGetValue("EndAlt", out object endAlt))
                {
                    GeographicalData.EndPoint = new Windows.Devices.Geolocation.BasicGeoposition
                    {
                        Latitude = (double)endLat,
                        Longitude = (double)endLon,
                        Altitude = (double)endAlt
                    };
                }

                if (composite.TryGetValue("EndPointDescription", out object endPointDescription))
                    GeographicalData.EndPointDescription = endPointDescription.ToString();

                if (composite.TryGetValue("MapVersion", out object mapVersionStr))
                {
                    if (Enum.TryParse(mapVersionStr.ToString(), out Windows.UI.Xaml.Controls.Maps.MapStyle parsedStyle))
                        MainPage._AppMapStyle = parsedStyle;
                }
            }
        }

    }
}
