

using System.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using ShopBanDoGiaDung.Data;
using ShopBanDoGiaDung.Models;

namespace QuanLyShopDoGiaDung.ViewComponents
{
    public class TimKiemKhacViewComponent : ViewComponent
    {
        private readonly OnlineShopContext _context;
        public TimKiemKhacViewComponent(OnlineShopContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var idHang = HttpContext.Request.Query["idHang"];
            List<Hangsanxuat> lst = _context.Hangsanxuats.ToList();
            ViewBag.idHang = Convert.ToInt32(idHang);
            return View(lst);
        }
    }
}