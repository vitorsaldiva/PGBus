using MvvmHelpers;
using Newtonsoft.Json;
using PGBus.Models;
using PGBus.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Bindings;

namespace PGBus.ViewModels
{
    public class MapPageViewModel : BaseViewModel
    {
        public static Xamarin.Forms.GoogleMaps.Map map;
        private static PiracicabanaService _service { get; set; } = new PiracicabanaService();
        public Task Initialization { get; private set; }

        Location OriginCoordinates { get; set; }

        private MapSpan _visibleRegion;
        public MapSpan VisibleRegion
        {
            get => _visibleRegion;
            set { SetProperty(ref _visibleRegion, value); }
        }

        private ObservableCollection<Pin> _pins;
        public ObservableCollection<Pin> Pins
        {
            get => _pins;
            set
            {
                _pins = value;
                OnPropertyChanged("Pins");
            }
        }

        public MoveToRegionRequest MoveToRegionRequest { get; } = new MoveToRegionRequest();

        public Command GetActualUserLocationCommand { get { return new Command(async () => await OnCenterMap(OriginCoordinates)); } }


        public MapPageViewModel()
        {
            Initialization = InitializeAsync();
        }

        protected async Task<Location> GetActualUserLocation()
        {
            try
            {
                IsBusy = true;

                await Task.Yield();
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5000));
                return await Geolocation.GetLocationAsync(request);

            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Unable to get actual location", "Ok");
                return new Location();
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task OnCenterMap(Location location)
        {
            if (location != null)
               VisibleRegion = MapSpan.FromCenterAndRadius(
                                    new Position(location.Latitude, location.Longitude),
                                        Distance.FromMeters(50));

            MoveToRegionRequest.MoveToRegion(VisibleRegion);
        }

        protected async Task<ObservableCollection<Pin>> LoadVehicles()
        {
            //linha 94BF
            var idLinha = "388c39b7dc234533438d6026b2b5e182f9bd4408";

            //var vehiclesJson = @"{'prefixo':'2801','lat':-24.011008,'lng':-46.413548, 'sentido':2, 'conteudo':'<span><b>Prefixo:</b> 2801</br><b>Linha: </b>94BF<br><b>Sentido: </b>VOLTA<br><b>Horário: </b>20/08/2019 23:51:56<br></span>'}
            //                ,{'prefixo':'2802','lat':-24.00462,'lng':-46.41322, 'sentido':1, 'conteudo':'<span><b>Prefixo:</b> 2802</br><b>Linha: </b>94BF<br><b>Sentido: </b>IDA<br><b>Horário: </b>20/08/2019 23:51:55<br></span>'}";

            //vehiclesJson = vehiclesJson.Insert(0, "[").Insert((vehiclesJson.Length + 1), "]");

            //var vehicles = JsonConvert.DeserializeObject<List<Vehicle>>(vehiclesJson);

            var vehicles = _service.LoadVehicles(idLinha);

            var listVehicles = new ObservableRangeCollection<Pin>();

            vehicles.ForEach(v =>
            {
                Pin vehicle = new Pin()
                {

                    Type = PinType.Generic,
                    Position = new Position(v.Lat, v.Lng),
                    ZIndex = 13,
                    Label = v.Prefixo,
                    //Icon = BitmapDescriptorFactory.FromBundle(@"bus_stop.png")

                };

                listVehicles.Add(vehicle);
            });

            return listVehicles;
        }

        protected async Task<ObservableCollection<Pin>> LoadBusStops()
        {
            //linha 94BF
            var idLinha = "388c39b7dc234533438d6026b2b5e182f9bd4408";

            var pontos = _service.LoadBusStops(idLinha);

            var listBusStops = new ObservableCollection<Pin>();

            pontos.ForEach(p =>
            {
                listBusStops.Add(new Pin()
                {
                    Type = PinType.Place,
                    Position = new Position(p.Lat, p.Lng),
                    ZIndex = 13,
                    Label = p.Codigo,
                    Icon = BitmapDescriptorFactory.FromBundle(@"bus_stop.png")
                });
            });

            return listBusStops;
        }


        protected async Task InitializeAsync()
        {
            OriginCoordinates = await GetActualUserLocation();
            OnCenterMap(OriginCoordinates);

            foreach (var pin in await LoadBusStops())
            {
                Pins.Add(pin);
            }

            foreach (var busPin in await LoadVehicles())
            {
                Pins.Add(busPin);
            }
        }


    }
}
