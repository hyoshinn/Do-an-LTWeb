using System;
using System.Collections.Generic;

namespace ShopBanDoGiaDung.Models;

public partial class Chitietsanpham
{
    public int MaSp { get; set; }

    public int? CanNang { get; set; }

    public string? KichThuoc { get; set; }

    public string? CongSuat { get; set; }

    public string? NoiSanXuat { get; set; }

    public string? NamSanXuat { get; set; }

    public virtual Sanpham MaSpNavigation { get; set; } = null!;
}
