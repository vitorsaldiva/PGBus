using MvvmHelpers;
using Newtonsoft.Json;
using PGBus.Models;
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
        public MapPage()
        {
            InitializeComponent();

            map.UiSettings.ZoomGesturesEnabled = true;
            map.UiSettings.MyLocationButtonEnabled = false;
            map.UiSettings.ZoomControlsEnabled = false;
            StylingMap();

            BindingContext = new MapPageViewModel();
            MapPageViewModel.map = map;
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

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selected = e.Item as BusStopDescription;

            MessagingCenter.Send(new Message { Value = selected.LineId }, "LineSelected");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }    
        
    }
}