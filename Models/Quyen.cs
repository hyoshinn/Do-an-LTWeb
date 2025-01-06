using System;
using System.Collections.Generic;

namespace ShopBanDoGiaDung.Models;

public partial class Quyen
{
    public int MaQ { get; set; }

    public string? ActionName { get; set; }

    public string? Ten { get; set; }

    public string? ControllerName { get; set; }

    public virtual ICollection<CvQA> CvQAs { get; set; } = new List<CvQA>();
}
