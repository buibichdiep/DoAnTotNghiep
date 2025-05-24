using Booking.EF;
using Booking.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Booking.Services
{
    public static class CloudinaryService
    {
        public static async Task<string> UploadImageAsync(Cloudinary cloudinary, List<IFormFile> files)
        {
            try
            {
                string urlImages = string.Empty;

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file == null || file.Length == 0)
                            return string.Empty;

                        var fileName = ConvertModel.RemoveDiacriticsAndSpaces(
                                        Path.GetFileNameWithoutExtension(file.FileName));

                        using (var stream = file.OpenReadStream())
                        {
                            var uploadParams = new ImageUploadParams
                            {
                                File = new FileDescription(fileName, stream),
                                PublicId = fileName + "_" + DateTime.UtcNow.Ticks,
                                Folder = "Booking"
                            };

                            var uploadResult = await cloudinary.UploadAsync(uploadParams);

                            // Trả về URL công khai của hình ảnh đã tải lên
                            var result = uploadResult.SecureUrl.ToString();

                            return result;
                        }
                    }
                }

                return urlImages;
            }
            catch (Exception)
            {
                // Xử lý lỗi tải lên hình ảnh
                return string.Empty;
            }
        }
    }
}
