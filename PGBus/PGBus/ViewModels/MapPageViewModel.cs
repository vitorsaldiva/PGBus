using MvvmHelpers;
using PGBus.MapCustomization;
using PGBus.Models;
using PGBus.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        public string SelectedLineId { get; set; }

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
                OnPropertyChanged(nameof(Pins));
            }
        }

        private ObservableCollection<Polyline> _polylines;
        public ObservableCollection<Polyline> Polylines
        {
            get => _polylines;
            set 
            { 
                _polylines = value;
                OnPropertyChanged(nameof(Polylines));
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

        public Command<PinClickedEventArgs> PinClickedClickedCommand
        {
            get
            {
                return new Command<PinClickedEventArgs>(async (args) => await PinClickedClicked(args));
            }
        }


        public MapPageViewModel()
        {
            Initialization = InitializeAsync();

            MessagingCenter.Subscribe<Message>(this, "LineSelected", message =>
            {
                var pins = new List<Pin>();
                var tasks = new List<Task<ObservableCollection<Pin>>>();
                SelectedLineId = message?.Value;


                tasks.Add(LoadBusStops(message?.Value));
                tasks.Add(LoadVehicles(message?.Value));

                ClearPinsMap();
                PageStatusEnum = PageStatusEnum.Default;


                Task.WhenAll(tasks).Result.ForEach(task =>
                {
                    AddPinsToMap(task);
                });

            });
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

        protected async Task<ObservableCollection<Pin>> LoadVehicles(string lineId)
        {
            string idLinha =
                _service.LoadLinesId().Where(l => l.LineId.Equals(lineId))
                .FirstOrDefault()?.LineId;

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
                    Icon = BitmapDescriptorFactory.FromBundle(@"bus.png"),
                    Tag = new PinAdditionalInfo { Sentido = v.Sentido },
                    Rotation = -30f
                };
                listVehicles.Add(vehicle);
            });

            return listVehicles;
        }

        protected async Task<ObservableCollection<Pin>> LoadBusStops(string lineId)
        {
            string idLinha =
                _service.LoadLinesId().Where(l => l.LineId.Equals(lineId))
                .FirstOrDefault()?.LineId;

            var pontos = _service.LoadBusStopsAndRoutes(idLinha);

            var listBusStops = new ObservableCollection<Pin>();
            pontos?.BusStops?.ForEach(p =>
            {
                var busStop = new Pin()
                {
                    Type = PinType.Place,
                    Position = new Position(p.Lat, p.Lng),
                    ZIndex = 13,
                    Label = p.Codigo,
                    Icon = BitmapDescriptorFactory.FromBundle(@"bus_stop.png"),
                    Tag = new PinAdditionalInfo { Sentido = p.Sentido.ToString() }
                };
                listBusStops.Add(busStop);
            });

            AddPolylineToMap(CustomPosition.ConvertToPosition(pontos.RotaIda));
            AddPolylineToMap(CustomPosition.ConvertToPosition(pontos.RotaVolta));

            return listBusStops;
        }

        protected async Task InitializeAsync()
        {
            OriginCoordinates = await GetActualUserLocation();
            OnCenterMap(OriginCoordinates);

            Device.StartTimer(TimeSpan.FromSeconds(16), () =>
            {
                var pinsToRemove = map.Pins.Where(p => p.Type.Equals(PinType.Generic)).ToList();
                var vehicles = LoadVehicles(SelectedLineId).Result;

                if (vehicles.Count > 0)
                {
                    foreach (var pin in pinsToRemove)
                    {
                        map.Pins.Remove(pin);
                    }

                    AddPinsToMap(vehicles);
                }

                return true;
            });

            Items = _service.LoadLinesId();
        }

        protected void AddPolylineToMap(IList<Position> positions)
        {
            var polyline = new Polyline();
            positions.ForEach(position => 
            {
                polyline.Positions.Add(position);
            });

            Polylines.Add(polyline);
        }

        protected void AddPinsToMap(IList<Pin> pins)
        {
            pins.ForEach(pin =>
            {
                Pins.Add(pin);
            });
        }

        protected void ClearPinsMap()
        {
            Pins.Clear();
        }

        protected async Task PinClickedClicked(PinClickedEventArgs pinClickedArgs)
        {
            if (pinClickedArgs.Pin.Type.Equals(PinType.Place))
            {
                var vehicles = Pins.Where(v => v.Type.Equals(PinType.Generic));
                Pin closestVehicle;

                closestVehicle = 
                    vehicles
                    .Where(v => ((PinAdditionalInfo)v.Tag).Sentido.Equals(((PinAdditionalInfo)pinClickedArgs.Pin.Tag).Sentido))
                    .OrderBy(v => Haversine(v.Position, pinClickedArgs.Pin.Position)).FirstOrDefault();

                Pins.Add(pinClickedArgs.Pin);

                var bounds = new Bounds(pinClickedArgs.Pin.Position, closestVehicle.Position);
                map.MoveToRegion(MapSpan.FromBounds(bounds));
            }

        }

        /// <summary>
        /// Função para calcular a distância entre duas coordenadas
        /// </summary>
        /// <param name="from">Latitude e Longitude do ponto de partida</param>
        /// <param name="to">Latitude e Longitude do ponto de destino</param>
        /// <returns></returns>
        protected double Haversine(Position from, Position to)
        {
            const double EarthRadius = 3958.756;

            double difference_lat = DegreesToRadians(to.Latitude - from.Latitude);
            double difference_lon = DegreesToRadians(to.Longitude - from.Longitude);

            double alpha = Math.Sin(difference_lat / 2) * Math.Sin(difference_lat / 2) +
                                Math.Cos(DegreesToRadians(from.Latitude)) *
                                Math.Cos(DegreesToRadians(to.Latitude)) *
                                Math.Sin(difference_lon / 2) * Math.Sin(difference_lon / 2);

            return 2 * Math.Atan2(Math.Sqrt(alpha), Math.Sqrt(1 - alpha)) * EarthRadius;
        }

        /// <summary>
        /// Função para conversão de graus em radianos
        /// </summary>
        /// <param name="degrees">Graus</param>
        /// <returns></returns>
        private double DegreesToRadians(double degrees)
        {
            return degrees / 180 * Math.PI;
        }

    }
}
