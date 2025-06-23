using System.ComponentModel.DataAnnotations;

namespace BookinhMVC.Models
{
    public class LichHen
    {
        [Key]
        public int MaLich { get; set; }
        public int MaBenhNhan { get; set; }
        public int MaBacSi { get; set; }
        public DateTime NgayGio { get; set; }
        public string TrieuChung { get; set; }
        public string TrangThai { get; set; }

        public BenhNhan BenhNhan { get; set; }
        public BacSi BacSi { get; set; }
    }
}
