
using Microsoft.EntityFrameworkCore;
using RealTimeSensorTrack.Data;
using RealTimeSensorTrack.Hubs;
using RealTimeSensorTrack.Services;

namespace RealTimeSensorTrack
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add Entity Framework
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add SignalR
            builder.Services.AddSignalR();

            // Add custom services
            builder.Services.AddScoped<IInMemoryDataService, InMemoryDataService>();
            builder.Services.AddScoped<IAnomalyDetectionService, AnomalyDetectionService>();
            builder.Services.AddScoped<IDataPurgeService, DataPurgeService>();

            // Add background services
            builder.Services.AddHostedService<SensorSimulationService>();
            builder.Services.AddHostedService<DataPurgeService>();

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
                
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Only use HTTPS redirection in production
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("AllowReactApp");
            app.UseStaticFiles();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<SensorHub>("/sensorHub");
            
            // Serve the dashboard
            app.MapGet("/", () => Results.Redirect("/index.html"));

            // Ensure database is created and seeded
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var seeder = new DatabaseSeeder(context);
                await seeder.SeedAsync();
            }

            await app.RunAsync();
        }
    }
}
