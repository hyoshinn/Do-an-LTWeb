using System;
using System.Collections.Generic;

namespace ShopBanDoGiaDung.Models;

public partial class Sanpham
{
    public int MaSp { get; set; }

    public string? TenSp { get; set; }

    public string? MoTa { get; set; }

    public string? Anh1 { get; set; }

    public string? Anh2 { get; set; }

    public string? Anh3 { get; set; }

    public string? Anh4 { get; set; }

    public string? Anh5 { get; set; }

    public string? Anh6 { get; set; }

    public int? SoLuongDaBan { get; set; }

    public int? SoLuongTrongKho { get; set; }

    public long? GiaTien { get; set; }

    public int? MaHang { get; set; }

    public int? MaDanhMuc { get; set; }

    public virtual ICollection<Chitietdonhang> Chitietdonhangs { get; set; } = new List<Chitietdonhang>();

    public virtual Chitietsanpham? Chitietsanpham { get; set; }

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual Danhmucsanpham? MaDanhMucNavigation { get; set; }

    public virtual Hangsanxuat? MaHangNavigation { get; set; }
}
