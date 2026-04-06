using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace HospitalRoomAPI.Middleware
{
    public class LicenseMiddleware
    {
        private readonly RequestDelegate _next;

        public LicenseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, Services.ILicenseService licenseService)
        {
            var path = context.Request.Path.Value?.ToLower();

            // ? Allow public endpoints (VERY IMPORTANT)
            if (IsPublicEndpoint(path))
            {
                await _next(context);
                return;
            }

            bool isValid;

            try
            {
                isValid = await licenseService.ValidateAsync();
            }
            catch (Exception)
            {
                // ?? If license validation fails unexpectedly
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("License validation error.");
                return;
            }

            if (!isValid)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("System not licensed.");
                return;
            }

            await _next(context);
        }

        // ================= HELPER =================
        private bool IsPublicEndpoint(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return true;

            return
                path.Contains("/swagger") ||

                // ? AUTH APIs (must be allowed)
                path.Contains("/api/auth/login") ||
                path.Contains("/api/auth/register-hospital") ||
                path.Contains("/api/auth/forgot-password") ||
                path.Contains("/api/auth/reset-password") ||

                // ? Health check (optional)
                path.Contains("/health") ||

                // ? Static files (if any)
                path.Contains("/uploads") ||

                // ? Root / default
                path == "/";
        }
    }
}