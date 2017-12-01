using FuelStationWebApi.Models;
using FuelStationWebApi.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;

namespace FuelStationWebApi.Data
{
    public class FuelsContext: DbContext
    {
        public FuelsContext(DbContextOptions<FuelsContext> options): base(options)
        {
        }
        public DbSet<Fuel> Fuels { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Tank> Tanks { get; set; }
        //представление 
        public IEnumerable<OperationViewModel> OperationsViewModel { get; }
    }
}
