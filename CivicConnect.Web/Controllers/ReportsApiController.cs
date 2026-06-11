using CivicConnect.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CivicConnect.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsApiController : ControllerBase
    {
        private readonly IPhotoService _photoService;

        public ReportsApiController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // Chuyển đổi IFormFile thành Stream để truyền cho tầng Core
            using var stream = file.OpenReadStream();
            var result = await _photoService.AddPhotoAsync(stream, file.FileName);

            return Ok(new
            {
                Url = result.Url,
                PublicId = result.PublicId,
                Message = "Done! Click link below to see optimized version of the image. Check the size and the format."
            });
        }
    }
}
