using Microsoft.AspNetCore.Builder;
using FuelStationWebApi.Data;

namespace FuelStationWebApi.Middleware
{
    public static class DbInitializerExtensions
    {
        public static IApplicationBuilder UseDbInitializer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DbInitializerMiddleware>();
        }

    }
}
