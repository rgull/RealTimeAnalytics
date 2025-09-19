
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
            builder.Services.AddScoped<IAlertService, AlertService>();

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
                
                // Add alert columns to Sensors table if they don't exist
                try
                {
                    await context.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sensors') AND name = 'WarningThreshold')
                            ALTER TABLE Sensors ADD WarningThreshold FLOAT NULL;
                        
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sensors') AND name = 'CriticalThreshold')
                            ALTER TABLE Sensors ADD CriticalThreshold FLOAT NULL;
                        
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sensors') AND name = 'MinThreshold')
                            ALTER TABLE Sensors ADD MinThreshold FLOAT NULL;
                        
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sensors') AND name = 'AlertEnabled')
                            ALTER TABLE Sensors ADD AlertEnabled BIT NOT NULL DEFAULT 1;
                    ");

                    // Create Alerts table if it doesn't exist
                    await context.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Alerts' AND xtype='U')
                        BEGIN
                            CREATE TABLE Alerts (
                                Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                                SensorId INT NOT NULL,
                                Message NVARCHAR(200) NOT NULL,
                                Severity NVARCHAR(50) NOT NULL,
                                ThresholdValue FLOAT NULL,
                                ActualValue FLOAT NULL,
                                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                                IsResolved BIT NOT NULL DEFAULT 0,
                                ResolvedAt DATETIME2 NULL,
                                FOREIGN KEY (SensorId) REFERENCES Sensors(Id)
                            );

                            CREATE INDEX IX_Alerts_SensorId ON Alerts(SensorId);
                            CREATE INDEX IX_Alerts_CreatedAt ON Alerts(CreatedAt);
                            CREATE INDEX IX_Alerts_Severity ON Alerts(Severity);
                            CREATE INDEX IX_Alerts_IsResolved ON Alerts(IsResolved);
                        END
                    ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not add alert columns: {ex.Message}");
                }
                
                var seeder = new DatabaseSeeder(context);
                await seeder.SeedAsync();
            }

            await app.RunAsync();
        }
    }
}
