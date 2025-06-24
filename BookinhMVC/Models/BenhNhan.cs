using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookinhMVC.Models
{
    public class BenhNhan
    {
        // Shared primary key với NguoiDung: ngăn EF tự tạo giá trị
        [Key, ForeignKey("NguoiDung")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaBenhNhan { get; set; }

        [Required]
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string SoBaoHiem { get; set; }
        public string HinhAnhBenhNhan { get; set; }
        public DateTime NgayTao { get; set; }

        public virtual NguoiDung NguoiDung { get; set; }
        public virtual ICollection<DanhGia> DanhGias { get; set; }
        public virtual ICollection<HoSoBenhAn> HoSoBenhAns { get; set; }
        public virtual ICollection<LichHen> LichHens { get; set; }
    }
}
