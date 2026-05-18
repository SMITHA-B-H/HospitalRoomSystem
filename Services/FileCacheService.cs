namespace HospitalRoomAPI.Services
{
    public class FileCacheService
    {
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        public FileCacheService()
        {
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        public async Task<string> GetOrDownloadFile(string fileId)
        {
            // Try multiple extensions
            var possibleFiles = Directory.GetFiles(_uploadPath, $"{fileId}.*");
            if (possibleFiles.Length > 0)
                return possibleFiles[0];

            var url = $"https://drive.google.com/uc?export=download&id={fileId}";

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to download file");

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "";

            // ?? Detect extension
            string ext = contentType switch
            {
                "video/mp4" => ".mp4",
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                _ => ".bin"
            };

            var filePath = Path.Combine(_uploadPath, $"{fileId}{ext}");

            var bytes = await response.Content.ReadAsByteArrayAsync();
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);

            return filePath;
        }
    }
}