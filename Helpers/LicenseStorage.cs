using System.IO;

namespace HospitalRoomAPI.Helpers
{
    public static class LicenseStorage
    {
        private static readonly string FolderPath =
            @"C:\ProgramData\HospitalRoomAPI";

        private static readonly string FilePath =
            Path.Combine(FolderPath, "license.lic");

        public static void SaveLicense(string license)
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            File.WriteAllText(FilePath, license);
        }

        public static string? GetLicense()
        {
            if (!File.Exists(FilePath))
                return null;

            return File.ReadAllText(FilePath);
        }
    }
}