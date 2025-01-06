using System;
using System.Collections.Generic;

namespace ShopBanDoGiaDung.Models;

public partial class Vanchuyen
{
    public int MaDonHang { get; set; }

    public string? NguoiNhan { get; set; }

    public string? DiaChi { get; set; }

    public string? Sdt { get; set; }

    public string? HinhThucVanChuyen { get; set; }

    public virtual Donhang MaDonHangNavigation { get; set; } = null!;
}
