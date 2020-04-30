using MvvmHelpers;
using PGBus.MapCustomization;
using PGBus.Models;
using PGBus.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
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
        public Task Initialization { get; private set; }

        public static Xamarin.Forms.GoogleMaps.Map map;

        private static PiracicabanaService _service { get; set; } = new PiracicabanaService();

        private Location OriginCoordinates { get; set; }

        private BusStopAndRoute BusStopAndRoute { get; set; }

        public string SelectedLineId { get; set; }


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
                    VehicleSelected = string.Empty;
                });
            }
        }

        public Command<PinClickedEventArgs> PinClickedCommand
        {
            get
            {
                return new Command<PinClickedEventArgs>(async (args) => await PinClicked(args));
            }
        }

        protected string VehicleSelected;

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

        protected async Task<ObservableCollection<Pin>> LoadVehicles(string lineId)
        {
            if (string.IsNullOrEmpty(lineId))
                return new ObservableCollection<Pin>();

            string idLinha =
                _service.LoadLinesId().Where(l => l.LineId.Equals(lineId))
                .FirstOrDefault()?.LineId;

            var vehicles = _service.LoadVehicles(idLinha);

            var listVehicles = new ObservableRangeCollection<Pin>();

            vehicles.ForEach(v =>
            {
                Pin vehicle = new Pin()
                {
                    Anchor = new Point(0.5, 0.5),
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

            BusStopAndRoute = _service.LoadBusStopsAndRoutes(idLinha);

            var listBusStops = new ObservableCollection<Pin>();
            BusStopAndRoute?.BusStops?.ForEach(p =>
            {
                var busStop = new Pin()
                {
                    Anchor = new Point(0.5, 0.5),
                    Type = PinType.Place,
                    Position = new Position(p.Lat, p.Lng),
                    ZIndex = 13,
                    Label = p.Codigo,
                    Icon = BitmapDescriptorFactory.FromBundle(@"bus_stop.png"),
                    Tag = new PinAdditionalInfo { Sentido = p.Sentido.ToString() }
                };
                listBusStops.Add(busStop);
            });

            var allRoutes = BusStopAndRoute.RotaVolta.Union(BusStopAndRoute.RotaIda);
            AddPolylineToMap(allRoutes.ToList());

            return listBusStops;
        }

        protected async Task InitializeAsync()
        {
            OriginCoordinates = await GetActualUserLocation();
            OnCenterMap(OriginCoordinates);

            Device.StartTimer(TimeSpan.FromSeconds(16), () =>
            {
                var pinsToRemove = map.Pins.Where(p => p?.Type == (PinType.Generic)).ToList();

                var vehicles = LoadVehicles(SelectedLineId).Result;

                if (vehicles.Count > 0)
                {
                    foreach (var pin in pinsToRemove)
                    {
                        map.Pins.Remove(pin);
                    }

                    if (!string.IsNullOrEmpty(VehicleSelected))
                    {
                        var vehicleSelected = vehicles.Where(p => p.Label.Equals(VehicleSelected)).FirstOrDefault();
                        AddPinsToMap(vehicleSelected);
                        UpdatePolylineMap(vehicleSelected, Polylines.SelectMany(p => p.Positions).ToList());
                    }
                    else
                        AddPinsToMap(vehicles);

                }

                return true;
            });

            MessagingCenter.Subscribe<Message>(this, "LineSelected", message =>
            {
                ClearPinsMap();
                ClearPolylines();

                PageStatusEnum = PageStatusEnum.Default;

                var pins = new List<Pin>();
                var tasks = new List<Task<ObservableCollection<Pin>>>();
                SelectedLineId = message?.Value;

                tasks.Add(LoadBusStops(message?.Value));
                tasks.Add(LoadVehicles(message?.Value));

                Task.WhenAll(tasks).Result.ForEach(task =>
                {
                    AddPinsToMap(task);
                });

            });

            Items = _service.LoadLinesId();
        }



        protected async Task PinClicked(PinClickedEventArgs pinClickedArgs)
        {
            if (pinClickedArgs.Pin.Type.Equals(PinType.Place))
            {
                ClearPolylines();

                var vehicles = Pins.Where(v => v?.Type == (PinType.Generic)).ToList();
                Pin closestVehicle;
                var busStopPin = pinClickedArgs.Pin;

                closestVehicle =
                    vehicles
                    .Where(v => ((PinAdditionalInfo)v.Tag).Sentido.Equals(((PinAdditionalInfo)pinClickedArgs.Pin.Tag).Sentido))
                    .OrderBy(v =>
                    Location.CalculateDistance(
                        v.Position.Latitude, v.Position.Longitude,
                        busStopPin.Position.Latitude, busStopPin.Position.Longitude, DistanceUnits.Kilometers))
                    .FirstOrDefault();

                if (closestVehicle != null)
                {
                    ClearPinsMap();
                    Pins.Add(busStopPin);
                    Pins.Add(closestVehicle);

                    VehicleSelected = closestVehicle.Label;

                    AddPolylineToMap(GetRouteToClosestVehicle(closestVehicle.Position, busStopPin.Position,
                        ((PinAdditionalInfo)busStopPin.Tag).Sentido == "1" ? BusStopAndRoute?.RotaIda : BusStopAndRoute?.RotaVolta));

                    //TODO: Recuperar informações de tempo restante para veiculo chegar ao local selecionado

                    //TODO: Verificar por que bound não está funcionando...
                    var bounds = new Bounds(busStopPin.Position, closestVehicle.Position);
                    map.MoveToRegion(MapSpan.FromBounds(bounds));
                }
                else
                    return;
            }

        }

        private IList<Position> GetRouteToClosestVehicle(Position from, Position to, List<Position> routes)
        {
            var vehicleRoute = new List<Position>();

            /*
             *  Utiliza a coordenada inicial (lat/lng veículo) para buscar, 
             *  dentro da lista de coordenadas da rota da linha, a coordenada 
             *  mais próxima para exibição no mapa
             */
            var initialRoutePoint =
                routes.OrderBy(p =>
                    Location.CalculateDistance(latitudeStart: from.Latitude, longitudeStart: from.Longitude,
                    latitudeEnd: p.Latitude, longitudeEnd: p.Longitude, DistanceUnits.Kilometers))
                .FirstOrDefault();

            /*
             *  Utiliza a coordenada final (lat/lng ponto do onibus) para buscar, 
             *  dentro da lista de coordenadas da rota da linha, a coordenada 
             *  mais próxima para exibição no mapa
             */
            var finalRoutePoint =
                routes.OrderBy(p =>
                    Location.CalculateDistance(latitudeStart: to.Latitude, longitudeStart: to.Longitude,
                    latitudeEnd: p.Latitude, longitudeEnd: p.Longitude, DistanceUnits.Kilometers))
                .FirstOrDefault();

            /*
             * Seleciona apenas as coordenadas que estejam entre o intervalo entre o ponto inicial e o final
             * para exibição do polyline
             */
            vehicleRoute = routes
                .Skip(routes.IndexOf(initialRoutePoint))
                .Take(routes.IndexOf(finalRoutePoint) - routes.IndexOf(initialRoutePoint) + 1)
                .ToList();

            // Inclui os pontos inicial e final para exibição correta do polyline
            vehicleRoute.Insert(0, from);
            vehicleRoute.Add(to);

            return vehicleRoute;
        }

        protected void AddPolylineToMap(IList<Position> positions)
        {
            ClearPolylines();
            var polyline = new Polyline
            {
                StrokeColor = Color.FromHex("e65c00"),
                StrokeWidth = 5
            };
            positions.ForEach(position =>
            {
                polyline.Positions.Add(position);
            });

            Polylines.Add(polyline);
        }

        protected void UpdatePolylineMap(Pin vehiclePoint, IList<Position> positions)
        {
            var polylinePositions = Polylines.SelectMany(p => p.Positions);
            var closestPoint =
                polylinePositions
                .OrderBy(p => Location.CalculateDistance(latitudeStart: vehiclePoint.Position.Latitude, longitudeStart: vehiclePoint.Position.Longitude,
                                                         latitudeEnd: p.Latitude, longitudeEnd: p.Longitude, DistanceUnits.Kilometers)).FirstOrDefault();

            var polylinePositionsList = polylinePositions.ToList();
            polylinePositionsList.RemoveRange(0, polylinePositionsList.IndexOf(closestPoint) - 1);

            AddPolylineToMap(polylinePositionsList);
        }

        private void ClearPolylines()
        {
            Polylines.Clear();
        }

        protected void AddPinsToMap(IList<Pin> pins)
        {
            pins.ForEach(pin =>
            {
                Pins.Add(pin);
            });
        }

        protected void AddPinsToMap(Pin pin)
        {
            Pins.Add(pin);
        }

        protected void ClearPinsMap()
        {
            Pins.Clear();
        }

    }
}
