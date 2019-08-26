using PGBus.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PGBus.Services
{
    public interface IPiracicabanaService
    {
        List<Vehicle> LoadVehicles(string lineId);
    }
}
