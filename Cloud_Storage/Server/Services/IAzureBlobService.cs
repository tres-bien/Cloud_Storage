namespace CloudService.Services
{
    public interface IAzureBlobService
    {
        Task<List<BlobDto>> ListAsync();
        Task<BlobResponceDto> UploadAsync(IFormFile blob);
    }
}