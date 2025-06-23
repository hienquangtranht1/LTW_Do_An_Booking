using System.Diagnostics;
using System.Linq;
using BookinhMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookinhMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookingContext _context;

        public HomeController(ILogger<HomeController> logger, BookingContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Trang chủ: truyền danh sách bác sĩ cho Index.cshtml
        public IActionResult Index()
        {
            var doctors = _context.BacSis.Include(b => b.Khoa).ToList();
            ViewBag.Doctors = doctors;
            return View();
        }

        // Trang giới thiệu
        public IActionResult HospitalAbout()
        {
            return View();
        }

        // Trang blog
        public IActionResult HospitalBlog()
        {
            return View();
        }

        // Trang xét nghiệm
        public IActionResult Testing()
        {
            return View();
        }

        // Trang khám tổng quát
        public IActionResult GeneralCheckup()
        {
            return View();
        }

        // Trang tim mạch
        public IActionResult Cardiology()
        {
            return View();
        }

        // Trang liên hệ
        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Trang danh sách bác sĩ với tìm kiếm
        public IActionResult Doctors(string name, int departmentId = 0)
        {
            var doctors = _context.BacSis.Include(b => b.Khoa).AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                doctors = doctors.Where(d => d.HoTen.Contains(name));
            }
            if (departmentId > 0)
            {
                doctors = doctors.Where(d => d.MaKhoa == departmentId);
            }

            var departments = _context.Khoas.ToList();
            ViewBag.Departments = departments;
            ViewBag.SearchName = name;
            ViewBag.SelectedDepartment = departmentId;

            return View(doctors.ToList());
        }

        // Trang hỏi đáp với bác sĩ (bệnh nhân đặt câu hỏi, xem câu trả lời)
        [HttpGet]
        public IActionResult QA()
        {
            var doctors = _context.BacSis.Include(b => b.Khoa).ToList();
            var answeredQuestions = _context.Questions
                .Include(q => q.User)
                .Include(q => q.Doctor)
                .Where(q => !string.IsNullOrEmpty(q.Answer))
                .OrderByDescending(q => q.AnsweredAt)
                .ToList();

            ViewBag.Doctors = doctors;
            return View(answeredQuestions);
        }

        [HttpPost]
        public IActionResult QA(int DoctorId, string Title, string Content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("QA");
            }
            if (DoctorId == 0)
            {
                TempData["Error"] = "Vui lòng chọn bác sĩ.";
                return RedirectToAction("QA");
            }
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                return RedirectToAction("QA");
            }

            var question = new Question
            {
                UserId = userId.Value,
                DoctorId = DoctorId,
                Title = Title,
                Content = Content,
                Status = "Chờ trả lời",
                CreatedAt = DateTime.Now,
                Answer = "", 
                Category = "" 
            };
            _context.Questions.Add(question);
            _context.SaveChanges();

            TempData["Message"] = "Câu hỏi của bạn đã được gửi thành công!";
            return RedirectToAction("QA");
        }
    }
}
