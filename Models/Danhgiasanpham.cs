using System;
using System.Collections.Generic;

namespace ShopBanDoGiaDung.Models;

public partial class Danhgiasanpham
{
    public int MaTaiKhoan { get; set; }

    public int MaSp { get; set; }

    public int? DanhGia { get; set; }

    public string? NoiDungBinhLuan { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual Sanpham MaSpNavigation { get; set; } = null!;

    public virtual Taikhoan MaTaiKhoanNavigation { get; set; } = null!;
}
