using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookinhMVC.Models
{
    public class DanhGia
    {
        [Key]
        public int MaDanhGia { get; set; }

        [ForeignKey("BacSi")]
        public int MaBacSi { get; set; }

        [ForeignKey("BenhNhan")]
        public int MaBenhNhan { get; set; }

        [Range(1, 5)]
        public int DiemDanhGia { get; set; }

        public string NhanXet { get; set; }
        public DateTime NgayDanhGia { get; set; }

        public virtual BacSi BacSi { get; set; }
        public virtual BenhNhan BenhNhan { get; set; }
    }
}
