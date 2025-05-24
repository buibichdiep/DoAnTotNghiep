using Booking.Utilities;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

namespace Booking.Models
{
    public class ConvertModel
    {
        public static string ConvertSlug(string title)
        {
            if (string.IsNullOrEmpty(title)) return string.Empty;

            var slug = AppUtilities.GenerateSlug(title) + ".html";

            return slug;
        }

        public static string RemoveDiacriticsAndSpaces(string input)
        {
            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && c != ' ')
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string ConvertUserName(string input)
        {
            var name = RemoveDiacriticsAndSpaces(input.ToLowerInvariant());

            Random random = new Random();

            var fullName = name + random.Next(1, 10000).ToString();

            return fullName;
        }

        public static IFormFile ConvertFileToIFormFile(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, fileBytes.Length, "file", Path.GetFileName(filePath))
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream" // Bạn có thể điều chỉnh loại MIME nếu cần
            };

            return formFile;
        }

        public static string ConvertStringToMoney(string value)
        {
            var money = string.Empty;
            if (decimal.TryParse(value, out decimal price))
            {
                money = string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:C0}", price);
            }
            return money;
        }

        public static string ConvertStringToDatetime(string value)
        {
            string formattedDateTime = string.Empty;
            // Chuyển đổi chuỗi thành DateTime
            if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm",
                                       CultureInfo.InvariantCulture, DateTimeStyles.None,
                                       out DateTime dateTime))
            {
                // Lấy thứ trong tuần
                string dayOfWeek = dateTime.ToString("dddd", new CultureInfo("vi-VN"));
                // Viết hoa chữ cái đầu của thứ
                dayOfWeek = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dayOfWeek);

                // Định dạng ngày giờ theo yêu cầu
                formattedDateTime = dateTime.ToString($"HH:mm '{dayOfWeek}', dd/MM/yyyy", new CultureInfo("vi-VN"));
                
            }

            return formattedDateTime;
        }
    }
}
