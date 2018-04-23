using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace FuelStationWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //.UseUrls("http://192.168.1.37:80")            
                .UseStartup<Startup>()
                .Build();
    }
}
