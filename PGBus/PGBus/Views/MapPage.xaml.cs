﻿using PGBus.ViewModels;
using System.Reflection;
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

    }
}