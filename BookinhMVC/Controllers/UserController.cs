using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BookinhMVC.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System.IO;
using System;
using System.Threading.Tasks;

namespace BookinhMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly BookingContext _context;
        private readonly PasswordHasher<NguoiDung> _passwordHasher;

        public UserController(BookingContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<NguoiDung>();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            string username,
            string password,
            string fullname,
            DateTime dob,
            string gender,
            string phone,
            string email,
            string address,
            string soBaoHiem)
        {
            // Kiểm tra tính duy nhất
            if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == username))
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                return View();
            }
            if (await _context.BenhNhans.AnyAsync(b => b.Email == email))
            {
                ModelState.AddModelError("", "Email đã được sử dụng.");
                return View();
            }
            if (!string.IsNullOrWhiteSpace(soBaoHiem) && await _context.BenhNhans.AnyAsync(b => b.SoBaoHiem == soBaoHiem))
            {
                ModelState.AddModelError("", "Mã bảo hiểm y tế đã được sử dụng.");
                return View();
            }

            // Sinh OTP đăng ký
            string otp = new Random().Next(100000, 999999).ToString();
            var registrationData = new RegistrationModel
            {
                username = username,
                password = password,
                fullname = fullname,
                dob = dob,
                gender = gender,
                phone = phone,
                email = email,
                address = address,
                soBaoHiem = soBaoHiem,
                otp = otp
            };

            HttpContext.Session.SetString("RegistrationData", JsonSerializer.Serialize(registrationData));
            HttpContext.Session.SetString("RegistrationOtp", otp);

            // Gửi OTP qua email
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("hienquangtranht1@gmail.com", "BỆNH VIỆN FOUR_ROCK"),
                    Subject = "Xác nhận đăng ký tài khoản tại BỆNH VIỆN FOUR_ROCK",
                    Body = $"Xin chào {fullname},\nMã OTP của bạn là: {otp}\nMã này có hiệu lực trong 5 phút.",
                    IsBodyHtml = false
                };
                mail.To.Add(email);

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("hienquangtranht1@gmail.com", "qewg mrze brpz lncf"),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Không thể gửi OTP: " + ex.Message);
                return View();
            }
            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string otp)
        {
            var sessionOtp = HttpContext.Session.GetString("RegistrationOtp");
            var registrationJson = HttpContext.Session.GetString("RegistrationData");
            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(registrationJson))
            {
                ModelState.AddModelError("", "Thông tin đăng ký đã hết hạn. Vui lòng đăng ký lại.");
                return RedirectToAction("Register");
            }
            if (otp != sessionOtp)
            {
                ModelState.AddModelError("", "Mã OTP không đúng.");
                return View();
            }

            var registrationData = JsonSerializer.Deserialize<RegistrationModel>(registrationJson);

            // Kiểm tra các ràng buộc tính duy nhất
            if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == registrationData.username))
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                return RedirectToAction("Register");
            }
            if (await _context.BenhNhans.AnyAsync(b => b.Email == registrationData.email))
            {
                ModelState.AddModelError("", "Email đã được sử dụng.");
                return RedirectToAction("Register");
            }
            if (!string.IsNullOrWhiteSpace(registrationData.soBaoHiem) && await _context.BenhNhans.AnyAsync(b => b.SoBaoHiem == registrationData.soBaoHiem))
            {
                ModelState.AddModelError("", "Mã bảo hiểm y tế đã được sử dụng.");
                return RedirectToAction("Register");
            }

            // Tạo đối tượng NguoiDung
            var user = new NguoiDung
            {
                TenDangNhap = registrationData.username,
                VaiTro = "Bệnh nhân",
                NgayTao = DateTime.Now
            };
            user.MatKhau = _passwordHasher.HashPassword(user, registrationData.password);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Thêm user vào bảng NguoiDungs
                _context.NguoiDungs.Add(user);
                await _context.SaveChangesAsync();

                // Tạo đối tượng BenhNhan với shared primary key: MaBenhNhan = user.MaNguoiDung
                var patient = new BenhNhan
                {
                    MaBenhNhan = user.MaNguoiDung,  // Chia sẻ khóa chính từ NguoiDungs
                    HoTen = registrationData.fullname,
                    NgaySinh = registrationData.dob,
                    GioiTinh = registrationData.gender,
                    SoDienThoai = registrationData.phone,
                    Email = registrationData.email,
                    DiaChi = registrationData.address,
                    SoBaoHiem = registrationData.soBaoHiem ?? "",
                    HinhAnhBenhNhan = "default.jpg",
                    NgayTao = DateTime.Now
                };

                _context.BenhNhans.Add(patient);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                HttpContext.Session.Remove("RegistrationData");
                HttpContext.Session.Remove("RegistrationOtp");

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Registration failed: " + ex.Message);
                return View();
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == username);
            if (user != null && _passwordHasher.VerifyHashedPassword(user, user.MatKhau, password) == PasswordVerificationResult.Success)
            {
                HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
                HttpContext.Session.SetString("UserRole", user.VaiTro);

                if (user.VaiTro == "Bệnh nhân")
                {
                    // Vì shared primary key, truy vấn BenhNhan theo MaBenhNhan = user.MaNguoiDung
                    var patient = await _context.BenhNhans.FirstOrDefaultAsync(b => b.MaBenhNhan == user.MaNguoiDung);
                    if (patient != null)
                    {
                        HttpContext.Session.SetString("PatientName", patient.HoTen);
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role != "Bệnh nhân")
                return RedirectToAction("Login", "User");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword, string verificationCode)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            if (userId == null || string.IsNullOrEmpty(role) || role != "Bệnh nhân")
            {
                TempData["cp_message"] = "Bạn chưa đăng nhập hoặc không có quyền.";
                return RedirectToAction("Login");
            }
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null)
            {
                TempData["cp_message"] = "Không tìm thấy người dùng.";
                return RedirectToAction("ChangePassword");
            }
            if (_passwordHasher.VerifyHashedPassword(user, user.MatKhau, oldPassword) != PasswordVerificationResult.Success)
            {
                TempData["cp_message"] = "Mật khẩu cũ không đúng.";
                return RedirectToAction("ChangePassword");
            }
            if (newPassword != confirmPassword)
            {
                TempData["cp_message"] = "Mật khẩu mới và xác nhận không khớp.";
                return RedirectToAction("ChangePassword");
            }

            var sessionOtp = HttpContext.Session.GetString("ChangePasswordOtp");
            var sessionOtpTime = HttpContext.Session.GetString("ChangePasswordOtpTime");
            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(sessionOtpTime))
            {
                TempData["cp_message"] = "Mã OTP đã hết hạn hoặc chưa được gửi.";
                return RedirectToAction("ChangePassword");
            }
            if (verificationCode != sessionOtp)
            {
                TempData["cp_message"] = "Mã OTP không đúng.";
                return RedirectToAction("ChangePassword");
            }
            if (DateTime.TryParse(sessionOtpTime, out var otpTime))
            {
                if ((DateTime.Now - otpTime).TotalMinutes > 5)
                {
                    TempData["cp_message"] = "Mã OTP đã hết hạn.";
                    return RedirectToAction("ChangePassword");
                }
            }

            user.MatKhau = _passwordHasher.HashPassword(user, newPassword);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("ChangePasswordOtp");
            HttpContext.Session.Remove("ChangePasswordOtpTime");
            TempData["cp_message"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ChangePassword");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            if (userId == null || string.IsNullOrEmpty(role) || role != "Bệnh nhân")
                return RedirectToAction("Login");
            var patient = await _context.BenhNhans
                    .Include(b => b.NguoiDung)
                    .FirstOrDefaultAsync(b => b.MaBenhNhan == userId);
            if (patient == null)
                return RedirectToAction("Login");

            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullname, DateTime dob, string gender, string phone, string email, string address, string soBaoHiem, IFormFile HinhAnhBenhNhan)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            if (userId == null || string.IsNullOrEmpty(role) || role != "Bệnh nhân")
                return RedirectToAction("Login");
            var patient = await _context.BenhNhans
                    .Include(b => b.NguoiDung)
                    .FirstOrDefaultAsync(b => b.MaBenhNhan == userId);
            if (patient == null)
                return RedirectToAction("Login");

            if (await _context.BenhNhans.AnyAsync(b => b.Email == email && b.MaBenhNhan != userId))
            {
                TempData["notification"] = "Email đã được sử dụng bởi người khác.";
                return RedirectToAction("UpdateProfile");
            }
            if (!string.IsNullOrWhiteSpace(soBaoHiem) && await _context.BenhNhans.AnyAsync(b => b.SoBaoHiem == soBaoHiem && b.MaBenhNhan != userId))
            {
                TempData["notification"] = "Mã bảo hiểm y tế đã được sử dụng bởi người khác.";
                return RedirectToAction("UpdateProfile");
            }

            patient.HoTen = fullname;
            patient.NgaySinh = dob;
            patient.GioiTinh = gender;
            patient.SoDienThoai = phone;
            patient.Email = email;
            patient.DiaChi = address;
            patient.SoBaoHiem = soBaoHiem;
            if (HinhAnhBenhNhan != null && HinhAnhBenhNhan.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{HinhAnhBenhNhan.FileName}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await HinhAnhBenhNhan.CopyToAsync(stream);
                }
                patient.HinhAnhBenhNhan = fileName;
            }
            await _context.SaveChangesAsync();
            TempData["notification"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("UpdateProfile");
        }

        public class RegistrationModel
        {
            public string username { get; set; }
            public string password { get; set; }
            public string fullname { get; set; }
            public DateTime dob { get; set; }
            public string gender { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
            public string address { get; set; }
            public string soBaoHiem { get; set; }
            public string otp { get; set; }
        }
    }
}
