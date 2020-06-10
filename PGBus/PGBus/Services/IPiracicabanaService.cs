using PGBus.Models;
using System.Collections.Generic;

namespace PGBus.Services
{
    public interface IPiracicabanaService
    {
        List<Vehicle> LoadVehicles(string lineId);
        BusStopAndRoute LoadBusStopsAndRoutes(string lineId);
    }
}
