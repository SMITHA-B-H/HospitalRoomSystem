using System.Management;
using System.Security.Cryptography;
using System.Text;

public static class HardwareHelper
{
    public static string GetMachineId()
    {
        string cpuId = "";
        string boardId = "";

        using (var searcher = new ManagementObjectSearcher("select ProcessorId from Win32_Processor"))
        {
            foreach (var item in searcher.Get())
            {
                cpuId = item["ProcessorId"]?.ToString();
                break;
            }
        }

        using (var searcher = new ManagementObjectSearcher("select SerialNumber from Win32_BaseBoard"))
        {
            foreach (var item in searcher.Get())
            {
                boardId = item["SerialNumber"]?.ToString();
                break;
            }
        }

        var raw = $"{cpuId}-{boardId}";

        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));

        return Convert.ToBase64String(hash);
    }
}