using MvvmHelpers;
using Newtonsoft.Json;
using PGBus.Models;
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
            var vehiclesJson = @"{'prefixo':'2801','lat':-24.011008,'lng':-46.413548, 'sentido':2, 'conteudo':'<span><b>Prefixo:</b> 2801</br><b>Linha: </b>94BF<br><b>Sentido: </b>VOLTA<br><b>Horário: </b>20/08/2019 23:51:56<br></span>'}
                            ,{'prefixo':'2802','lat':-24.00462,'lng':-46.41322, 'sentido':1, 'conteudo':'<span><b>Prefixo:</b> 2802</br><b>Linha: </b>94BF<br><b>Sentido: </b>IDA<br><b>Horário: </b>20/08/2019 23:51:55<br></span>'}";

            vehiclesJson = vehiclesJson.Insert(0, "[").Insert((vehiclesJson.Length + 1), "]");

            var vehicles = JsonConvert.DeserializeObject<List<Vehicle>>(vehiclesJson);

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
            var stopsJson = @"[
                                    {'ponto':'4659','lat':-23.999152,'lng':-46.414534,'sentido':1,'conteudo':'<b>1400 - AV. TRABALHADORES (TERM. TUDE BASTOS)</b>'},
                                    {'ponto':'4748','lat':-24.001501,'lng':-46.41271,'sentido':1,'conteudo':'<b>7318 - AV. PRESIDENTE COSTA E SILVA,1341</b>'},
                                    {'ponto':'4773','lat':-24.003198,'lng':-46.413029,'sentido':1,'conteudo':'<b>7877 - AV. PRESIDENTE COSTA E SILVA, 1319 </b>'},
                                    {'ponto':'4563','lat':-24.005891,'lng':-46.413491,'sentido':1,'conteudo':'<b>1257 - AV. PRESIDENTE COSTA E SILVA, 1163</b>'},
                                    {'ponto':'4565','lat':-24.007507,'lng':-46.413661,'sentido':1,'conteudo':'<b>1259 - AV. PRESIDENTE COSTA E SILVA, 843</b>'},
                                    {'ponto':'4566','lat':-24.010129,'lng':-46.414273,'sentido':1,'conteudo':'<b>1260 - AV. PRESIDENTE COSTA E SILVA, 699</b>'},
                                    {'ponto':'4747','lat':-24.013193,'lng':-46.414752,'sentido':1,'conteudo':'<b>7317 - AV. PRESIDENTE COSTA E SILVA, S/N</b>'},
                                    {'ponto':'4567','lat':-24.012578,'lng':-46.413853,'sentido':1,'conteudo':'<b>1261 - RUA PERNAMBUCO, 50</b>'},
                                    {'ponto':'4568','lat':-24.011008,'lng':-46.413548,'sentido':1,'conteudo':'<b>1262 - RUA PERNAMBUCO, 300</b>'},
                                    {'ponto':'4451','lat':-24.011021,'lng':-46.410018,'sentido':1,'conteudo':'<b>1120 - RUA JAÚ, 695</b>'},
                                    {'ponto':'4450','lat':-24.011216,'lng':-46.407549,'sentido':1,'conteudo':'<b>1119 - RUA JAÚ, 297</b>'},
                                    {'ponto':'4619','lat':-24.010021,'lng':-46.406556,'sentido':1,'conteudo':'<b>1321 - RUA RIO BRANCO, 537</b>'},
                                    {'ponto':'4813','lat':-24.008697,'lng':-46.405187,'sentido':1,'conteudo':'<b>9897 - AV. MARECHAL MALLET, 1071</b>'},
                                    {'ponto':'4811','lat':-24.010636,'lng':-46.40348,'sentido':1,'conteudo':'<b>9895 - AV. MARECHAL MALLET, 701</b>'},
                                    {'ponto':'4693','lat':-24.010109,'lng':-46.40223,'sentido':1,'conteudo':'<b>1436 - RUA XIXOVA, 562</b>'},
                                    {'ponto':'4694','lat':-24.007923,'lng':-46.401797,'sentido':1,'conteudo':'<b>1437 - RUA XIXOVA, 824</b>'},
                                    {'ponto':'4441','lat':-24.007624,'lng':-46.400718,'sentido':2,'conteudo':'<b>1108 - RUA CYNTHIA GILFFRIDA, 1000</b>'},
                                    {'ponto':'4440','lat':-24.007957,'lng':-46.399106,'sentido':2,'conteudo':'<b>1107 - RUA CYNTHIA GILFFRIDA, 642</b>'},
                                    {'ponto':'4439','lat':-24.009423,'lng':-46.396377,'sentido':2,'conteudo':'<b>1105 - RUA CYNTHIA GILFFRIDA, 301</b>'},
                                    {'ponto':'4607','lat':-24.011946,'lng':-46.397863,'sentido':2,'conteudo':'<b>1307 - RUA COSTA MACHADO, 553</b>'},
                                    {'ponto':'4606','lat':-24.014403,'lng':-46.399584,'sentido':2,'conteudo':'<b>1306 - RUA COSTA MACHADO, 266</b>'},
                                    {'ponto':'4809','lat':-24.012232,'lng':-46.401757,'sentido':2,'conteudo':'<b>9893 - AV. MARECHAL MALLET, 504</b>'},
                                    {'ponto':'4812','lat':-24.009811,'lng':-46.403897,'sentido':2,'conteudo':'<b>9896 - AV. MARECHAL MALLET, S/N (PRAÇA C. SAITTA)</b>'},
                                    {'ponto':'4814','lat':-24.008464,'lng':-46.405055,'sentido':2,'conteudo':'<b>9898 - AV. MARECHAL MALLET, 938</b>'},
                                    {'ponto':'4713','lat':-24.009588,'lng':-46.406589,'sentido':2,'conteudo':'<b>6197 - AV. RIO BRANCO, 528 </b>'},
                                    {'ponto':'4714','lat':-24.0117,'lng':-46.406827,'sentido':2,'conteudo':'<b>6198 - AV. RIO BRANCO, 191</b>'},
                                    {'ponto':'4715','lat':-24.012506,'lng':-46.40882,'sentido':2,'conteudo':'<b>6199 - RUA BAHIA, 408 </b>'},
                                    {'ponto':'4716','lat':-24.012281,'lng':-46.411665,'sentido':2,'conteudo':'<b>6200 - RUA BAHIA, 680</b>'},
                                    {'ponto':'4717','lat':-24.012119,'lng':-46.413494,'sentido':2,'conteudo':'<b>6201 - RUA BAHIA, 848</b>'},
                                    {'ponto':'4568','lat':-24.011008,'lng':-46.413548,'sentido':2,'conteudo':'<b>1262 - RUA PERNAMBUCO, 300</b>'},
                                    {'ponto':'4569','lat':-24.008477,'lng':-46.412963,'sentido':2,'conteudo':'<b>1263 - RUA PERNAMBUCO, 550</b>'},
                                    {'ponto':'4564','lat':-24.005006,'lng':-46.413124,'sentido':2,'conteudo':'<b>1258 - AV. PRESIDENTE COSTA E SILVA, S/N</b>'},
                                    {'ponto':'4562','lat':-24.001006,'lng':-46.412468,'sentido':2,'conteudo':'<b>1256 - AV. PRESIDENTE COSTA E SILVA, S/N</b>'},
                                    {'ponto':'4759','lat':-23.998386,'lng':-46.414144,'sentido':2,'conteudo':'<b>7503 - TERMINAL TUDE BASTOS</b>'}]";

            var pontos = JsonConvert.DeserializeObject<List<BusStop>>(stopsJson);

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
        }


    }
}
