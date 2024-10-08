﻿using FuelStationWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FuelStationWebApi.Data
{
    public class FuelsContext: DbContext
    {
        public FuelsContext(DbContextOptions<FuelsContext> options) : base(options)
        {
        }
        public FuelsContext()
        {
        }
        public virtual DbSet<Fuel> Fuels { get; set; }
        public virtual DbSet<Operation> Operations { get; set; }
        public virtual DbSet<Tank> Tanks { get; set; }
    }
}
