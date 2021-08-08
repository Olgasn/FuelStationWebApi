using FuelStationWebApi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace FuelStationWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Sqlite
            services.AddDbContext<FuelsContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("FuelSqlite")));
            //SQL Server
            //services.AddDbContext<CouncilDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("CouncilConnectionSQL")));
            //MySQL
            //services.AddDbContext<CouncilDbContext>(options =>
            //    options.UseMySQL(Configuration.GetConnectionString("CouncilConnectionMysql")));

            // Add framework services.
            services.AddControllers();
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "FuelStation API",
                    Description = "Данные об операциях на топливной базе",
                    //TermsOfService = new Uri("https://go.microsoft.com/fwlink/?LinkID=206977"),
                    Contact = new OpenApiContact
                    {
                        Name = "Asenchik Oleg",
                        Email = string.Empty,
                        Url = new Uri("https://github.com/Olgasn/FuelStationWebApi")
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, FuelsContext context)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FuelStation API V1");
            });


            app.UseDeveloperExceptionPage();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // инициализация базы данных
            DbInitializer.Initialize(context);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
