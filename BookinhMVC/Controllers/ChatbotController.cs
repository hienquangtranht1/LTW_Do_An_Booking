using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BookinhMVC.Models;

namespace BookinhMVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly BookingContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        // Nên lưu API key vào biến môi trường hoặc cấu hình bảo mật
        private readonly string _geminiApiKey = "AIzaSyAJA-Eqw-cgV1LYCFDyj1F_CEsl6H1f4hc";
        private readonly string _geminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=";

        public ChatbotController(BookingContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Ok(new { reply = "Tin nhắn không được để trống." });

            // 1. Xử lý truy vấn local (database)
            var localReply = await ProcessLocalQuery(message);
            if (!string.IsNullOrEmpty(localReply))
                return Ok(new { reply = localReply });

            // 2. Nếu không có, gọi Gemini AI
            var hospitalInfo = await GetHospitalInfo();
            var context = BuildContext(hospitalInfo);
            var fullMessage = context + "Câu hỏi: " + message;

            var geminiReply = await CallGeminiApi(fullMessage);
            return Ok(new { reply = CleanResponse(geminiReply) });
        }

        // Lấy thông tin bệnh viện, bác sĩ, khoa
        private async Task<HospitalInfo> GetHospitalInfo()
        {
            var doctors = await _context.BacSis
                .Include(bs => bs.Khoa)
                .Select(bs => new DoctorInfo
                {
                    Id = bs.MaBacSi,
                    Name = bs.HoTen,
                    Department = bs.Khoa != null ? bs.Khoa.TenKhoa : "Chưa xác định",
                    Description = bs.MoTa ?? "Không có mô tả",
                    Phone = bs.SoDienThoai ?? "Chưa cập nhật",
                    Email = bs.Email ?? "Chưa cập nhật"
                }).ToListAsync();

            var departments = await _context.Khoas
                .Select(k => new DepartmentInfo
                {
                    Id = k.MaKhoa,
                    Name = k.TenKhoa,
                    Description = k.MoTa ?? "Không có mô tả"
                }).ToListAsync();

            return new HospitalInfo
            {
                Doctors = doctors,
                Departments = departments,
                Hospital = new HospitalBasicInfo
                {
                    Name = "Four Rock Hospital",
                    Address = "Khu E Hutech, Quận 9, TP. Hồ Chí Minh",
                    Phone = "(0123) 456-789",
                    Email = "info@fourrock.com",
                    WorkingHours = "24/7 - Hỗ trợ cả ngày lẫn đêm",
                    Emergency = "Cấp cứu 24/7"
                }
            };
        }

        // Xử lý truy vấn local
        private async Task<string> ProcessLocalQuery(string userMessage)
        {
            var message = userMessage.ToLower();
            var info = await GetHospitalInfo();

            // Bác sĩ
            if (message.Contains("bác sĩ") || message.Contains("bác sí") || message.Contains("doctor"))
            {
                // Tìm bác sĩ cụ thể
                foreach (var doctor in info.Doctors)
                {
                    if (message.Contains(doctor.Name.ToLower()))
                    {
                        return $"Thông tin về Bác sĩ {doctor.Name}:\n" +
                               $"- Chuyên khoa: {doctor.Department}\n" +
                               $"- Mô tả: {doctor.Description}\n" +
                               $"- Số điện thoại: {doctor.Phone}\n" +
                               $"- Email: {doctor.Email}\n\n" +
                               "Bạn có thể đặt lịch khám với bác sĩ này qua website của chúng tôi.";
                    }
                }
                // Liệt kê tất cả bác sĩ
                if (message.Contains("danh sách") || message.Contains("có những") || message.Contains("list"))
                {
                    var doctorList = "Danh sách bác sĩ tại Four Rock Hospital:\n\n";
                    for (int i = 0; i < info.Doctors.Count; i++)
                        doctorList += $"{i + 1}. {info.Doctors[i].Name} - {info.Doctors[i].Department}\n";
                    doctorList += "\nVui lòng hỏi thêm nếu bạn muốn biết chi tiết về bác sĩ nào.";
                    return doctorList;
                }
                // Hỏi về chuyên khoa
                foreach (var dept in info.Departments)
                {
                    if (message.Contains(dept.Name.ToLower()))
                    {
                        var deptDoctors = info.Doctors.Where(d => d.Department == dept.Name).ToList();
                        var response = $"Thông tin về Khoa {dept.Name}:\n- Mô tả: {dept.Description}\n\n";
                        if (deptDoctors.Any())
                        {
                            response += "Bác sĩ thuộc khoa:\n";
                            foreach (var doc in deptDoctors)
                                response += $"- {doc.Name}\n";
                        }
                        else
                        {
                            response += "Hiện tại chưa có thông tin bác sĩ thuộc khoa này.\n";
                        }
                        return response;
                    }
                }
            }

            // Địa chỉ, liên hệ
            if (message.Contains("địa chỉ") || message.Contains("ở đâu") || message.Contains("address"))
            {
                return $"Thông tin liên hệ Four Rock Hospital:\n" +
                       $"- Địa chỉ: {info.Hospital.Address}\n" +
                       $"- Hotline: {info.Hospital.Phone}\n" +
                       $"- Email: {info.Hospital.Email}\n\n" +
                       $"Chúng tôi hoạt động {info.Hospital.WorkingHours}.";
            }

            // Thời gian khám
            if (message.Contains("thời gian") || message.Contains("giờ") || message.Contains("mở cửa") || message.Contains("time"))
            {
                return $"Thời gian hoạt động:\n- {info.Hospital.WorkingHours}\n- {info.Hospital.Emergency}\n\n" +
                       $"Bạn có thể đặt lịch khám qua website hoặc gọi hotline {info.Hospital.Phone}.";
            }

            // Dịch vụ
            if (message.Contains("dịch vụ") || message.Contains("service") || message.Contains("khám"))
            {
                return "Dịch vụ chính tại Four Rock Hospital:\n" +
                       "- Khám tổng quát\n- Tim mạch\n- Xét nghiệm\n- Cấp cứu 24/7\n- Nội trú\n- Điều trị chuyên khoa\n\n" +
                       $"Liên hệ {info.Hospital.Phone} để biết thêm chi tiết.";
            }

            // Đặt lịch
            if (message.Contains("đặt lịch") || message.Contains("book") || message.Contains("appointment"))
            {
                return $"Hướng dẫn đặt lịch khám tại Four Rock Hospital:\n" +
                       "- Cách 1: Đặt lịch trực tuyến qua website.\n" +
                       $"- Cách 2: Gọi hotline {info.Hospital.Phone}.\n" +
                       $"- Cách 3: Đến trực tiếp tại {info.Hospital.Address}.\n\n" +
                       $"Thời gian đặt lịch: {info.Hospital.WorkingHours}.\n" +
                       "Bạn muốn đặt lịch với bác sĩ nào?";
            }

            return null;
        }

        // Tạo context cho Gemini
        private string BuildContext(HospitalInfo info)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Bạn là trợ lý AI của Four Rock Hospital. Thông tin bệnh viện:\n");
            sb.AppendLine($"Tên: {info.Hospital.Name}");
            sb.AppendLine($"Địa chỉ: {info.Hospital.Address}");
            sb.AppendLine($"Hotline: {info.Hospital.Phone}");
            sb.AppendLine($"Email: {info.Hospital.Email}");
            sb.AppendLine($"Thời gian: 24/7\n");
            sb.AppendLine("Danh sách bác sĩ:");
            foreach (var doctor in info.Doctors)
                sb.AppendLine($"- {doctor.Name} (Chuyên khoa: {doctor.Department})");
            sb.AppendLine("\nDịch vụ chính: Khám tổng quát, Tim mạch, Xét nghiệm, Cấp cứu 24/7\n");
            sb.AppendLine("Hãy trả lời câu hỏi sau một cách thân thiện và chuyên nghiệp. Nếu câu hỏi không liên quan đến y tế hoặc bệnh viện, hãy lịch sự chuyển hướng về dịch vụ y tế:\n");
            return sb.ToString();
        }

        // Gọi Gemini API
        private async Task<string> CallGeminiApi(string fullMessage)
        {
            var client = _httpClientFactory.CreateClient();
            var url = _geminiApiUrl + _geminiApiKey;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = fullMessage }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 800
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);

                if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var contentElem) &&
                    contentElem.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0 &&
                    parts[0].TryGetProperty("text", out var textElem))
                {
                    return textElem.GetString();
                }
                if (doc.RootElement.TryGetProperty("error", out var errorElem) &&
                    errorElem.TryGetProperty("message", out var errMsg))
                {
                    return "Xin lỗi, tôi gặp vấn đề kỹ thuật. Vui lòng liên hệ hotline (0123) 456-789 để được hỗ trợ.";
                }
            }
            catch
            {
                return "Lỗi kết nối. Vui lòng thử lại sau.";
            }
            return "Xin lỗi, tôi không hiểu câu hỏi của bạn. Bạn có thể hỏi về thông tin bác sĩ, dịch vụ, địa chỉ hoặc đặt lịch khám.";
        }

        // Làm sạch phản hồi
        private string CleanResponse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "Xin lỗi, tôi không hiểu câu hỏi của bạn. Bạn có thể hỏi về thông tin bác sĩ, dịch vụ, địa chỉ hoặc đặt lịch khám.";
            text = text.Replace("**", "");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n\s*\n", "\n");
            return text.Trim();
        }

        // Model phụ trợ
        private class HospitalInfo
        {
            public System.Collections.Generic.List<DoctorInfo> Doctors { get; set; }
            public System.Collections.Generic.List<DepartmentInfo> Departments { get; set; }
            public HospitalBasicInfo Hospital { get; set; }
        }
        private class DoctorInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public string Description { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
        }
        private class DepartmentInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
        private class HospitalBasicInfo
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string WorkingHours { get; set; }
            public string Emergency { get; set; }
        }
    }
}
