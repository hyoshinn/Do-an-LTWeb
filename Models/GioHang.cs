using System;
using System.Collections.Generic;

namespace ShopBanDoGiaDung.Models;

public partial class GioHang
{
    public int MaTaiKhoan { get; set; }

    public int MaSp { get; set; }

    public int? Soluong { get; set; }

    public virtual Sanpham MaSpNavigation { get; set; } = null!;

    public virtual Taikhoan MaTaiKhoanNavigation { get; set; } = null!;
}
