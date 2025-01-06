using System.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using ShopBanDoGiaDung.Data;
using ShopBanDoGiaDung.Models;

namespace QuanLyShopDoGiaDung.ViewComponents
{
    public class DanhMucViewComponent: ViewComponent

    {
        private readonly OnlineShopContext _context;
        public DanhMucViewComponent(OnlineShopContext context){
            _context = context;
        }
        public  IViewComponentResult Invoke()
        {
            var idCategory = HttpContext.Request.Query["idCategory"];
            List<Danhmucsanpham> lst =  _context.Danhmucsanphams.ToList();
            ViewBag.idCategory = Convert.ToInt32(idCategory);
            return View(lst);
        }
        
    }
}