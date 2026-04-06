public interface IFileStorageService
{
	Task<string> UploadAsync(IFormFile file);
	Task DeleteAsync(string fileId);
}