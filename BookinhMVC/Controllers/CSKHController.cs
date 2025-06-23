using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using BookinhMVC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookinhMVC.Controllers
{
    [Route("[controller]/[action]")]
    public class CSKHController : Controller
    {
        private readonly BookingContext _context;

        public CSKHController(BookingContext context)
        {
            _context = context;
        }

        // Hiển thị form đăng nhập CSKH
        [HttpGet]
        [Route("/CSKH/Login")]
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý đăng nhập CSKH từ bảng CsKhs (không dùng NguoiDungs)
        [HttpPost]
        [Route("/CSKH/Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Truy vấn từ bảng CsKhs
            var cskh = await _context.CsKhs.FirstOrDefaultAsync(c => c.Username == username);
            if (cskh != null)
            {
                // Nếu mật khẩu được mã hóa, bạn cần giải mã hoặc dùng IPasswordHasher tương ứng.
                // Ở đây, giả sử mật khẩu được lưu trực tiếp (plain text) để đơn giản:
                if (cskh.Password == password)
                {
                    // Lưu thông tin CSKH vào session. Ví dụ, giả sử CsKh có các thuộc tính Id (khóa chính) và FullName.
                    HttpContext.Session.SetInt32("CSKHId", cskh.Id);
                    HttpContext.Session.SetString("CSKHName", cskh.FullName);
                    HttpContext.Session.SetString("UserRole", "CSKH");
                    return RedirectToAction("Dashboard", "CSKH");
                }
            }
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View();
        }

        // Đăng xuất CSKH
        [HttpPost]
        [Route("/CSKH/Logout")]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("CSKHId");
            HttpContext.Session.Remove("CSKHName");
            HttpContext.Session.Remove("UserRole");
            return RedirectToAction("Login");
        }

        // Dashboard CSKH: danh sách khách hàng và chat (chỉ cho CSKH)
        [HttpGet]
        [Route("/CSKH/Dashboard")]
        public async Task<IActionResult> Dashboard(int? customerId)
        {
            var cskhId = HttpContext.Session.GetInt32("CSKHId");
            var role = HttpContext.Session.GetString("UserRole");
            if (cskhId == null || string.IsNullOrEmpty(role) || role != "CSKH")
                return RedirectToAction("Login");

            ViewBag.CSKHId = cskhId;

            var cskh = await _context.CsKhs.FindAsync(cskhId);

            // Lấy tất cả khách hàng đã từng chat với CSKH này (gửi hoặc nhận)
            var customerIds = await _context.ChatMessages
                .Where(m => m.ReceiverId == cskhId || m.SenderId == cskhId)
                .Select(m => m.SenderId == cskhId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var customers = await _context.NguoiDungs
                .Where(u => customerIds.Contains(u.MaNguoiDung))
                .ToListAsync();

            ViewBag.Customers = customers;
            ViewBag.CurrentCustomerId = customerId;

            if (customerId != null)
            {
                var messages = await _context.ChatMessages
                    .Where(m =>
                        (m.SenderId == customerId && m.ReceiverId == cskhId) ||
                        (m.SenderId == cskhId && m.ReceiverId == customerId))
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();
                ViewBag.Messages = messages;
            }
            else
            {
                ViewBag.Messages = null;
            }

            return View(cskh);
        }

        // Gửi tin nhắn từ CSKH đến khách hàng (chỉ cho CSKH)
        [HttpPost]
        [Route("/CSKH/SendMessage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int receiver_id, string message)
        {
            var cskhId = HttpContext.Session.GetInt32("CSKHId");
            var role = HttpContext.Session.GetString("UserRole");
            if (cskhId == null || string.IsNullOrEmpty(role) || role != "CSKH")
                return RedirectToAction("Login");

            if (receiver_id == 0 || string.IsNullOrWhiteSpace(message))
                return RedirectToAction("Dashboard", new { customerId = receiver_id });

            var chatMessage = new ChatMessage
            {
                SenderId = cskhId.Value,
                ReceiverId = receiver_id,
                Message = message.Trim(),
                CreatedAt = DateTime.Now
            };
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { customerId = receiver_id });
        }
    }
}
