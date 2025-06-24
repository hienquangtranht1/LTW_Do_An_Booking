using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BookinhMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace BookinhMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly BookingContext _context;
        public AdminController(BookingContext context) => _context = context;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var actionName = context.ActionDescriptor.RouteValues["action"];
            if (actionName == "Login" || actionName == "Logout")
            {
                base.OnActionExecuting(context);
                return;
            }
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                context.Result = RedirectToAction("Login", "Admin");
                return;
            }
            base.OnActionExecuting(context);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var admin = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TenDangNhap == username && u.VaiTro == "Admin");

            var hasher = new PasswordHasher<NguoiDung>();
            if (admin != null && hasher.VerifyHashedPassword(admin, admin.MatKhau, password) == PasswordVerificationResult.Success)
            {
                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetString("AdminName", admin.TenDangNhap);
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Admin");
        }

        // Dashboard admin
        public IActionResult Index()
        {
            ViewBag.UserCount = _context.NguoiDungs.Count();
            ViewBag.DoctorCount = _context.BacSis.Count();
            ViewBag.PatientCount = _context.BenhNhans.Count();
            ViewBag.DepartmentCount = _context.Khoas.Count();
            ViewBag.AppointmentCount = _context.LichHens.Count();
            ViewBag.ReviewCount = _context.DanhGias.Count();
            return View();
        }

        // --------- CRUD Khoa ---------
        public async Task<IActionResult> Departments()
        {
            var departments = await _context.Khoas.AsNoTracking().ToListAsync();
            return View(departments);
        }

        public IActionResult CreateDepartment() => View();

        [HttpPost]
        public async Task<IActionResult> CreateDepartment(string tenKhoa, string moTa)
        {
            var model = new Khoa
            {
                TenKhoa = tenKhoa,
                MoTa = moTa,
                NgayTao = DateTime.Now
            };
            _context.Khoas.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Departments");
        }

        public async Task<IActionResult> EditDepartment(int id)
        {
            var dep = await _context.Khoas.FindAsync(id);
            if (dep == null) return NotFound();
            return View(dep);
        }

        [HttpPost]
        public async Task<IActionResult> EditDepartment(int id, string tenKhoa, string moTa)
        {
            var existing = await _context.Khoas.AsNoTracking().FirstOrDefaultAsync(k => k.MaKhoa == id);
            if (existing == null) return NotFound();
            var model = new Khoa
            {
                MaKhoa = id,
                TenKhoa = tenKhoa,
                MoTa = moTa,
                NgayTao = existing.NgayTao
            };
            _context.Khoas.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Departments");
        }

        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dep = await _context.Khoas.FindAsync(id);
            if (dep == null) return NotFound();
            return View(dep);
        }

        [HttpPost, ActionName("DeleteDepartment")]
        public async Task<IActionResult> DeleteDepartmentConfirmed(int id)
        {
            var dep = await _context.Khoas.FindAsync(id);
            if (dep != null)
            {
                _context.Khoas.Remove(dep);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Departments");
        }

        public async Task<IActionResult> DepartmentDetails(int id)
        {
            var dep = await _context.Khoas.FindAsync(id);
            if (dep == null) return NotFound();
            return View(dep);
        }

        // --------- CRUD NguoiDung ---------
        public async Task<IActionResult> Users()
        {
            var users = await _context.NguoiDungs.AsNoTracking().ToListAsync();
            return View(users);
        }

        public IActionResult CreateUser() => View();

        [HttpPost]
        public async Task<IActionResult> CreateUser(string tenDangNhap, string matKhau, string vaiTro)
        {
            var model = new NguoiDung
            {
                TenDangNhap = tenDangNhap,
                VaiTro = vaiTro,
                NgayTao = DateTime.Now
            };
            var hasher = new PasswordHasher<NguoiDung>();
            model.MatKhau = hasher.HashPassword(model, matKhau);
            _context.NguoiDungs.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(int id, string tenDangNhap, string vaiTro, string newPassword)
        {
            var existing = await _context.NguoiDungs.AsNoTracking().FirstOrDefaultAsync(u => u.MaNguoiDung == id);
            if (existing == null) return NotFound();
            var model = new NguoiDung
            {
                MaNguoiDung = id,
                TenDangNhap = tenDangNhap,
                VaiTro = vaiTro,
                NgayTao = existing.NgayTao,
                MatKhau = existing.MatKhau
            };
            if (!string.IsNullOrEmpty(newPassword))
            {
                var hasher = new PasswordHasher<NguoiDung>();
                model.MatKhau = hasher.HashPassword(model, newPassword);
            }
            _context.NguoiDungs.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user != null)
            {
                _context.NguoiDungs.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // --------- CRUD BacSi ---------
        public async Task<IActionResult> Doctors()
        {
            var doctors = await _context.BacSis.Include(b => b.Khoa).AsNoTracking().ToListAsync();
            return View(doctors);
        }

        public IActionResult CreateDoctor()
        {
            ViewBag.Khoas = _context.Khoas.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor(
            string tenDangNhap,
            string hoTen,
            int maKhoa,
            string soDienThoai,
            string email,
            string moTa,
            IFormFile file)
        {
            // Tạo tài khoản người dùng cho bác sĩ với mật khẩu mặc định
            var nguoiDung = new NguoiDung
            {
                TenDangNhap = tenDangNhap,
                VaiTro = "BacSi",
                NgayTao = DateTime.Now
            };
            var hasher = new PasswordHasher<NguoiDung>();
            nguoiDung.MatKhau = hasher.HashPassword(nguoiDung, "123456"); // Mật khẩu mặc định

            _context.NguoiDungs.Add(nguoiDung);
            await _context.SaveChangesAsync();

            // Lưu file ảnh như hiện tại...
            string fileName = null;
            if (file != null && file.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            // Tạo bác sĩ
            var model = new BacSi
            {
                MaNguoiDung = nguoiDung.MaNguoiDung, // Tự động lấy MaNguoiDung vừa tạo
                HoTen = hoTen,
                MaKhoa = maKhoa,
                SoDienThoai = soDienThoai,
                Email = email,
                HinhAnhBacSi = fileName,
                MoTa = moTa,
                NgayTao = DateTime.Now
            };
            _context.BacSis.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Doctors");
        }

        public async Task<IActionResult> EditDoctor(int id)
        {
            var doctor = await _context.BacSis.Include(b => b.NguoiDung).FirstOrDefaultAsync(b => b.MaBacSi == id);
            if (doctor == null) return NotFound();
            ViewBag.Khoas = _context.Khoas.ToList();
            return View(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> EditDoctor(int id, int maNguoiDung, string hoTen, int maKhoa, string soDienThoai, string email, string hinhAnhBacSi, string moTa, IFormFile file)
        {
            var existing = await _context.BacSis.AsNoTracking().FirstOrDefaultAsync(b => b.MaBacSi == id);
            if (existing == null) return NotFound();

            // Xử lý upload file nếu có
            string fileName = hinhAnhBacSi; // Giữ tên file cũ nếu không upload file mới
            if (file != null && file.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            var model = new BacSi
            {
                MaBacSi = id,
                MaNguoiDung = maNguoiDung,
                HoTen = hoTen,
                MaKhoa = maKhoa,
                SoDienThoai = soDienThoai,
                Email = email,
                HinhAnhBacSi = fileName,
                MoTa = moTa,
                NgayTao = existing.NgayTao
            };
            _context.BacSis.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Doctors");
        }

        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.BacSis.FindAsync(id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        [HttpPost, ActionName("DeleteDoctor")]
        public async Task<IActionResult> DeleteDoctorConfirmed(int id)
        {
            var doctor = await _context.BacSis.FindAsync(id);
            if (doctor != null)
            {
                _context.BacSis.Remove(doctor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Doctors");
        }

        public async Task<IActionResult> DoctorDetails(int id)
        {
            var doctor = await _context.BacSis.Include(b => b.Khoa).FirstOrDefaultAsync(b => b.MaBacSi == id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        // --------- CRUD BenhNhan ---------
        public async Task<IActionResult> Patients()
        {
            var patients = await _context.BenhNhans.AsNoTracking().ToListAsync();
            return View(patients);
        }

        public IActionResult CreatePatient() => View();

        [HttpPost]
        public async Task<IActionResult> CreatePatient(int maNguoiDung, string hoTen, DateTime ngaySinh, string gioiTinh, string soDienThoai, string email, string diaChi, string soBaoHiem, string hinhAnhBenhNhan)
        {
            var model = new BenhNhan
            {
                MaBenhNhan = maNguoiDung,
                HoTen = hoTen,
                NgaySinh = ngaySinh,
                GioiTinh = gioiTinh,
                SoDienThoai = soDienThoai,
                Email = email,
                DiaChi = diaChi,
                SoBaoHiem = soBaoHiem,
                HinhAnhBenhNhan = hinhAnhBenhNhan,
                NgayTao = DateTime.Now
            };
            _context.BenhNhans.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Patients");
        }

        public async Task<IActionResult> EditPatient(int id)
        {
            var patient = await _context.BenhNhans.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> EditPatient(IFormCollection form, IFormFile file)
        {
            var id = int.Parse(form["id"]);
            var patient = await _context.BenhNhans.FindAsync(id);
            if (patient == null) return NotFound();

            string fileName = form["hinhAnhBenhNhan"];
            if (file != null && file.Length > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            patient.HinhAnhBenhNhan = fileName;
            patient.HoTen = form["hoTen"];
            patient.NgaySinh = DateTime.Parse(form["ngaySinh"]);
            patient.GioiTinh = form["gioiTinh"];
            patient.SoDienThoai = form["soDienThoai"];
            patient.Email = form["email"];
            patient.DiaChi = form["diaChi"];
            patient.SoBaoHiem = form["soBaoHiem"];

            _context.BenhNhans.Update(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction("Patients");
        }

        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.BenhNhans.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        [HttpPost, ActionName("DeletePatient")]
        public async Task<IActionResult> DeletePatientConfirmed(int id)
        {
            var patient = await _context.BenhNhans.FindAsync(id);
            if (patient != null)
            {
                _context.BenhNhans.Remove(patient);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Patients");
        }

        public async Task<IActionResult> PatientDetails(int id)
        {
            var patient = await _context.BenhNhans.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // --------- CRUD LichHen ---------
        public async Task<IActionResult> Appointments()
        {
            var appts = await _context.LichHens
                .Include(l => l.BenhNhan)
                .Include(l => l.BacSi)
                .AsNoTracking()
                .ToListAsync();
            return View(appts);
        }

        public IActionResult CreateAppointment() => View();

        [HttpPost]
        public async Task<IActionResult> CreateAppointment(int maBenhNhan, int maBacSi, DateTime ngayGio, string trieuChung, string trangThai)
        {
            var model = new LichHen
            {
                MaBenhNhan = maBenhNhan,
                MaBacSi = maBacSi,
                NgayGio = ngayGio,
                TrieuChung = trieuChung,
                TrangThai = trangThai
            };
            _context.LichHens.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Appointments");
        }

        public async Task<IActionResult> EditAppointment(int id)
        {
            var appt = await _context.LichHens.FindAsync(id);
            if (appt == null) return NotFound();
            return View(appt);
        }

        [HttpPost]
        public async Task<IActionResult> EditAppointment(int id, int maBenhNhan, int maBacSi, DateTime ngayGio, string trieuChung, string trangThai)
        {
            var model = new LichHen
            {
                MaLich = id,
                MaBenhNhan = maBenhNhan,
                MaBacSi = maBacSi,
                NgayGio = ngayGio,
                TrieuChung = trieuChung,
                TrangThai = trangThai
            };
            _context.LichHens.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Appointments");
        }

        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appt = await _context.LichHens.FindAsync(id);
            if (appt == null) return NotFound();
            return View(appt);
        }

        [HttpPost, ActionName("DeleteAppointment")]
        public async Task<IActionResult> DeleteAppointmentConfirmed(int id)
        {
            var appt = await _context.LichHens.FindAsync(id);
            if (appt != null)
            {
                _context.LichHens.Remove(appt);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Appointments");
        }

        public async Task<IActionResult> AppointmentDetails(int id)
        {
            var appt = await _context.LichHens
                .Include(l => l.BenhNhan)
                .Include(l => l.BacSi)
                .FirstOrDefaultAsync(l => l.MaLich == id);
            if (appt == null) return NotFound();
            return View(appt);
        }

        // --------- CRUD DanhGia ---------
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.DanhGias
                .Include(r => r.BacSi)
                .Include(r => r.BenhNhan)
                .AsNoTracking()
                .ToListAsync();
            return View(reviews);
        }

        public IActionResult CreateReview() => View();

        [HttpPost]
        public async Task<IActionResult> CreateReview(int maBacSi, int maBenhNhan, int diemDanhGia, string nhanXet, DateTime ngayDanhGia)
        {
            var model = new DanhGia
            {
                MaBacSi = maBacSi,
                MaBenhNhan = maBenhNhan,
                DiemDanhGia = diemDanhGia,
                NhanXet = nhanXet,
                NgayDanhGia = ngayDanhGia
            };
            _context.DanhGias.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Reviews");
        }

        public async Task<IActionResult> EditReview(int id)
        {
            var review = await _context.DanhGias.FindAsync(id);
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost]
        public async Task<IActionResult> EditReview(int id, int maBacSi, int maBenhNhan, int diemDanhGia, string nhanXet, DateTime ngayDanhGia)
        {
            var model = new DanhGia
            {
                MaDanhGia = id,
                MaBacSi = maBacSi,
                MaBenhNhan = maBenhNhan,
                DiemDanhGia = diemDanhGia,
                NhanXet = nhanXet,
                NgayDanhGia = ngayDanhGia
            };
            _context.DanhGias.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Reviews");
        }

        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.DanhGias.FindAsync(id);
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost, ActionName("DeleteReview")]
        public async Task<IActionResult> DeleteReviewConfirmed(int id)
        {
            var review = await _context.DanhGias.FindAsync(id);
            if (review != null)
            {
                _context.DanhGias.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Reviews");
        }

        public async Task<IActionResult> ReviewDetails(int id)
        {
            var review = await _context.DanhGias
                .Include(r => r.BacSi)
                .Include(r => r.BenhNhan)
                .FirstOrDefaultAsync(r => r.MaDanhGia == id);
            if (review == null) return NotFound();
            return View(review);
        }

        // --------- CRUD LichLamViec ---------
        public IActionResult WorkSchedules(int? doctorId, DateTime? weekStart)
        {
            var query = _context.LichLamViecs
                .Include(l => l.BacSi)
                .ThenInclude(bs => bs.NguoiDung)
                .AsQueryable();

            if (doctorId.HasValue)
                query = query.Where(l => l.MaBacSi == doctorId.Value);

            // Sửa: Tính lại tuần bắt đầu từ thứ 2 gần nhất của weekStart (hoặc hôm nay nếu null)
            DateTime start;
            if (weekStart.HasValue)
            {
                int daysFromMonday = ((int)weekStart.Value.DayOfWeek + 6) % 7;
                start = weekStart.Value.AddDays(-daysFromMonday);
            }
            else
            {
                var today = DateTime.Today;
                int daysFromMonday = ((int)today.DayOfWeek + 6) % 7;
                start = today.AddDays(-daysFromMonday);
            }

            var schedules = query
                .Where(l => l.NgayLamViec >= start && l.NgayLamViec <= start.AddDays(6))
                .OrderBy(l => l.MaBacSi)
                .ThenBy(l => l.NgayLamViec)
                .ToList();

            ViewBag.Doctors = _context.BacSis.Include(bs => bs.NguoiDung).ToList();
            ViewBag.SelectedDoctor = doctorId;
            ViewBag.WeekStart = start;
            return View("WorkSchedules", schedules);
        }

        [HttpPost]
        public IActionResult ConfirmWorkSchedule(int id)
        {
            var schedule = _context.LichLamViecs.FirstOrDefault(l => l.MaLich == id);
            if (schedule != null)
            {
                schedule.TrangThai = "Đã xác nhận";
                _context.SaveChanges();
            }
            return RedirectToAction("WorkSchedules");
        }

        // GET: Admin/CreateWorkSchedule
        public IActionResult CreateWorkSchedule(int? doctorId, string date)
        {
            ViewBag.Doctors = _context.BacSis.Include(b => b.Khoa).ToList();
            var model = new LichLamViec();
            if (doctorId.HasValue) model.MaBacSi = doctorId.Value;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var d)) model.NgayLamViec = d;
            return View(model);
        }

        // POST: Admin/CreateWorkSchedule
        [HttpPost]
        public async Task<IActionResult> CreateWorkSchedule(int MaBacSi, DateTime NgayLamViec, string GioBatDau, string GioKetThuc, string TrangThai)
        {
            var lich = new LichLamViec
            {
                MaBacSi = MaBacSi,
                NgayLamViec = NgayLamViec,
                ThuTrongTuan = ((int)NgayLamViec.DayOfWeek == 0 ? "8" : ((int)NgayLamViec.DayOfWeek + 1).ToString()),
                GioBatDau = TimeSpan.Parse(GioBatDau),
                GioKetThuc = TimeSpan.Parse(GioKetThuc),
                TrangThai = TrangThai,
                NgayTao = DateTime.Now
            };
            _context.LichLamViecs.Add(lich);
            await _context.SaveChangesAsync();
            return RedirectToAction("WorkSchedules");
        }

        // GET: Admin/EditWorkSchedule/{id}
        [HttpGet]
        public IActionResult EditWorkSchedule(int id)
        {
            ViewBag.Doctors = _context.BacSis.Include(b => b.Khoa).ToList();
            var model = _context.LichLamViecs.Include(l => l.BacSi).FirstOrDefault(l => l.MaLich == id);
            if (model == null) return NotFound();
            return View(model);
        }

        // POST: Admin/EditWorkSchedule
        [HttpPost]
        public async Task<IActionResult> EditWorkSchedule(int MaLich, int MaBacSi, DateTime NgayLamViec, string GioBatDau, string GioKetThuc, string TrangThai)
        {
            var lich = await _context.LichLamViecs.FindAsync(MaLich);
            if (lich == null) return NotFound();
            lich.MaBacSi = MaBacSi;
            lich.NgayLamViec = NgayLamViec;
            lich.ThuTrongTuan = ((int)NgayLamViec.DayOfWeek == 0 ? "8" : ((int)NgayLamViec.DayOfWeek + 1).ToString());
            lich.GioBatDau = TimeSpan.Parse(GioBatDau);
            lich.GioKetThuc = TimeSpan.Parse(GioKetThuc);
            lich.TrangThai = TrangThai;
            await _context.SaveChangesAsync();
            return RedirectToAction("WorkSchedules");
        }

        // GET: Admin/DeleteWorkSchedule/{id}
        [HttpGet]
        public IActionResult DeleteWorkSchedule(int id)
        {
            var model = _context.LichLamViecs.Include(l => l.BacSi).ThenInclude(b => b.Khoa).FirstOrDefault(l => l.MaLich == id);
            if (model == null) return NotFound();
            return View(model);
        }

        // POST: Admin/DeleteWorkSchedule
        [HttpPost, ActionName("DeleteWorkSchedule")]
        public async Task<IActionResult> DeleteWorkScheduleConfirmed(int MaLich)
        {
            var lich = await _context.LichLamViecs.FindAsync(MaLich);
            if (lich != null)
            {
                _context.LichLamViecs.Remove(lich);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("WorkSchedules");
        }

        // [HttpPost]
        public async Task<IActionResult> EditWorkSchedule(int? id, int? doctorId, string Ngay, string GioBatDau, string GioKetThuc, string actionType)
        {
            // ...
            return View();
        }
    }
}