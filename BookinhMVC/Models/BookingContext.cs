using Microsoft.EntityFrameworkCore;

namespace BookinhMVC.Models
{
    public class BookingContext : DbContext
    {
        public BookingContext(DbContextOptions<BookingContext> options) : base(options) { }

        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<Khoa> Khoas { get; set; }
        public DbSet<BacSi> BacSis { get; set; }
        public DbSet<BenhNhan> BenhNhans { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<CsKh> CsKhs { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<HoSoBenhAn> HoSoBenhAns { get; set; }
        public DbSet<LichHen> LichHens { get; set; }
        public DbSet<LichLamViec> LichLamViecs { get; set; }
        public DbSet<Question> Questions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình quan hệ 1-1 giữa BacSi và NguoiDung
            modelBuilder.Entity<BacSi>()
                .HasOne(b => b.NguoiDung)
                .WithOne(n => n.BacSi)
                .HasForeignKey<BacSi>(b => b.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ 1-1 giữa BenhNhan và NguoiDung với shared primary key
            // Thiết lập khóa chính cho bảng BenhNhans và báo cho EF không tạo tự động giá trị
            modelBuilder.Entity<BenhNhan>()
                .HasKey(b => b.MaBenhNhan);

            modelBuilder.Entity<BenhNhan>()
                .Property(b => b.MaBenhNhan)
                .ValueGeneratedNever();

            modelBuilder.Entity<BenhNhan>()
                .HasOne(b => b.NguoiDung)
                .WithOne(u => u.BenhNhan)
                .HasForeignKey<BenhNhan>(b => b.MaBenhNhan)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình DanhGia để tránh vấn đề multiple cascade paths
            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.BacSi)
                .WithMany(b => b.DanhGias)
                .HasForeignKey(d => d.MaBacSi)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.BenhNhan)
                .WithMany(b => b.DanhGias)
                .HasForeignKey(d => d.MaBenhNhan)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình HoSoBenhAn
            modelBuilder.Entity<HoSoBenhAn>()
                .HasOne(h => h.BacSi)
                .WithMany(b => b.HoSoBenhAns)
                .HasForeignKey(h => h.MaBacSi)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HoSoBenhAn>()
                .HasOne(h => h.BenhNhan)
                .WithMany(b => b.HoSoBenhAns)
                .HasForeignKey(h => h.MaBenhNhan)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình LichHen
            modelBuilder.Entity<LichHen>()
                .HasOne(l => l.BacSi)
                .WithMany(b => b.LichHens)
                .HasForeignKey(l => l.MaBacSi)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichHen>()
                .HasOne(l => l.BenhNhan)
                .WithMany(b => b.LichHens)
                .HasForeignKey(l => l.MaBenhNhan)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
