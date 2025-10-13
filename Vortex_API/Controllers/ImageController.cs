using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ImageController : ControllerBase
    {
        private readonly IImageRepository _imageRepo;

        public ImageController(IImageRepository imageRepo)
        {
            _imageRepo = imageRepo;
        }

        // Lấy danh sách ảnh theo productId
        [HttpGet("get-image-by/{productId}")]
        //[Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var images = await _imageRepo.GetByProductIdAsync(productId);
            return Ok(images);
        }

        //  Upload 1 ảnh cho sản phẩm
        [HttpPost("upload-image-by-id/{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImage(int productId, [FromForm] ImageUploadDTO dto)
        {
            var image = await _imageRepo.UploadImage(productId, dto);
            if (image == null)
                return NotFound("Product not found");

            return Ok(image);
        }

        // Xóa ảnh theo ID
        [HttpDelete("delete-image-by-id/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var deleted = await _imageRepo.DeleteImage(imageId);
            if (!deleted)
                return NotFound("Image not found");

            return Ok(new { Message = "Image deleted successfully" });
        }
    }
}
