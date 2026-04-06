using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;

public class GoogleDriveService : IFileStorageService
{
    private readonly DriveService _driveService;

    public GoogleDriveService()
    {
        UserCredential credential;

        using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { DriveService.Scope.Drive },
                "user",
                CancellationToken.None,
                new FileDataStore("token.json", true)
            ).Result;
        }

        _driveService = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "HospitalApp"
        });
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new Exception("File is empty");

        var fileMeta = new Google.Apis.Drive.v3.Data.File()
        {
            Name = Guid.NewGuid() + Path.GetExtension(file.FileName)
        };

        using var stream = file.OpenReadStream();

        var request = _driveService.Files.Create(fileMeta, stream, file.ContentType);
        request.Fields = "id";

        var result = await request.UploadAsync();

        if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
        {
            var error = result.Exception?.Message ?? "Unknown error";
            throw new Exception($"Upload failed: {error}");
        }

        var uploadedFile = request.ResponseBody;

        // Make file public
        await _driveService.Permissions.Create(new Google.Apis.Drive.v3.Data.Permission
        {
            Type = "anyone",
            Role = "reader"
        }, uploadedFile.Id).ExecuteAsync();

        return $"https://drive.google.com/uc?id={uploadedFile.Id}";
    }

    public async Task DeleteAsync(string fileId)
    {
        await _driveService.Files.Delete(fileId).ExecuteAsync();
    }
}