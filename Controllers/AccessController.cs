using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ShopBanDoGiaDung.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using ShopBanDoGiaDung.Common;
using QuanLyShopDoGiaDung.Common;
using ShopBanDoGiaDung.Models;
using ShopBanDoGiaDung.authorize;

namespace ShopBanDoGiaDung.Controllers
{
    public class AccessController : Controller
    {
        private readonly OnlineShopContext _context;

        public AccessController(OnlineShopContext context)
        {
            _context = context;
        }
        public static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");

            }
            return byte2String;

            //nếu bạn muốn các chữ cái in thường thay vì in hoa thì bạn thay chữ "X" in hoa trong "X2" thành "x"
        }
        public IActionResult Login()
        {
            ViewBag.prevouisPage = Request.Headers.Referer.ToString();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginInfo loginInfo)
        {
          try {
              ViewData["action"] = "login";
            var user = await _context.Taikhoans.SingleOrDefaultAsync(c => c.Email == loginInfo.Email);
            if (user == null)
            {
                ViewData["ValidateMessage"] = "Tài khoản không tồn tại";
                return View();
            }
            var f_password = GetMD5(loginInfo.Password);
            if (user.MatKhau != f_password)
            {
                ViewData["ValidateMessage"] = "Mật khẩu không chính xác";
                return View();
            }
            if (user.MatKhau == f_password)
            {
                List<Claim> claims = new List<Claim>()
                  {
                      new Claim(ClaimTypes.NameIdentifier,loginInfo.Email),
                      new Claim("OtherProperties","Example Role")

                  };
                List<Donhang> dg = new List<Donhang>();
                dg = _context.Donhangs.Where(c => c.TinhTrang == 1).ToList();
                int sl = dg.Count();
                HttpContext.Session.SetInt32("so", sl);
               
                //lu thogn tin vao session
                HttpContext.Session.SetString("email", loginInfo.Email);
                HttpContext.Session.SetInt32("Ma", user.MaTaiKhoan);
                //HttpContext.Session.SetString("role", user.Quyen);
                HttpContext.Session.SetString("SDT", user.Sdt);
                HttpContext.Session.SetString("DiaChi", user.DiaChi);
                var data = from tk in _context.Taikhoans
                            join cv in _context.ChucVus on tk.MaCv equals cv.MaCv
                            join qcv in _context.CvQAs on cv.MaCv equals qcv.MaCv
                            join q in _context.Quyens on qcv.MaQ equals q.MaQ
                            join a in _context.ActionTs on qcv.MaA equals a.MaA
                            where (tk.Email == loginInfo.Email)
                            select new AccountRole
                            {
                                MaTaiKhoan = tk.MaTaiKhoan,
                                MaQ = q.MaQ,
                                MaCv = cv.MaCv,
                                TenCV = cv.Ten,
                                TenQ = q.Ten,
                                ControllerName = q.ControllerName,
                                ActionName = q.ActionName,
                                MaA = a.MaA,
                                TenA = a.TenA,
                            };
                List<AccountRole> roles = data.ToList();

                HttpContext.Session.SetJson("QuyenTK", roles);

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    // IsPersistent = loginInfo.KeepLoggedIn
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), properties);
                if(!String.IsNullOrEmpty(loginInfo.previousPage)){
                    return Redirect(loginInfo.previousPage);
                }

                    int soquyen = data.Where(c=>c.MaA==1).Count();

                    Console.WriteLine($"Received soquyen: {soquyen}");
                    if (soquyen == 6 && data.Where(c=>c.MaA==3).FirstOrDefault().MaQ == 7)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Admin");
                }
                }
            return View();

          }catch(Exception ex){
              ViewData["ValidateMessage"] = "Đăng nhập thất bại";
            return View();
             
          }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterInfo registerInfo)
        {
           try{
              ViewData["action"] = "register";
            var user = await _context.Taikhoans.SingleOrDefaultAsync(c => c.Email == registerInfo.Email);
            if (user != null)
            {
                ViewData["ValidateMessage"] = "Tài khoản đã tồn tại";
                 return RedirectToAction("Login", "Access");
            }
            var f_password = GetMD5(registerInfo.Password);
            Taikhoan newTk = new Taikhoan(){
                Ten= registerInfo.Ten ,
                Email = registerInfo.Email,
                MatKhau = f_password,
                DiaChi= registerInfo.DiaChi,
                Sdt=registerInfo.Sdt,
                NgaySinh=registerInfo.NgaySinh,
                MaCv=3,
             
            };
            _context.Taikhoans.Add(newTk);
            await _context.SaveChangesAsync(); 

            List<Claim> claims = new List<Claim>()
                  {
                      new Claim(ClaimTypes.NameIdentifier,registerInfo.Email),
                      new Claim("OtherProperties","Example Role")

                  };
                /*HttpContext.Session.SetString("email", registerInfo.Email);
                HttpContext.Session.SetInt32("Ma", newTk.MaTaiKhoan);

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new AuthenticationProperties()
                   {
                       AllowRefresh = true
                   };
               await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
               new ClaimsPrincipal(claimsIdentity), properties);*/
                //lu thogn tin vao session
                HttpContext.Session.SetString("email", registerInfo.Email);
                HttpContext.Session.SetInt32("Ma", newTk.MaTaiKhoan);
                //HttpContext.Session.SetString("role", user.Quyen);
                HttpContext.Session.SetString("SDT", newTk.Sdt);
                HttpContext.Session.SetString("DiaChi", newTk.DiaChi);
                var data = from tk in _context.Taikhoans
                           join cv in _context.ChucVus on tk.MaCv equals cv.MaCv
                           join qcv in _context.CvQAs on cv.MaCv equals qcv.MaCv
                           join q in _context.Quyens on qcv.MaQ equals q.MaQ
                           join a in _context.ActionTs on qcv.MaA equals a.MaA
                           where (tk.Email == registerInfo.Email)
                           select new AccountRole
                           {
                               MaTaiKhoan = tk.MaTaiKhoan,
                               MaQ = q.MaQ,
                               MaCv = cv.MaCv,
                               TenCV = cv.Ten,
                               TenQ = q.Ten,
                               ControllerName = q.ControllerName,
                               ActionName = q.ActionName,
                               MaA = a.MaA,
                               TenA = a.TenA,
                           };
                List<AccountRole> roles = data.ToList();

                HttpContext.Session.SetJson("QuyenTK", roles);

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    // IsPersistent = loginInfo.KeepLoggedIn
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), properties);
                if (!String.IsNullOrEmpty(registerInfo.previousPage)){
                return Redirect(registerInfo.previousPage);
            }
            return RedirectToAction("Login", "Access");
           }catch(Exception ex) {
            ViewData["ValidateMessage"] = "Đăng ký thất bại";
              return RedirectToAction("Login", "Access");
            }
        }

        public ActionResult Logout()
        {
           
            HttpContext.Session.Clear();
            return RedirectToAction("Index","Home");
        }

    }
}
