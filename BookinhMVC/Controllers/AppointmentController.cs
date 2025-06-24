using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookinhMVC.Models;
using System.Globalization;

namespace BookinhMVC.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly BookingContext _context;
        public AppointmentController(BookingContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Book(int? selectedDoctorId = null)
        {
            ViewBag.Doctors = await _context.BacSis.Include(b => b.Khoa).ToListAsync();
            ViewBag.SelectedDoctorId = selectedDoctorId ?? 0;

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var patient = await _context.BenhNhans.FindAsync(userId.Value);
                ViewBag.PatientPhone = patient?.SoDienThoai ?? "";
                ViewBag.PatientEmail = patient?.Email ?? "";
            }
            else
            {
                ViewBag.PatientPhone = "";
                ViewBag.PatientEmail = "";
            }
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetAvailableDates(int doctorId)
        {
            var dates = await _context.LichLamViecs
                .Where(l => l.MaBacSi == doctorId && l.TrangThai == "Đã xác nhận" && l.NgayLamViec >= DateTime.Today)
                .Select(l => l.NgayLamViec.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            return Json(dates);
        }

        [HttpGet]
        public async Task<JsonResult> GetAvailableTimes(int doctorId, string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedDate))
                return Json(new List<string>());

            // Tạm thời bỏ hết filter, chỉ lấy tất cả lịch làm việc của bác sĩ này
            var workSchedules = await _context.LichLamViecs
                .Where(lv => lv.MaBacSi == doctorId)
                .ToListAsync();

            var bookedTimes = await _context.LichHens
                .Where(l => l.MaBacSi == doctorId)
                .Select(l => l.NgayGio.TimeOfDay)
                .ToListAsync();

            // DEBUG: Trả về số lượng workSchedules và bookedTimes để kiểm tra
            if (workSchedules.Count == 0)
                return Json(new { times = new List<string>(), debug = "No workSchedules" });

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
                        availableTimes.Add(t.ToString(@"hh\:mm"));
                    }
                }
            }
            // Nếu không có giờ nào, trả về debug
            if (availableTimes.Count == 0)
                return Json(new { times = availableTimes, debug = $"workSchedules: {workSchedules.Count}, bookedTimes: {bookedTimes.Count}" });

            return Json(new { times = availableTimes });
        }

        private async Task<List<string>> GetAvailableTimesForDoctor(int doctorId, DateTime date)
        {
            var workSchedules = await _context.LichLamViecs
                .Where(lv => lv.MaBacSi == doctorId && lv.NgayLamViec >= date.Date && lv.NgayLamViec < date.Date.AddDays(1) && lv.TrangThai == "Đã xác nhận")
                .OrderBy(lv => lv.GioBatDau)
                .ToListAsync();

            var bookedTimes = await _context.LichHens
                .Where(l => l.MaBacSi == doctorId
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
                        availableTimes.Add(t.ToString(@"hh\:mm"));
                    }
                }
            }
            return availableTimes;
        }

        [HttpPost]
        public async Task<IActionResult> Book(int selectedDoctorId, DateTime selectedDate, string selectedTime, string symptoms)
        {
            ViewBag.Doctors = await _context.BacSis.Include(b => b.Khoa).ToListAsync();
            ViewBag.SelectedDoctorId = selectedDoctorId;

            int? selectedPatientId = HttpContext.Session.GetInt32("UserId");

            if (selectedDoctorId == 0 || selectedPatientId == null || selectedDate == default || string.IsNullOrEmpty(selectedTime))
            {
                ViewBag.Message = "Vui lòng điền đầy đủ thông tin.";
                return View();
            }

            var appointmentDateTime = selectedDate.Date.Add(TimeSpan.Parse(selectedTime));

            // Kiểm tra trùng lịch
            var exists = await _context.LichHens.AnyAsync(l =>
                l.MaBacSi == selectedDoctorId &&
                l.NgayGio == appointmentDateTime &&
                l.TrangThai != "Đã hủy");
            if (exists)
            {
                ViewBag.Message = "Thời gian này đã có lịch hẹn. Vui lòng chọn thời gian khác.";
                return View();
            }

            var appointment = new LichHen
            {
                MaBenhNhan = selectedPatientId.Value,
                MaBacSi = selectedDoctorId,
                NgayGio = appointmentDateTime,
                TrieuChung = symptoms,
                TrangThai = "Chờ xác nhận"
            };

            _context.LichHens.Add(appointment);
            await _context.SaveChangesAsync();
            ViewBag.Message = "Đặt lịch thành công! Vui lòng chờ xác nhận.";
            return View();
        }
    }
}