using FuelStationWebApi.Data;
using FuelStationWebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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
            // Запись действий в журнал с использованием пакета Serilog:
            // сообщения выводятся в консоль и записываются в текстовый файл (для каждых суток создается новый файл)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.Console()
                .WriteTo.File("FuelStationWebApiLog-.txt", rollingInterval:
                    RollingInterval.Day)
                .CreateLogger();


            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Подключение Serilog к узлу приложения: через него проходят и сообщения самой платформы ASP.NET Core.
            // Стандартные поставщики журнала при этом заменяются, поэтому уровни сообщений задаются настройкой выше,
            // а раздел Logging файла appsettings.json при использовании Serilog не действовал бы и потому в нем отсутствует.
            builder.Host.UseSerilog();

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

            // Политика CORS "AllowAll", которая подключается ниже через app.UseCors("AllowAll").
            // Разрешает запросы с любых источников, любыми методами и заголовками (удобно для учебного примера);
            // в реальном приложении список источников следует ограничить через WithOrigins(...).
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

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

                // Требование применять схему "Bearer" ко всем методам. В Microsoft.OpenApi 2.x ссылка на схему
                // создается из документа OpenAPI, поэтому требование задается функцией, получающей документ.
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });


            var app = builder.Build();
            // Страница сведений об исключениях (UseDeveloperExceptionPage) здесь не подключается:
            // платформа включает ее сама, но только в среде Development. В остальных средах
            // подробности ошибок клиенту не показываются, они записываются в журнал.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FuelStation API V1");
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            //Инициализация баз данных: выполняется один раз при запуске, до начала приема запросов.
            //Ошибка инициализации одной из баз записывается в журнал и не прерывает запуск службы.
            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                try
                {
                    // роли и учетная запись администратора; метод асинхронный, а Main - нет,
                    // поэтому при запуске дожидаемся его завершения
                    DbUserInitializer.Initialize(serviceProvider).GetAwaiter().GetResult();
                }
                catch (Exception exception)
                {
                    Log.Fatal(exception, "An error occurred while users db initialization");
                }
                try
                {
                    FuelsContext context = serviceProvider.GetRequiredService<FuelsContext>();
                    DbInitializer.Initialize(context);
                }
                catch (Exception exception)
                {
                    Log.Fatal(exception, "An error occurred while fuels db initialization");
                }
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