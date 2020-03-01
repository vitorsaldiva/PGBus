using MvvmHelpers;
using Newtonsoft.Json;
using PGBus.Models;
using PGBus.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Bindings;
using Xamarin.Forms.Internals;

namespace PGBus.ViewModels
{
    public class MapPageViewModel : BaseViewModel
    {
        Location OriginCoordinates { get; set; }

        public Task Initialization { get; private set; }
        public static Xamarin.Forms.GoogleMaps.Map map;
        private static PiracicabanaService _service { get; set; } = new PiracicabanaService();

        

        private PageStatusEnum _pageStatusEnum;
        public PageStatusEnum PageStatusEnum
        {
            get => _pageStatusEnum;
            set { SetProperty(ref _pageStatusEnum, value); }
        }

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

        private List<BusStopDescription> _items;
        public List<BusStopDescription> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged("Items");
            }
        }

        public MoveToRegionRequest MoveToRegionRequest { get; } = new MoveToRegionRequest();

        public Command GetActualUserLocationCommand { get { return new Command(async () => await OnCenterMap(OriginCoordinates)); } }

        public Command ChangePageStatusCommand
        {
            get
            {
                return new Command<PageStatusEnum>((param) =>
                {
                    PageStatusEnum = param;
                });
            }
        }

        public Command CloseLinesPageCommand
        {
            get
            {
                return new Command(() =>
                {
                    PageStatusEnum = PageStatusEnum.Default;
                });
            }
        }


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
                                         Distance.FromKilometers(2));

            MoveToRegionRequest.MoveToRegion(VisibleRegion);
        }

        protected async Task<ObservableCollection<Pin>> LoadVehicles()
        {
            //linha 94BF
            string idLinha =
                _service.LoadLinesId().Where(l => l.Code.Equals("CBS")).FirstOrDefault()?.LineId;

            var vehicles = _service.LoadVehicles(idLinha);

            var listVehicles = new ObservableRangeCollection<Pin>();

            vehicles.ForEach(v =>
            {
                Pin vehicle = new Pin()
                {

                    Type = PinType.Generic,
                    Position = new Position(v.Lat, v.Lng),
                    ZIndex = 15,
                    Label = v.Prefixo,
                    Icon = BitmapDescriptorFactory.FromBundle(@"bus.png")

                };

                listVehicles.Add(vehicle);
            });

            return listVehicles;
        }

        protected async Task<ObservableCollection<Pin>> LoadBusStops()
        {
            //linha 94BF
            string idLinha =
                _service.LoadLinesId().Where(l => l.Code.Equals("CBS")).FirstOrDefault()?.LineId;


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

            Device.StartTimer(TimeSpan.FromSeconds(16), () =>
            {
                var pinsToRemove = map.Pins.Where(p => p.Type.Equals(PinType.Generic)).ToList();
                var vehicles = LoadVehicles().Result;

                if (vehicles.Count > 0)
                {
                    foreach (var pin in pinsToRemove)
                    {
                        map.Pins.Remove(pin);
                    }

                    vehicles.ForEach(p => map.Pins.Add(p));
                }

                return true;
            });

            Items = _service.LoadLinesId();

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
