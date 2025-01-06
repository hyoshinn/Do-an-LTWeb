using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using QuanLyShopDoGiaDung.Models;
using ShopBanDoGiaDung.Data;
using ShopBanDoGiaDung.Models;

namespace QuanLyShopDoGiaDung.Controllers
{
    public class CartController : Controller
    {
        public const string SessionCart = "sessionCart";
        private readonly ILogger<CartController> _logger;
        private readonly OnlineShopContext _context;

        public CartController(ILogger<CartController> logger, OnlineShopContext context)
        {
            _context = context;
            _logger = logger;
        }

         public IActionResult Index()
            {
                var cart = HttpContext.Session.Get(SessionCart);
                var list = new List<CartModel>();
                var tongtien = 0;
                
                if (cart != null)
                {
                    var json = Encoding.UTF8.GetString(cart);
                     list = JsonSerializer.Deserialize<List<CartModel>>(json);
                     foreach (var item in list)
                        {
                            tongtien +=item.soluong *  Convert.ToInt32(item.sanpham.GiaTien);
                        }
                }
                
                ViewBag.tongtien = tongtien;
                
                return View(list);
            }

         [HttpPost]
        public JsonResult AddItem(int productId)
        {

            var product = _context.Sanphams.FirstOrDefault(c => c.MaSp == productId);
            var cart = HttpContext.Session.Get(SessionCart);
            var countCart = HttpContext.Session.GetInt32("countCart");
            int count = 0;
            
            if(product.SoLuongTrongKho > 0)
            {
                
                if (cart != null)
                {
                    var json = Encoding.UTF8.GetString(cart);
                    var  list = JsonSerializer.Deserialize<List<CartModel>>(json);
                    if (list.Exists(x => x.sanpham.MaSp == productId))
                    {

                        foreach (var item in list)
                        {
                            if (item.sanpham.MaSp == productId)
                            {
                                item.soluong += 1;
                            }
                        }
                    }
                    else
                    {
                        //tạo mới đối tượng cart item
                        var item = new CartModel();
                        item.sanpham = product;
                        item.soluong = 1;
                        list.Add(item);
                       
                    }
                
                 
                    //Gán vào session
                   var jsonSetSession = JsonSerializer.Serialize(list);
                   var byteArrayCart = Encoding.UTF8.GetBytes(jsonSetSession);
                   HttpContext.Session.Set(SessionCart,byteArrayCart);
                   count = list.Count;
                }
                else
                {
                    //tạo mới đối tượng cart item
                    var item = new CartModel();
                    item.sanpham = product;
                    item.soluong = 1;
                    var list = new List<CartModel>();
                    list.Add(item);
                    //Gán vào session
                     var jsonSetSession = JsonSerializer.Serialize(list);
                      var byteArrayCart = Encoding.UTF8.GetBytes(jsonSetSession);
                    HttpContext.Session.Set(SessionCart,byteArrayCart);
                    count = 1;

                    
                }
                HttpContext.Session.SetInt32("countCart", count);
                return Json(new
                {
                    countCart = count, 
                    status = true
                });
            }
            else
            {
                return Json(new
                {
                   
                    status = false
                });
            }          
        }

         public ActionResult Total()
        {
            var cart = HttpContext.Session.Get(SessionCart);
            var json = Encoding.UTF8.GetString(cart);
            var  list = JsonSerializer.Deserialize<List<CartModel>>(json);
            int total = 0;
            foreach (var item in list)
            {
                total += Int32.Parse(item.sanpham.GiaTien.ToString()) * Int32.Parse(item.soluong.ToString());
            }
            ViewBag.tong = total;

            return PartialView();
        }

         public JsonResult Delete(long id)
        {
            var countCart = HttpContext.Session.GetInt32("countCart");
            var cart = HttpContext.Session.Get(SessionCart);
            var json = Encoding.UTF8.GetString(cart);
            var  list = JsonSerializer.Deserialize<List<CartModel>>(json);
            list.RemoveAll(x => x.sanpham.MaSp == id);
            //Gán vào session
            int count = (int)countCart;
            var jsonSetSession = JsonSerializer.Serialize(list);
            var byteArrayCart = Encoding.UTF8.GetBytes(jsonSetSession);
            HttpContext.Session.Set(SessionCart,byteArrayCart);
            HttpContext.Session.SetInt32("countCart", count - 1);
            return Json(new
            {
                status = true
            });
        }


         [HttpPost]
        public JsonResult Update(int productId, int amount)
        {
           var product = _context.Sanphams.FirstOrDefault(c => c.MaSp == productId);
            var countCart = HttpContext.Session.GetInt32("countCart");
            var cart = HttpContext.Session.Get(SessionCart);
            var json = Encoding.UTF8.GetString(cart);
            var  list = JsonSerializer.Deserialize<List<CartModel>>(json);
            float price = 0;
            float tongtien = 0;
            string kiemtrahethang = "";
            if(amount > product.SoLuongTrongKho) {
                kiemtrahethang = "hethang";
            }
            if(amount <= 0){
                list.RemoveAll(x => x.sanpham.MaSp == productId);
                int count = (int)countCart;
                HttpContext.Session.SetInt32("countCart", count - 1);
            }

            foreach (var item in list)
            {

                if(item.sanpham.MaSp == productId){
                    item.soluong = amount;
                    price = amount * Convert.ToInt32(item.sanpham.GiaTien);
                     
                }
                tongtien += item.soluong * Convert.ToInt32(item.sanpham.GiaTien);
            }
           var jsonSetSession = JsonSerializer.Serialize(list);
            var byteArrayCart = Encoding.UTF8.GetBytes(jsonSetSession);
            HttpContext.Session.Set(SessionCart,byteArrayCart);
            return Json(new
            {
                status = true,
                productId = productId ,
                price = price,
                tongtien = tongtien,
                countCart = list.Count,
                kiemtrahethang = kiemtrahethang
                
            });
        }

        public JsonResult DeleteAll(){
            HttpContext.Session.Remove(SessionCart);
            HttpContext.Session.Remove("countCart");
            return Json( new {
                status = true
            });
        }
        
        [HttpPost]
        public async Task<JsonResult> ThanhToan(ThongTinThanhToan thanhToan) {
                try {
                    if( String.IsNullOrEmpty(thanhToan.ten)) {
                          return Json(new
                            {
                                status = false,
                                message = "Họ tên người nhận không được bỏ trống"
                            });

                    }
                    if( String.IsNullOrEmpty(thanhToan.SDT)) {
                          return Json(new
                            {
                                status = false,
                                message = "Số điện thoại không được bỏ trống"
                            });

                    }
                    if( String.IsNullOrEmpty(thanhToan.DiaChi)) {
                          return Json(new
                            {
                                status = false,
                                message = "Địa chỉ không được bỏ trống"
                            });

                    }
                    var order = new Donhang();
                    order.MaTaiKhoan = HttpContext.Session.GetInt32("Ma");
                    order.NgayLap = DateTime.Now; /*Convert.ToDateTime("2/2/2022")*/;
                    var cart = HttpContext.Session.Get(SessionCart);
                    var json = Encoding.UTF8.GetString(cart);
                    var  list = JsonSerializer.Deserialize<List<CartModel>>(json);
                    long total = 0;
                    foreach (var item in list)
                    {
                        total +=item.soluong *  Convert.ToInt32(item.sanpham.GiaTien);
                    }
                    order.TongTien = total;
                    order.TinhTrang = 1;
                    //Thêm Order               
                    _context.Donhangs.Add(order);
                    await _context.SaveChangesAsync();
                    var id = order.MaDonHang;
                    var vc = new Vanchuyen();
                    vc.MaDonHang = id;
                    vc.NguoiNhan = thanhToan.ten;
                    vc.DiaChi =thanhToan.DiaChi;
                    vc.Sdt = thanhToan.SDT;
                    vc.HinhThucVanChuyen = "Giao tận nhà";
                    _context.Vanchuyens.Add(vc);
                    foreach (var item in list)
                    {
                        var it = _context.Sanphams.Find(item.sanpham.MaSp);
                        var orderDetail = new Chitietdonhang();
                        orderDetail.MaDonHang = id;
                        orderDetail.MaSp = item.sanpham.MaSp;
                        orderDetail.SoLuongMua = item.soluong;
                        it.SoLuongDaBan += item.soluong;
                        it.SoLuongTrongKho -= item.soluong;
                        _context.Chitietdonhangs.Add(orderDetail);
                    }
                    await _context.SaveChangesAsync();
                     HttpContext.Session.Remove(SessionCart);      
                     HttpContext.Session.Remove("countCart");           
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
            return View("Error!");
        }
    }
}