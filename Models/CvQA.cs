using System;
using System.Collections.Generic;

namespace ShopBanDoGiaDung.Models;

public partial class CvQA
{
    public int MaA { get; set; }

    public int MaCv { get; set; }

    public int MaQ { get; set; }

    public string? Ten { get; set; }

    public virtual ActionT MaANavigation { get; set; } = null!;

    public virtual ChucVu MaCvNavigation { get; set; } = null!;

    public virtual Quyen MaQNavigation { get; set; } = null!;
}
