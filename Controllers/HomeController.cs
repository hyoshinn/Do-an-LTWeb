using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyShopDoGiaDung.authorize;
using ShopBanDoGiaDung.Data;
using ShopBanDoGiaDung.Models;
using System.Diagnostics;
using QuanLyShopDoGiaDung.Common;
using QuanLyShopDoGiaDung.DTO;
using System.Text.Json;


namespace ShopBanDoGiaDung.Controllers
{
    public class HomeController : Controller
    {
        private readonly OnlineShopContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, OnlineShopContext context)
        {
            _logger = logger;
            _context = context;
        }

      /*  [CustomAuthorize("khach")]*/
        public async Task<IActionResult> Index()
        {
            var sanpham = (from a in _context.Sanphams
                           orderby a.SoLuongDaBan descending
                           select a).Take(6);
            var model =await sanpham.ToListAsync();
            ViewBag.sanpham = model;
            ViewBag.danhmucsp =await _context.Danhmucsanphams.ToListAsync();
            ViewBag.hang =await _context.Hangsanxuats.ToListAsync();
            return View();
        }

        public async Task<IActionResult> SPHang(int idHang, string ten,int PageIndex, int PageSize,  int maxPrice, int minPrice, string orderPrice)
        {
            if(PageIndex == 0 || PageSize == 0 ){
                PageIndex = 1;
                PageSize = 100;
            }
            ViewBag.tenhang = ten;
            IQueryable<Sanpham> query = (IQueryable<Sanpham>)_context.Sanphams;
            if (maxPrice != 0){
               query = query.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice && item.MaHang == idHang )    ;
            }
            else {
                query = query.Where(s=> s.MaHang == idHang);
            }
            if(orderPrice == "tang") {
                query = query.OrderBy(item => item.GiaTien);
            }
            if(orderPrice == "giam") {
                query = query.OrderByDescending(item => item.GiaTien);
            }
            List<Sanpham> model =await query.ToListAsync();
            var count =await query.CountAsync();
             PaginatedList<Sanpham> data = new PaginatedList<Sanpham>(model,count,PageIndex, PageSize );     
            ViewBag.sanpham = data;
            ViewBag.maxPrice = maxPrice;
            ViewBag.minPrice = minPrice;
            ViewBag.orderPrice = orderPrice;
            ViewBag.tenhang = ten;
            ViewBag.idHang = idHang;
            return View();
        }
        public async Task<IActionResult> SPDanhMuc(int idCategory, string ten, int PageIndex, int PageSize,  int maxPrice, int minPrice, string orderPrice)
        {

            if(PageIndex == 0 || PageSize == 0 ){
                PageIndex = 1;
                PageSize = 100;
            }
          
            IQueryable<Sanpham> query = (IQueryable<Sanpham>)_context.Sanphams;      
            if (maxPrice != 0){
               query = query.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice && item.MaDanhMuc == idCategory );
            }
            else {
                query = query.Where(s=> s.MaDanhMuc == idCategory);
            }
            if(orderPrice == "tang") {
                query = query.OrderBy(item => item.GiaTien);
            }
            if(orderPrice == "giam") {
                query = query.OrderByDescending(item => item.GiaTien);
            }
            List<Sanpham> model =await query.ToListAsync();
            var count =await query.CountAsync();
             PaginatedList<Sanpham> data = new PaginatedList<Sanpham>(model,count,PageIndex, PageSize );     
            ViewBag.sanpham = data;
            ViewBag.maxPrice = maxPrice;
            ViewBag.minPrice = minPrice;
            ViewBag.orderPrice = orderPrice;
            ViewBag.tendanhmuc = ten;
            ViewBag.idCategory = idCategory;
            return View();
        }

         public async Task<IActionResult> ProductDetail(int id)
        {

            var danhgia = from a in _context.Taikhoans
                          join b in _context.Danhgiasanphams on a.MaTaiKhoan equals b.MaTaiKhoan
                          join c in _context.Sanphams on b.MaSp equals c.MaSp
                          where c.MaSp == id
                          orderby b.NgayDanhGia descending
                          select new CommentView()
                          {
                              TenTaiKhoan = a.Ten,
                              DanhGia = b.DanhGia,
                              NoiDung = b.NoiDungBinhLuan,
                              ThoiGian = b.NgayDanhGia
                          };
            var dsdanhgia =await danhgia.ToListAsync();
            int? sum = 0;
            foreach (var item in dsdanhgia)
            {
                sum += item.DanhGia;
            }
            double sao = Math.Round((double)sum / dsdanhgia.Count(), 1);
            ViewBag.sao = sao;
            //var danhgia = obj.DANHGIASANPHAMs.Where(s => s.MaSP.Equals(id) && s.MaTaiKhoan.Equals(makh)).ToList();
            var model = _context.Sanphams.Find(id);
            ViewBag.sanpham = model;
            ViewBag.danhgia = dsdanhgia;
            return View();
        }

       [HttpGet]
        public async Task<ActionResult> AllProduct(int PageIndex, int PageSize, int maxPrice, int minPrice, string orderPrice)
        {
            if(PageIndex == 0 || PageSize == 0){
                PageIndex = 1;
                PageSize=100;
            }
            IQueryable<Sanpham> model = (IQueryable<Sanpham>)_context.Sanphams;
            if (maxPrice != 0){
               model = model.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice);
            }
            if(orderPrice == "tang") {
                model = model.OrderBy(item => item.GiaTien);
            }
            if(orderPrice == "giam") {
                model = model.OrderByDescending(item => item.GiaTien);
            }
             var count =await model.CountAsync();
            List<Sanpham> dt =await model.Skip((PageIndex -1 )* PageSize).Take(PageSize).ToListAsync();
            PaginatedList<Sanpham> data = new PaginatedList<Sanpham>(dt,count,PageIndex, PageSize );     
            ViewBag.sanpham = data;
            ViewBag.maxPrice = maxPrice;
            ViewBag.minPrice = minPrice;
            ViewBag.orderPrice = orderPrice;
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> AllProduct2(int PageIndex, int PageSize,string idHangs, string idCategories, string maxPrice, string minPrice, string search)
        {   

            IQueryable<Sanpham> model = (IQueryable<Sanpham>)_context.Sanphams;
             
            if(!String.IsNullOrEmpty(search)) {
                model = model.Where((item) => item.TenSp.Contains(search));
            }
            if (!String.IsNullOrEmpty(idHangs)) {
                 int[] arrayIdHang = Array.ConvertAll(idHangs.Split(","), int.Parse);
                 model = model.Where((item) => arrayIdHang.Contains(item.MaHang??0));
            }

            if(!String.IsNullOrEmpty(idCategories)) {
                int[] arrayCategory = Array.ConvertAll(idCategories.Split(","), int.Parse);
                model  = model.Where((item) => arrayCategory.Contains((item.MaDanhMuc??0)));
            }
            if(!String.IsNullOrEmpty(maxPrice)) {
                int priceMax = Convert.ToInt32(maxPrice);
                model = model.Where((item) => item.GiaTien < priceMax);
            }
             if(!String.IsNullOrEmpty(minPrice)) {
                int priceMin = Convert.ToInt32(minPrice);
                model = model.Where((item) => item.GiaTien > priceMin);
            }

            
            List<Sanpham> dt =await model.Skip((PageIndex -1 )* PageSize).Take(PageSize).ToListAsync();
           
             var count =await model.CountAsync();
            PaginatedList<Sanpham> data = new PaginatedList<Sanpham>(dt,count,PageIndex, PageSize );   
              ViewBag.sanpham = data;  
              ViewBag.idHangs = idHangs;
              ViewBag.idCategories = idCategories;
              ViewBag.maxPrice = maxPrice;
              ViewBag.minPrice = minPrice;
              ViewBag.search = search;
            return View();
        }

        public async Task<ActionResult> Search(string search,int PageIndex, int PageSize,  int maxPrice, int minPrice, string orderPrice )
        {
             if(PageIndex == 0 || PageSize == 0){
                PageIndex = 1;
                PageSize=100;
            }
            IQueryable<Sanpham> model = (IQueryable<Sanpham>)_context.Sanphams.Where(s => s.TenSp.Contains(search));
            if (maxPrice != 0){
               model = model.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice && item.TenSp.Contains(search) );
            }
            else {
                model = model.Where(s => s.TenSp.Contains(search));
            }
            if(orderPrice == "tang") {
                model = model.OrderBy(item => item.GiaTien);
            }
            if(orderPrice == "giam") {
                model = model.OrderByDescending(item => item.GiaTien);
            }
            var count = await model.CountAsync();
            List<Sanpham> dt =await model.Skip((PageIndex -1 )* PageSize).Take(PageSize).ToListAsync();
            PaginatedList<Sanpham> data = new PaginatedList<Sanpham>(dt,count,PageIndex, PageSize ); 
            ViewBag.sanpham = data;
            ViewBag.search = search;
             ViewBag.maxPrice = maxPrice;
            ViewBag.minPrice = minPrice;
            ViewBag.orderPrice = orderPrice;
            return View();
        }


        public async Task<IActionResult> profile()
        {
            var ma =  HttpContext.Session.GetInt32("Ma");
            var model =await _context.Taikhoans.Where(s => s.MaTaiKhoan == ma).ToListAsync();
            ViewBag.taikhoan = model;
            return View();
        }
        

         [HttpPost]
        public async Task<ActionResult> ChangeProfile( TaiKhoanDto tk)
        {
            var it =await _context.Taikhoans.FindAsync(tk.MaTaiKhoan);
            if(it == null) {
                return RedirectToAction("profile");
            }
            it.Ten = tk.Ten;
            it.Email = tk.Email;
            it.DiaChi = tk.DiaChi;
            it.Sdt = tk.Sdt;
            it.NgaySinh = tk.NgaySinh;
            HttpContext.Session.SetString("email",it.Email);
            HttpContext.Session.SetString("SDT", it.Sdt);
            HttpContext.Session.SetString("DiaChi", it.DiaChi);
            await _context.SaveChangesAsync();
            return RedirectToAction("profile");
        }


         public async Task<ActionResult> MyOrder(string typeMenu, int PageIndex, int PageSize)
        {
           
           var ma = HttpContext.Session.GetInt32("Ma");

            if(String.IsNullOrEmpty(typeMenu)){
                typeMenu = "tatca";
            }
            if(PageIndex == 0 || PageSize == 0){
                PageIndex = 1;
                PageSize=100;
            }
            IQueryable<Donhang> query = (IQueryable<Donhang>)_context.Donhangs;
            query = typeMenu switch
            {
                "tatca" => query.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma),
                "chuathanhtoan" => query.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma && a.TinhTrang == 0),
                "choxacnhan" => query.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma && a.TinhTrang == 1),
                "dangvanchuyen" => query.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma && a.TinhTrang == 2),
                "dahoanthanh" => query.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma && a.TinhTrang == 3),
                "dahuy" => query.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma && a.TinhTrang == 4),
                _ => query.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma),
            };
            List<Donhang> dt = await query.OrderByDescending(item => item.MaDonHang)
            .Skip((PageIndex -1 )* PageSize).Take(PageSize)
            .ToListAsync();
            var count = await query.CountAsync();
            PaginatedList<Donhang> data = new PaginatedList<Donhang>(dt,count,PageIndex, PageSize );
            ViewBag.donhang = data;  
            ViewBag.typeMenu = typeMenu;
             Console.WriteLine(ViewBag.typeMenu);
            
            return View();
        }

         public ActionResult MyOrderDetail(int id)
        {
            var kq = from a in _context.Chitietdonhangs
                     join b in _context.Sanphams on a.MaSp equals b.MaSp
                     where a.MaDonHang == id
                     select new MyOrderDetail()
                     {
                         MaSanPham = b.MaSp,
                         TenSP = b.TenSp,
                         Anh = b.Anh1,
                         GiaBan = b.GiaTien,
                         SoLuong = a.SoLuongMua,
                         ThanhTien = b.GiaTien * a.SoLuongMua
                     };
            var ds = kq.ToList();
            return PartialView(ds);
        }

        public async Task<JsonResult> HuyDonHang(int ma)
        {
           try{
             var dh =await _context.Donhangs.FindAsync(ma);
            if(dh == null) {
                return Json(
                    new {
                        status = true
                    }
                );
            }
            dh.TinhTrang = 4;
            await _context.SaveChangesAsync();
            return Json(new
            {
                status = true
            });

           }catch (Exception ex) {
            Console.WriteLine(ex);
              return Json(new
            {
                status = false
            });
           }
        }

        public async Task<JsonResult> DaNhanHang(int ma)
        {
            try {
                var dh =await _context.Donhangs.FindAsync(ma);
                dh.TinhTrang = 3;
                await _context.SaveChangesAsync();
                return Json(new
                {
                    status = true
                });

           }catch (Exception ex) {
            Console.WriteLine(ex);
              return Json(new
            {
                status = false
            });
           }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}