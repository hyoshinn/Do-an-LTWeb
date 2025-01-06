using System;
using System.Collections.Generic;

namespace ShopBanDoGiaDung.Models;

public partial class ChucVu
{
    public int MaCv { get; set; }

    public string? Ten { get; set; }

    public virtual ICollection<CvQA> CvQAs { get; set; } = new List<CvQA>();

    public virtual ICollection<Taikhoan> Taikhoans { get; set; } = new List<Taikhoan>();
}
