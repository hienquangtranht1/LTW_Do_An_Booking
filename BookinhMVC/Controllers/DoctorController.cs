using Microsoft.AspNetCore.Mvc;
using BookinhMVC.Models;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

namespace BookinhMVC.Controllers
{
    public class DoctorController : Controller
    {
        private readonly BookingContext _context;
        public DoctorController(BookingContext context)
        {
            _context = context;
        }

        // GET: /Doctor/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View("Login");
        }

        // POST: /Doctor/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == username && u.VaiTro == "BacSi");
            if (user != null)
            {
                var hasher = new PasswordHasher<NguoiDung>();
                var result = hasher.VerifyHashedPassword(user, user.MatKhau, password);
                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetInt32("DoctorId", user.MaNguoiDung);
                    HttpContext.Session.SetString("DoctorName", user.TenDangNhap);
                    var bacSi = _context.BacSis.FirstOrDefault(b => b.MaNguoiDung == user.MaNguoiDung);
                    if (bacSi != null)
                    {
                        HttpContext.Session.SetInt32("MaBacSi", bacSi.MaBacSi);
                        HttpContext.Session.SetString("DoctorImage", bacSi.HinhAnhBacSi ?? "default.jpg");
                    }
                    return RedirectToAction("Appointments");
                }
            }
            ViewBag.Error = "Sai tài khoản hoặc không phải bác sĩ.";
            return View("Login");
        }

        // Middleware kiểm tra đăng nhập và vai trò bác sĩ
        private bool IsDoctorLoggedIn()
        {
            var id = HttpContext.Session.GetInt32("DoctorId");
            if (id == null) return false;
            var user = _context.Set<NguoiDung>().FirstOrDefault(u => u.MaNguoiDung == id && u.VaiTro == "BacSi");
            return user != null;
        }

        // Lịch hẹn
        public async Task<IActionResult> Appointments(DateTime? date)
        {
            if (!IsDoctorLoggedIn()) return RedirectToAction("Login");
            var maBacSi = HttpContext.Session.GetInt32("MaBacSi");
            var filterDate = date ?? DateTime.Today;

            var appointments = await (from lh in _context.Set<LichHen>()
                                      join bn in _context.Set<BenhNhan>() on lh.MaBenhNhan equals bn.MaBenhNhan
                                      where lh.MaBacSi == maBacSi
                                      && lh.NgayGio.Date == filterDate.Date
                                      select new AppointmentViewModel
                                      {
                                          MaLich = lh.MaLich,
                                          HoTenBenhNhan = bn.HoTen,
                                          DiaChi = bn.DiaChi,
                                          GioiTinh = bn.GioiTinh,
                                          NgayGio = lh.NgayGio, // Fixed conversion
                                          TrieuChung = lh.TrieuChung,
                                          TrangThai = lh.TrangThai,
                                          AvailableTimes = null // sẽ gán sau
                                      }).ToListAsync();

            foreach (var appt in appointments)
            {
                appt.AvailableTimes = await GetAvailableTimesForAppointment(maBacSi.Value, appt.NgayGio.Date);
            }

            return View("Appointments", appointments);
        }

        // Hàm lấy danh sách khung giờ trống cho một ngày
        private async Task<List<string>> GetAvailableTimesForAppointment(int maBacSi, DateTime date)
        {
            var workSchedules = await _context.LichLamViecs
                .Where(lv => lv.MaBacSi == maBacSi && lv.NgayLamViec == date && lv.TrangThai == "Đã xác nhận")
                .OrderBy(lv => lv.GioBatDau)
                .ToListAsync();

            var bookedTimes = await _context.LichHens
                .Where(l => l.MaBacSi == maBacSi
                    && l.NgayGio.Date == date.Date
                    && l.TrangThai != "Đã hủy")
                .Select(l => l.NgayGio.TimeOfDay)
                .ToListAsync();

            var availableTimes = new List<string>();
            foreach (var schedule in workSchedules)
            {
                var start = schedule.GioBatDau;
                var end = schedule.GioKetThuc;
                for (var t = start; t < end; t = t.Add(TimeSpan.FromMinutes(30)))
                {
                    bool isBooked = bookedTimes.Any(bt => Math.Abs((bt - t).TotalMinutes) < 1);
                    if (!isBooked)
                    {
                        availableTimes.Add(t.ToString(@"hh\:mm\:ss"));
                    }
                }
            }
            return availableTimes;
        }

        [HttpPost]
        public IActionResult UpdateAppointment(int id, string status)
        {
            if (!IsDoctorLoggedIn()) return RedirectToAction("Login");
            var appointment = _context.Set<LichHen>().FirstOrDefault(l => l.MaLich == id);
            if (appointment != null)
            {
                appointment.TrangThai = status;
                _context.SaveChanges();
            }
            return RedirectToAction("Appointments");
        }

        // Hồ sơ bệnh án
        public IActionResult MedicalRecords(string search)
        {
            if (!IsDoctorLoggedIn()) return RedirectToAction("Login");
            var maBacSi = HttpContext.Session.GetInt32("MaBacSi");
            var records = (from hs in _context.Set<HoSoBenhAn>()
                           join bn in _context.Set<BenhNhan>() on hs.MaBenhNhan equals bn.MaBenhNhan
                           join bs in _context.Set<BacSi>() on hs.MaBacSi equals bs.MaBacSi
                           where (string.IsNullOrEmpty(search) || bn.HoTen.Contains(search))
                           select new MedicalRecordViewModel
                           {
                               MaHoSo = hs.MaHoSo,
                               TenBenhNhan = bn.HoTen,
                               TenBacSi = bs.HoTen,
                               MaBacSi = hs.MaBacSi, // Thêm để biết bác sĩ nào tạo hồ sơ
                               NgayKham = hs.NgayKham,
                               ChanDoan = hs.ChanDoan,
                               PhuongAnDieuTri = hs.PhuongAnDieuTri
                           }).ToList();
            return View("MedicalRecords", records);
        }

        [HttpPost]
        public IActionResult AddOrUpdateMedicalRecord(string TenBenhNhan, DateTime NgayKham, string ChanDoan, string PhuongAnDieuTri)
        {
            if (!IsDoctorLoggedIn()) return RedirectToAction("Login");
            var maBacSi = HttpContext.Session.GetInt32("MaBacSi");
            if (maBacSi == null)
            {
                TempData["Error"] = "Không xác định được bác sĩ. Vui lòng đăng nhập lại.";
                return RedirectToAction("MedicalRecords");
            }
            // Tìm bệnh nhân theo tên
            var benhNhan = _context.Set<BenhNhan>().FirstOrDefault(bn => bn.HoTen == TenBenhNhan);
            if (benhNhan == null)
            {
                TempData["Error"] = "Không tìm thấy bệnh nhân với tên này. Vui lòng nhập đúng tên bệnh nhân đã đăng ký!";
                return RedirectToAction("MedicalRecords");
            }
            var record = new HoSoBenhAn
            {
                MaBenhNhan = benhNhan.MaBenhNhan,
                MaBacSi = maBacSi.Value,
                NgayKham = NgayKham,
                ChanDoan = ChanDoan,
                PhuongAnDieuTri = PhuongAnDieuTri
            };
            _context.Set<HoSoBenhAn>().Add(record);
            _context.SaveChanges();
            TempData["Success"] = "Thêm hồ sơ bệnh án thành công!";
            return RedirectToAction("MedicalRecords");
        }

        [HttpPost]
        public IActionResult UpdateMedicalRecord(int MaHoSo, string ChanDoan, string PhuongAnDieuTri)
        {
            if (!IsDoctorLoggedIn()) return RedirectToAction("Login");
            var maBacSi = HttpContext.Session.GetInt32("MaBacSi");
            
            var record = _context.Set<HoSoBenhAn>().FirstOrDefault(hs => hs.MaHoSo == MaHoSo);
            if (record == null)
            {
                TempData["Error"] = "Không tìm thấy hồ sơ bệnh án!";
                return RedirectToAction("MedicalRecords");
            }
            
            // Kiểm tra quyền chỉnh sửa - chỉ bác sĩ tạo hồ sơ mới được chỉnh sửa
            if (record.MaBacSi != maBacSi)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa hồ sơ bệnh án này!";
                return RedirectToAction("MedicalRecords");
            }
            
            record.ChanDoan = ChanDoan;
            record.PhuongAnDieuTri = PhuongAnDieuTri;
            _context.SaveChanges();
            TempData["Success"] = "Cập nhật hồ sơ bệnh án thành công!";
            return RedirectToAction("MedicalRecords");
        }

        // Lịch làm việc
        public IActionResult WorkSchedule(DateTime? weekStart)
        {
            if (!IsDoctorLoggedIn()) return RedirectToAction("Login");
            var maBacSi = HttpContext.Session.GetInt32("MaBacSi");
            var start = weekStart ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
            var end = start.AddDays(6);
            var schedules = _context.Set<LichLamViec>()
                .Where(l => l.MaBacSi == maBacSi && l.NgayLamViec >= start && l.NgayLamViec <= end)
                .ToList();
            ViewBag.WeekStart = start;
            ViewBag.WeekEnd = end;
            return View("WorkSchedule", schedules);
        }

        [HttpPost]
        public IActionResult UpdateWorkSchedule(int Id, string Ngay, string GioBatDau, string GioKetThuc, string actionType)
        {
            if (!IsDoctorLoggedIn()) return RedirectToAction("Login");
            var maBacSi = HttpContext.Session.GetInt32("MaBacSi");
            DateTime ngayLamViec = DateTime.Parse(Ngay);
            TimeSpan gioBatDau = TimeSpan.Parse(GioBatDau);
            TimeSpan gioKetThuc = TimeSpan.Parse(GioKetThuc);

            if (actionType == "delete")
            {
                var schedule = _context.Set<LichLamViec>().FirstOrDefault(l => l.MaLich == Id && l.MaBacSi == maBacSi);
                if (schedule != null)
                    _context.Set<LichLamViec>().Remove(schedule);
            }
            else if (actionType == "add")
            {
                var lich = new LichLamViec
                {
                    MaBacSi = maBacSi.Value,
                    NgayLamViec = ngayLamViec,
                    ThuTrongTuan = ((int)ngayLamViec.DayOfWeek == 0 ? "8" : ((int)ngayLamViec.DayOfWeek + 1).ToString()),
                    GioBatDau = gioBatDau,
                    GioKetThuc = gioKetThuc,
                    TrangThai = "Chua xac nhan",
                    NgayTao = DateTime.Now
                };
                _context.Set<LichLamViec>().Add(lich);
            }
            else if (actionType == "update")
            {
                var schedule = _context.Set<LichLamViec>().FirstOrDefault(l => l.MaLich == Id && l.MaBacSi == maBacSi);
                if (schedule != null)
                {
                    schedule.GioBatDau = gioBatDau;
                    schedule.GioKetThuc = gioKetThuc;
                    schedule.TrangThai = "Chua xac nhan";
                }
            }
            _context.SaveChanges();
            return RedirectToAction("WorkSchedule");
        }

        // Thông tin cá nhân
        public IActionResult Profile()
        {
            if (!IsDoctorLoggedIn()) return RedirectToAction("Login");
            var maNguoiDung = HttpContext.Session.GetInt32("DoctorId");
            var bacSi = _context.Set<BacSi>().FirstOrDefault(b => b.MaNguoiDung == maNguoiDung);
            if (bacSi == null)
            {
                return RedirectToAction("Login");
            }
            return View(bacSi);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(int MaBacSi, string HoTen, string SoDienThoai, string Email, string MoTa, IFormFile profileImage)
        {
            var doctor = await _context.BacSis.FindAsync(MaBacSi);
            if (doctor == null) return RedirectToAction("Profile");

            // Cập nhật thông tin cơ bản
            doctor.HoTen = HoTen;
            doctor.SoDienThoai = SoDienThoai;
            doctor.Email = Email;
            doctor.MoTa = MoTa;

            // Xử lý upload ảnh nếu có
            if (profileImage != null && profileImage.Length > 0)
            {
                // Tạo thư mục uploads nếu chưa có
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // Đặt tên file duy nhất
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(profileImage.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Lưu file vào thư mục uploads
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                // Xóa file cũ nếu không phải default
                if (!string.IsNullOrEmpty(doctor.HinhAnhBacSi) && doctor.HinhAnhBacSi != "default.jpg")
                {
                    var oldPath = Path.Combine(uploadPath, doctor.HinhAnhBacSi);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Cập nhật tên file vào DB và session
                doctor.HinhAnhBacSi = fileName;
                HttpContext.Session.SetString("DoctorImage", fileName);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!IsDoctorLoggedIn()) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestChangePasswordOtp()
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorId");

            if (!IsDoctorLoggedIn())
                return Json(new { success = false, message = "Chưa đăng nhập." });

            var doctor = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == doctorId);

            if (doctor == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin bác sĩ." });

            // Lấy email từ bảng BacSi
            var bacSi = await _context.BacSis.FirstOrDefaultAsync(b => b.MaNguoiDung == doctorId);
            if (bacSi == null || string.IsNullOrEmpty(bacSi.Email))
                return Json(new { success = false, message = "Không tìm thấy email của bác sĩ." });

            // Sinh mã OTP
            string otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("ChangePasswordOtp", otp);
            HttpContext.Session.SetString("ChangePasswordOtpTime", DateTime.Now.ToString());

            // Gửi OTP qua email
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("hienquangtranht1@gmail.com", "BỆNH VIỆN FOUR_ROCK"),
                    Subject = "Mã OTP đổi mật khẩu",
                    Body = $"Xin chào {doctor.TenDangNhap},\nMã OTP của bạn là: {otp}\nMã này có hiệu lực trong 5 phút.",
                    IsBodyHtml = false
                };
                mail.To.Add(bacSi.Email);

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("hienquangtranht1@gmail.com", "qewg mrze brpz lncf"),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(mail);
                return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Không thể gửi OTP: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword, string verificationCode)
        {
            if (!IsDoctorLoggedIn()) return RedirectToAction("Login");

            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            var doctor = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == doctorId);

            if (doctor == null)
            {
                TempData["cp_message"] = "Không tìm thấy thông tin bác sĩ.";
                return RedirectToAction("ChangePassword");
            }

            var hasher = new PasswordHasher<NguoiDung>();
            if (hasher.VerifyHashedPassword(doctor, doctor.MatKhau, oldPassword) != PasswordVerificationResult.Success)
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

            if (DateTime.TryParse(sessionOtpTime, out var otpTime) && (DateTime.Now - otpTime).TotalMinutes > 5)
            {
                TempData["cp_message"] = "Mã OTP đã hết hạn.";
                return RedirectToAction("ChangePassword");
            }

            doctor.MatKhau = hasher.HashPassword(doctor, newPassword);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("ChangePasswordOtp");
            HttpContext.Session.Remove("ChangePasswordOtpTime");

            TempData["cp_message"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ChangePassword");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Doctor");
        }

        // Hỏi đáp
        public IActionResult QA()
        {
            var questions = _context.Questions
                .Include(q => q.User)
                .Include(q => q.Doctor).ThenInclude(d => d.Khoa)
                .OrderByDescending(q => q.AnsweredAt)
                .ToList();

            ViewBag.Doctors = _context.BacSis.Include(b => b.Khoa).ToList();
            return View(questions);
        }

        [HttpPost]
        public IActionResult QA(int DoctorId, string Title, string Content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đặt câu hỏi.";
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
                CreatedAt = DateTime.Now
            };
            _context.Questions.Add(question);
            _context.SaveChanges();

            TempData["Message"] = "Câu hỏi của bạn đã được gửi thành công!";
            return RedirectToAction("QA");
        }

        public IActionResult Question()
        {
            var maBacSi = HttpContext.Session.GetInt32("MaBacSi");
            if (maBacSi == null)
            {
                return RedirectToAction("Login", "Doctor");
            }

            var questions = _context.Questions
                .Include(q => q.User)
                .Where(q => q.DoctorId == maBacSi)
                .OrderByDescending(q => q.CreatedAt)
                .ToList();

            return View(questions);
        }

        [HttpPost]
        public IActionResult Answer(int questionId, string answer)
        {
            var maBacSi = HttpContext.Session.GetInt32("MaBacSi");
            if (maBacSi == null)
                return RedirectToAction("Login");

            var question = _context.Questions.FirstOrDefault(q => q.Id == questionId && q.DoctorId == maBacSi);
            if (question == null)
            {
                TempData["Error"] = "Không tìm thấy câu hỏi.";
                return RedirectToAction("Question");
            }

            question.Answer = answer;
            question.Status = "Đã trả lời";
            question.AnsweredAt = DateTime.Now;
            _context.SaveChanges();

            TempData["Success"] = "Đã trả lời câu hỏi thành công!";
            return RedirectToAction("Question");
        }

        [HttpPost]
        public IActionResult DeleteQuestion(int questionId)
        {
            var maBacSi = HttpContext.Session.GetInt32("MaBacSi");
            if (maBacSi == null)
                return RedirectToAction("Login");

            var question = _context.Questions.FirstOrDefault(q => q.Id == questionId && q.DoctorId == maBacSi);
            if (question == null)
            {
                TempData["Error"] = "Không tìm thấy câu hỏi.";
                return RedirectToAction("Question");
            }

            _context.Questions.Remove(question);
            _context.SaveChanges();

            TempData["Success"] = "Đã xóa câu hỏi thành công!";
            return RedirectToAction("Question");
        }
    }
}