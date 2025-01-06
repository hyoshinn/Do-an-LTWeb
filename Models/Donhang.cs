using System;
using System.Collections.Generic;

namespace ShopBanDoGiaDung.Models;

public partial class Donhang
{
    public int MaDonHang { get; set; }

    public int? MaTaiKhoan { get; set; }

    public long? TongTien { get; set; }

    public int? TinhTrang { get; set; }

    public DateTime? NgayLap { get; set; }

    public virtual ICollection<Chitietdonhang> Chitietdonhangs { get; set; } = new List<Chitietdonhang>();

    public virtual Taikhoan? MaTaiKhoanNavigation { get; set; }

    public virtual Vanchuyen? Vanchuyen { get; set; }
}
