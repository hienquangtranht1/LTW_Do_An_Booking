using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookinhMVC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookinhMVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatServiceController : Controller
    {
        private readonly BookingContext _context;

        public ChatServiceController(BookingContext context)
        {
            _context = context;
        }

        // API: Lấy lịch sử chat giữa bệnh nhân và CSKH
        [HttpPost]
        [Route("/api/patient/getMessages")]
        public async Task<IActionResult> GetPatientMessages([FromForm] int user1, [FromForm] int user2)
        {
            // Kiểm tra session bệnh nhân
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            if (userId == null || string.IsNullOrEmpty(role) || role != "Bệnh nhân")
                return Json(new { success = false, messages = Array.Empty<object>() });

            if (user1 == 0 || user2 == 0)
                return Json(new { success = false, messages = Array.Empty<object>() });

            var messages = await _context.ChatMessages
                .Where(m =>
                    (m.SenderId == user1 && m.ReceiverId == user2) ||
                    (m.SenderId == user2 && m.ReceiverId == user1))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    message = m.Message,
                    createdAt = m.CreatedAt
                })
                .ToListAsync();

            return Json(new { success = true, messages });
        }

        // API: Gửi tin nhắn từ bệnh nhân đến CSKH
        [HttpPost]
        [Route("/api/patient/sendMessage")]
        public async Task<IActionResult> SendPatientMessage([FromForm] int receiver_id, [FromForm] string message)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            if (userId == null || string.IsNullOrEmpty(role) || role != "Bệnh nhân")
                return Json(new { success = false, message = "Bạn cần đăng nhập với vai trò bệnh nhân." });

            if (receiver_id == 0 || string.IsNullOrWhiteSpace(message))
                return Json(new { success = false, message = "Thiếu thông tin cần thiết." });

            // Đảm bảo receiver_id là 1 (CSKH mặc định)
            if (receiver_id != 1)
                return Json(new { success = false, message = "Người nhận không hợp lệ." });

            var chatMessage = new ChatMessage
            {
                SenderId = userId.Value,
                ReceiverId = receiver_id,
                Message = message.Trim(),
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            var result = await _context.SaveChangesAsync() > 0;
            return Json(new { success = result, message = result ? "Tin nhắn đã được gửi" : "Gửi tin nhắn thất bại." });
        }

        // API: Lấy lịch sử chat giữa CSKH và bệnh nhân
        [HttpPost]
        [Route("/api/cskh/getMessages")]
        public async Task<IActionResult> GetCSKHMessages([FromForm] int cskhId, [FromForm] int customerId)
        {
            // Kiểm tra session CSKH
            var userId = HttpContext.Session.GetInt32("CSKHId");
            var role = HttpContext.Session.GetString("UserRole");
            if (userId == null || string.IsNullOrEmpty(role) || role != "CSKH")
                return Json(new { success = false, messages = Array.Empty<object>() });

            if (cskhId == 0 || customerId == 0)
                return Json(new { success = false, messages = Array.Empty<object>() });

            var messages = await _context.ChatMessages
                .Where(m =>
                    (m.SenderId == cskhId && m.ReceiverId == customerId) ||
                    (m.SenderId == customerId && m.ReceiverId == cskhId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    message = m.Message,
                    createdAt = m.CreatedAt
                })
                .ToListAsync();

            return Json(new { success = true, messages });
        }
    }
}