using FuelStationWebApi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Reflection;

namespace FuelStationWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Запись действий в журнал с использованием пакета Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.File("FuelStationWebApiLog-.txt", rollingInterval:
                    RollingInterval.Day)
                .CreateLogger();


            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;

            //Sqlite
            string connectionString = builder.Configuration.GetConnectionString("FuelSqlite");
            services.AddDbContext<FuelsContext>(options =>
                options.UseSqlite(connectionString));

            //SQL Server
            //connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            ////Вариант строки подключения к экземпляру удаленного SQL Server, требующего имя пользователя и пароль
            //// создаем конфигурацию для считывания секретной информации
            //IConfigurationRoot configuration = builder.Configuration.AddUserSecrets<Program>().Build();
            //connectionString = configuration.GetConnectionString("RemoteSQLConnection");
            ////Считываем пароль и имя пользователя из secrets.json
            //string secretPass = configuration["Database:password"];
            //string secretUser = configuration["Database:login"];
            //SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString)
            //{
            //    Password = secretPass,
            //    UserID = secretUser
            //};
            //connectionString = sqlConnectionStringBuilder.ConnectionString;

            //services.AddDbContext<FuelsContext>(options =>
            //    options.UseSqlServer(connectionString));

            //MySQL
            //connectionString = builder.Configuration.GetConnectionString("FuelConnectionMysql");
            //services.AddDbContext<FuelsContext>(options =>
            //    options.UseMySQL(connectionString));

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




            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FuelStation API V1");
            });

            app.UseDeveloperExceptionPage();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            //Инициализация базы данных
            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                try
                {
                    FuelsContext context = serviceProvider.GetRequiredService<FuelsContext>();
                    DbInitializer.Initialize(context);
                }
                catch (Exception exception)
                {
                    Log.Fatal(exception, "An error occurred while db initialization");
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();


            app.Run();

        }
    }
}