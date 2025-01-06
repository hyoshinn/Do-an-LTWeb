using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyShopDoGiaDung.DTO
{
    public class TaiKhoanDto
    {
      public int MaTaiKhoan { get; set; } 
        public string? Ten { get; set; }

      public DateTime? NgaySinh { get; set; }

      public string? Sdt { get; set; }

      public string? DiaChi { get; set; }

      public string? Email { get; set; }
    }
}