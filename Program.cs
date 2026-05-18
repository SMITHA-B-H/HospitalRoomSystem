using HospitalRoomAPI.Data;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Hubs;
using HospitalRoomAPI.Middleware;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;

using System.Text;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// CONFIG
// =====================================================
var storagePath = builder.Configuration["StoragePath"];

// fallback if not set in appsettings.json
if (string.IsNullOrWhiteSpace(storagePath))
{
    storagePath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Uploads"
    );
}

// auto create folder if missing
Directory.CreateDirectory(storagePath);

// =====================================================
// DATABASE
// =====================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// =====================================================
// CONTROLLERS + JSON
// =====================================================
builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// =====================================================
// DEPENDENCY INJECTION - REPOSITORIES
// =====================================================
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IDisplayRepository, DisplayRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IFloorRepository, FloorRepository>();
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();

// =====================================================
// DEPENDENCY INJECTION - SERVICES
// =====================================================
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IDisplayService, DisplayService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IFloorService, FloorService>();
builder.Services.AddScoped<IQueueService, QueueService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();

builder.Services.AddScoped<SignalRService>();

builder.Services.AddSingleton<FileCacheService>();

builder.Services.AddHostedService<AnnouncementCleanupService>();

// =====================================================
// KESTREL / PORT
// =====================================================
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

// =====================================================
// SWAGGER
// =====================================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HospitalRoomAPI",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    });
});

// =====================================================
// SIGNALR
// =====================================================
builder.Services.AddSignalR();

// =====================================================
// JWT AUTH
// =====================================================
var jwtSection = builder.Configuration.GetSection("Jwt");

var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

// Program.cs

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,

            // ✅ Disable token expiry check
            ValidateLifetime = false,

            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(key)
        };
});

// =====================================================
// CORS
// =====================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllDynamic", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// =====================================================
// BUILD APP
// =====================================================
var app = builder.Build();

// =====================================================
// DEVELOPMENT TOOLS
// =====================================================
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

        app.UseSwagger();
        app.UseSwaggerUI();

// =====================================================
// PIPELINE
// =====================================================

// CORS FIRST
app.UseCors("AllowAllDynamic");

// ROUTING
app.UseRouting();

// =====================================================
// STATIC FILES (NO WWWROOT REQUIRED)
// Serves files from StoragePath => /files
// =====================================================
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/files"
});

// =====================================================
// WEBSOCKETS / SIGNALR
// =====================================================
app.UseWebSockets();

// =====================================================
// AUTH
// =====================================================
app.UseAuthentication();
app.UseAuthorization();

// =====================================================
// CUSTOM MIDDLEWARE
// =====================================================
app.UseMiddleware<LicenseMiddleware>();

// =====================================================
// ENDPOINTS
// =====================================================
app.MapControllers();

app.MapHub<RoomHub>("/roomHub");

// =====================================================
// lan port 
// =====================================================
app.Urls.Add("http://0.0.0.0:5000");


// =====================================================
// RUN
// =====================================================
app.Run();

public partial class Program { }