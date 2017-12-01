using FuelStationWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FuelStationWebApi.ViewModels
{
    public class OperationsViewModel
    {
        IEnumerable<OperationViewModel> Operations { get; set; }
        IEnumerable<Tank> Tanks { get; set; }

    }
}
