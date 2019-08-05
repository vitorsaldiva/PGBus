using PGBus.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace PGBus.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        Location OriginCoordinates { get; set; }

        public MapPage ()
		{
			InitializeComponent ();
            BindingContext = new MapPageViewModel();

            map.MyLocationEnabled = true;
            map.UiSettings.MyLocationButtonEnabled = false;
            map.UiSettings.ZoomControlsEnabled = false;
            map.UiSettings.ZoomGesturesEnabled = true; 
            GetActualUserLocation();
            StylingMap();

        }

        private void StylingMap()
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream($"PGBus.Assets.mapstyle.json");
            string styleFile;
            using (var reader = new System.IO.StreamReader(stream))
            {
                styleFile = reader.ReadToEnd();
            }

            map.MapStyle = MapStyle.FromJson(styleFile); 
        }

        async Task OnCenterMap(Location location)
        {
            map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(location.Latitude, location.Longitude), Distance.FromMeters(50))); 

            //LoadBuses(location);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected async Task GetActualUserLocation()
        {
            try
            {
                await Task.Yield();
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5000));
                OriginCoordinates = await Geolocation.GetLocationAsync(request);

                if (OriginCoordinates != null)
                {
                    await OnCenterMap(OriginCoordinates);
                }
            }
            catch (Exception ex)
            {
                //await UserDialogs.Instance.AlertAsync("Error", "Unable to get actual location", "Ok");
            }
        }

        private async void MyLocation_Clicked(object sender, EventArgs e)
        {
            await OnCenterMap(OriginCoordinates);
        }
    }
}