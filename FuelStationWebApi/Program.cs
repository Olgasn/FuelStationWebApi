using FuelStationWebApi.Data;
using FuelStationWebApi.Middleware;
using FuelStationWebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Reflection;
using System.Text;

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
            var configuration = builder.Configuration;
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


            // Подключение Identity для хранения данных пользователей
            string connectionUsersString = configuration.GetConnectionString("IdentityConnection");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionUsersString));
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();



            // Настройка JWT аутентификации
            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            builder.Services.AddTransient<AuthService>();

            // Add framework services.
            services.AddControllers();



            // Register the Swagger generator
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "FuelStation API",
                    Description = "Данные об операциях на топливной базе",
                    Contact = new OpenApiContact
                    {
                        Name = "Asenchik Oleg",
                        Email = string.Empty,
                        Url = new Uri("https://github.com/Olgasn/FuelStationWebApi")
                    }
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter your JWT token in this field",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                c.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                };
                c.AddSecurityRequirement(securityRequirement);
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

            //Инициализация баз данных
            app.UseDbInitializer();

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