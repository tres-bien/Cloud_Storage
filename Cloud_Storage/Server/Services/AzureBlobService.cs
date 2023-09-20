using Azure.Storage;
using Azure.Storage.Sas;
using Azure.Storage.Blobs;

namespace CloudService.Services
{
    public class AzureBlobService : IAzureBlobService
    {
        private readonly string _storageAccount = "tresbienblob";
        private readonly string _accessKey = "2Nzto2cu59r0/haG/zfLcFEynP59+5ySFUcwByNflGSRbCYWZtTDiLFQz3PlzkYOEsiDhBwZkseq+ASttjF6/Q==";
        private readonly BlobContainerClient _filesContainer;

        public AzureBlobService()
        {
            var credantial = new StorageSharedKeyCredential(_storageAccount, _accessKey);
            var blobUri = $"https://{_storageAccount}.blob.core.windows.net";
            var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credantial);
            _filesContainer = blobServiceClient.GetBlobContainerClient("cloudstoragecontainer");
        }

        public async Task<List<BlobDto>> ListAsync()
        {
            List<BlobDto> files = new List<BlobDto>();

            await foreach (var file in _filesContainer.GetBlobsAsync())
            {
                string sasToken = GenerateSasTokenForBlob(file.Name, BlobSasPermissions.Read);
                string blobUrlWithSas = $"{_filesContainer.Uri}/{file.Name}?{sasToken}";
                files.Add(new BlobDto
                {
                    Uri = blobUrlWithSas,
                    Name = file.Name,
                    ContentType = file.Properties.ContentType
                });
            }
            return files;
        }

        public async Task<BlobResponceDto> UploadAsync(IFormFile blob)
        {
            BlobResponceDto responce = new();
            BlobClient client = _filesContainer.GetBlobClient(blob.FileName);

            await using (Stream? data = blob.OpenReadStream())
            {
                await client.UploadAsync(data);
            }

            responce.Status = $"File {blob.FileName} Uploaded Successfully";
            responce.Error = false;
            responce.Blob.Uri = client.Uri.AbsoluteUri;
            responce.Blob.Name = client.Name;
            responce.Blob.Date = DateTime.UtcNow;
            responce.Blob.FileSize = blob.Length;

            return responce;
        }

        public async Task<BlobDto?> DownloadAsync(string blobFilename)
        {
            BlobClient file = _filesContainer.GetBlobClient(blobFilename);

            if (await file.ExistsAsync())
            {
                var data = await file.OpenReadAsync();
                Stream blobContent = data;

                var content = await file.DownloadContentAsync();

                string name = blobFilename;
                string contentType = content.Value.Details.ContentType;

                return new BlobDto { Content = blobContent, Name = name, ContentType = contentType };
            }
            return null;
        }

        public async Task<BlobResponceDto> DeleteAsync(string blobFilename)
        {
            BlobClient file = _filesContainer.GetBlobClient(blobFilename);

            await file.DeleteAsync();

            return new BlobResponceDto { Error = false, Status = $"File: {blobFilename} has been successfuly deleted." };
        }

        private string GenerateSasTokenForBlob(string blobName, BlobSasPermissions permissions)
        {
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = _filesContainer.Name,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(permissions);

            return sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_storageAccount, _accessKey)).ToString();
        }
    }
}
