using System.IO;
using System.Threading.Tasks;
using System;

namespace HospitalRoomAPI.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly string _licensePath = Path.Combine("App_Data", "license.lic");

        private static bool? _cachedResult;
        private static DateTime _lastChecked;

        public async Task<bool> ValidateAsync()
        {
            // Cache for 5 minutes
            if (_cachedResult.HasValue && (DateTime.UtcNow - _lastChecked).TotalMinutes < 5)
                return _cachedResult.Value;

            if (!File.Exists(_licensePath))
            {
                Cache(false);
                return false;
            }

            var content = await File.ReadAllTextAsync(_licensePath);

            // TODO:
            // - RSA signature validation
            // - Expiry check
            // - Machine ID match

            var result = !string.IsNullOrWhiteSpace(content);

            Cache(result);
            return result;
        }

        private void Cache(bool result)
        {
            _cachedResult = result;
            _lastChecked = DateTime.UtcNow;
        }
    }
}