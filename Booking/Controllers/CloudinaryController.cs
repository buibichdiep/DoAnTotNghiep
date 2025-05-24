using Booking.EF;
using Booking.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Net;

namespace Booking.Controllers
{
    public class CloudinaryController : Controller
    {
        private readonly ILogger<CloudinaryController> _logger;
        private readonly AppDbContext _context;
        private readonly Cloudinary _cloudinary;

        public CloudinaryController
            (
                ILogger<CloudinaryController> logger,
                AppDbContext context,
                Cloudinary cloudinary
            )
        {
            _logger = logger;
            _context = context;
            _cloudinary = cloudinary;
        }

        [NonAction]
        public async Task<string> UploadMedia(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return string.Empty;

                var fileName = ConvertModel.RemoveDiacriticsAndSpaces(Path.GetFileNameWithoutExtension(file.FileName));

                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(fileName, stream),
                        PublicId = fileName + "_" + DateTime.UtcNow.Ticks,
                        Folder = "Booking"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.StatusCode == HttpStatusCode.OK)
                        return uploadResult.SecureUrl.ToString();
                }
                return string.Empty;
            }
            catch (Exception)
            {
                // Xử lý lỗi tải lên hình ảnh
                return string.Empty;
            }

        }

        [NonAction]
        public async Task<IActionResult> UploadMediaAsync(List<IFormFile> files)
        {
            try
            {
                var uploadTasks = files.Select(async file =>
                {
                    if (file == null || file.Length == 0)
                        return null;

                    var fileName = ConvertModel.RemoveDiacriticsAndSpaces(Path.GetFileNameWithoutExtension(file.FileName));

                    using (var stream = file.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(fileName, stream),
                            PublicId = fileName + "_" + DateTime.UtcNow.Ticks,
                            Folder = "Booking"
                        };

                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                        if (uploadResult.StatusCode == HttpStatusCode.OK)
                        {
                            var media = new Media
                            {
                                Id = Guid.NewGuid(),
                                PublicId = uploadResult.PublicId,
                                MediaUrl = uploadResult.SecureUrl.ToString(),
                                MediaType = GetFileType(file)
                            };

                            _context.Medias.Add(media);

                            return new
                            {
                                Id = media.Id,
                                Url = media.MediaUrl,
                                Type = media.MediaType
                            };
                        }

                        _logger.LogError($"Error: Not found");
                        return null;
                    }
                }).ToList();

                var result = await Task.WhenAll(uploadTasks);
                await _context.SaveChangesAsync();

                return Json(result);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi tải lên hình ảnh
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest();
            }
        }

        [NonAction]
        public async Task<IActionResult> DeleteMediaAsync(List<string> listPublicId)
        {
            try
            {
                // Tạo danh sách các tác vụ để xóa ảnh trên Cloudinary
                var deleteImageTasks = listPublicId.Select(async publicId =>
                {
                    if (string.IsNullOrEmpty(publicId))
                        return new { publicId = string.Empty, result = "invalid" };

                    var deletionParams = new DeletionParams(publicId);
                    var result = await _cloudinary.DestroyAsync(deletionParams);

                    return new { publicId, result = result.Result };
                }).ToList();

                // Chờ tất cả các thao tác xóa ảnh trên Cloudinary hoàn thành
                var deleteResults = await Task.WhenAll(deleteImageTasks);

                // Duyệt qua kết quả và xử lý xóa trong DbContext
                foreach (var deleteResult in deleteResults)
                {
                    var publicId = deleteResult.publicId;
                    var result = deleteResult.result;

                    if (result == "ok" || result == "not found")
                    {
                        var media = await _context.Medias.FirstOrDefaultAsync(m => m.PublicId == publicId);
                        if (media != null)
                        {
                            _context.Medias.Remove(media);
                        }
                    }
                }

                // Sau khi tất cả các thay đổi được thực hiện, lưu vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                return Json(deleteResults);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest();
            }
        }

        private string GetFileType(IFormFile file)
        {
            // Kiểm tra MIME type
            string mimeType = file.ContentType;

            if (mimeType.StartsWith("image/"))
            {
                return "Image";
            }
            else if (mimeType.StartsWith("video/"))
            {
                return "Video";
            }

            // Kiểm tra phần mở rộng nếu MIME type không có
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" }.Contains(extension))
            {
                return "Image";
            }
            else if (new[] { ".mp4", ".avi", ".mov", ".mkv", ".webm" }.Contains(extension))
            {
                return "Video";
            }

            return "Unknown";
        }
    }
}
