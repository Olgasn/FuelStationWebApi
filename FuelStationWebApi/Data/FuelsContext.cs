using FuelStationWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FuelStationWebApi.Data
{
    public class FuelsContext : DbContext
    {
        public FuelsContext(DbContextOptions<FuelsContext> options) : base(options)
        {
        }
        public DbSet<Fuel> Fuels { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Tank> Tanks { get; set; }
    }
}
