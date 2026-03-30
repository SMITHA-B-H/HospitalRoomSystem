using HospitalRoomAPI.Data;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================
// DATABASE CONFIGURATION
// =============================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// =============================
// DEPENDENCY INJECTION (CLEAN)
// =============================

// Repositories
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IDisplayRepository, DisplayRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();

// Services
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IDisplayService, DisplayService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<SignalRService>();

// =============================
// SWAGGER + SIGNALR
// =============================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddSignalR();

// =============================
// JWT AUTHENTICATION
// =============================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
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

// =============================
// CORS
// =============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            origin.StartsWith("http://localhost") ||
            origin.StartsWith("http://192.168.1.5"))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // ✅ Needed for SignalR
    });
});

var app = builder.Build();

// =============================
// MIDDLEWARE PIPELINE
// =============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // ✅ before routing

app.UseCors("AllowLocalhost");

app.UseHttpsRedirection();

app.UseAuthentication();   // ✅ FIRST
app.UseAuthorization();    // ✅ SECOND

app.MapControllers();
app.MapHub<RoomHub>("/roomHub");

app.Urls.Add("http://0.0.0.0:5285");

app.Run();

public partial class Program { }