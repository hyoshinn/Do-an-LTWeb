using System;
using System.Collections.Generic;

namespace ShopBanDoGiaDung.Models;

public partial class Hangsanxuat
{
    public int MaHang { get; set; }

    public string? TenHang { get; set; }

    public virtual ICollection<Sanpham> Sanphams { get; set; } = new List<Sanpham>();
}
