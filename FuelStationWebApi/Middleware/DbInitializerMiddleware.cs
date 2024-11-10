using FuelStationWebApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FuelStationWebApi.Middleware
{
    public class DbInitializerMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public Task Invoke(HttpContext context)
        {

                DbUserInitializer.Initialize(context).Wait();
                DbInitializer.Initialize(context.RequestServices.GetRequiredService<FuelsContext>());
 

            // Вызов следующего делегата / компонента middleware в конвейере
            return _next.Invoke(context);
        }
    }

}
