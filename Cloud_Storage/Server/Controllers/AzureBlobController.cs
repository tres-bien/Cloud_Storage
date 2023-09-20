using Microsoft.AspNetCore.Mvc;
using CloudService.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CloudService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureBlobController : ControllerBase
    {
        private readonly AzureBlobService _azureBlobService;

        public AzureBlobController(AzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService;
        }

        [HttpGet]
        public async Task<IActionResult> ListAllBlobs()
        {
            var result = await _azureBlobService.ListAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var result = await _azureBlobService.UploadAsync(file);
            return Ok(result);
        }

        [HttpGet]
        [Route("filename")]
        public async Task<IActionResult> Download(string filename)
        {
            var result = await _azureBlobService.DownloadAsync(filename);
            return File(result!.Content!, result.ContentType!, result.Name);
        }

        [HttpDelete]
        [Route("filename")]
        public async Task<IActionResult> Delete(string filename)
        {
            var result = await _azureBlobService.DeleteAsync(filename);
            return Ok(result);
        }
    }
}
