using Microsoft.AspNetCore.Mvc;
using ShopBanDoGiaDung.Data;
using System;
using ShopBanDoGiaDung.Models;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Drawing.Printing;
using X.PagedList;
using System.Drawing;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShopBanDoGiaDung.authorize;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ShopBanDoGiaDung.Controllers

{
    public class AdminController : Controller
    {
        private readonly OnlineShopContext obj;
        public AdminController(OnlineShopContext obj)
        {
            this.obj = obj;
        }
        public IActionResult Index()
        {
            var sanpham = obj.Sanphams.ToList();
            ViewBag.sanpham = sanpham;
            return View();
        }
       
        #region Quản lý
        #region Quản lý quyền hạn
        public IActionResult QuanLyQH(int page = 1, int pageSize = 10)
        {
            var query = from cv in obj.ChucVus
                        join qcv in obj.CvQAs on cv.MaCv equals qcv.MaCv
                        join q in obj.Quyens on qcv.MaQ equals q.MaQ
                        join a in obj.ActionTs on qcv.MaA equals a.MaA
                        select new ChucVuQuyen
                        {
                            MaCv = cv.MaCv,
                            MaQ= q.MaQ,
                            TenCV=cv.Ten,
                            TenQ=q.Ten,
                            ActionName=q.ActionName,
                            ControllerName=q.ControllerName,
                            MaA=qcv.MaA,
                            TenA=a.TenA
                        };
            List<ChucVuQuyen> roles = query.ToList();

            HttpContext.Session.SetJson("HD", roles);
            var quyen = obj.Quyens.ToList();
            ViewBag.quyen = quyen;
            var chucvu = obj.ChucVus.ToList();
            ViewBag.chucvu = chucvu;
            
            var A= obj.ActionTs.ToList();
            ViewBag.A = A;
            var model = chucvu.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            // Tính toán thông tin phân trang
            var totalItemCount = chucvu.Count();
            var pagedList = new StaticPagedList<ChucVu>(model, page, pageSize, totalItemCount);
            ViewBag.PageStartItem = (page - 1) * pageSize + 1;
            ViewBag.PageEndItem = Math.Min(page * pageSize, totalItemCount);
            ViewBag.Page = page;
            ViewBag.TotalItemCount = totalItemCount;
            return View(pagedList);

        }
        [HttpPost]
        public IActionResult SuaQH(int macv, int maa, int maq, string newValue)
        {
            Console.WriteLine($"Received macv: {macv}, maq: {maq}, maa: {maa}, newValue: {newValue}");

            try
            {
                if (ModelState.IsValid)
                {
                    

                    var tk = obj.CvQAs.FirstOrDefault(c => c.MaA == maa && c.MaQ == maq && c.MaCv == macv);

                    if (tk != null)
                    {
                        // Xóa đối tượng cũ
                        obj.CvQAs.Remove(tk);
                        obj.SaveChanges();

                        // Thêm đối tượng mới
                        var newCvQA = new CvQA
                        {
                            MaA = Convert.ToInt32(newValue), // hoặc là giá trị mới của MaA
                            MaQ = maq,
                            MaCv = macv
                            // Thêm các thuộc tính khác nếu cần
                        };

                        obj.CvQAs.Add(newCvQA);
                        obj.SaveChanges();

                        return Json(new { success = true, message = "Update successful" });
                    }


                    else
                    {
                        // Trả về JSON hoặc một thông báo lỗi tùy thuộc vào yêu cầu của bạn
                        return Json(new { success = false, message = "Record not found" });
                    }
                }
                else
                {
                    // Trả về JSON hoặc một thông báo lỗi về dữ liệu đầu vào không hợp lệ
                    return Json(new { success = false, message = "Invalid input data" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SuaQH: {ex.Message}");
                // Xử lý lỗi nếu có
                return Json(new { success = false, message = "Error updating record" });
            }

        }
        public IActionResult ThemCV(string tencv)
        {
            if (tencv != null)
            {
                int macv = obj.ChucVus.Max(c => c.MaCv);
                var cv = new ChucVu();
                cv.Ten = tencv;
                cv.MaCv = macv+1;
                obj.ChucVus.Add(cv);
                List<int> maq = obj.Quyens.Select(c => c.MaQ).ToList();
                foreach (var q in maq)
                {
                    var ac = new CvQA();
                    ac.MaCv = macv + 1;
                    ac.MaA = 1;
                    ac.MaQ = q;
                    obj.CvQAs.Add(ac);
                }
                obj.SaveChanges();
                return Json(new
                {
                    status = true
                });
            }

            return Json(new
            {
                status = false
            });

        }
        #endregion
        #region Quản lý tài khoản
        public IActionResult QuanLyTK(int matk, DateTime ngaysinh, string tenctk, string tendc, string sdt, string email, int chvu, int page = 1, int pageSize = 10)
        {
            // Thực hiện truy vấn và phân trang
            var query = from tk in obj.Taikhoans
                        join cv in obj.ChucVus on tk.MaCv equals cv.MaCv
                        select new TaiKhoanChucVu
                        {
                            MaTaiKhoan = tk.MaTaiKhoan,
                            Ten = tk.Ten,
                            Sdt = tk.Sdt,
                            DiaChi = tk.DiaChi,
                            NgaySinh = tk.NgaySinh,
                            MatKhau = tk.MatKhau,
                            TenChucVu = cv.Ten,
                            Email = tk.Email,
                            MaCV = cv.MaCv
                        };

            var chucvu = obj.ChucVus.ToList();
            ViewBag.chucvu = chucvu;

            if ( matk != 0)
            {
                query = query.Where(item => item.MaTaiKhoan == matk);
            }

            if (!string.IsNullOrEmpty(tenctk))
            {
                query = query.Where(dm => dm.Ten.Contains(tenctk));
            }
            if (ngaysinh != null && ngaysinh != DateTime.MinValue)
            {
                query = query.Where(item => item.NgaySinh == ngaysinh);
            }

            if (!string.IsNullOrEmpty(tendc))
            {
                query = query.Where(dm => dm.DiaChi.Contains(tendc));
            }

            if (!string.IsNullOrEmpty(sdt))
            {
                query = query.Where(dm => dm.Sdt.Contains(sdt));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(dm => dm.Email.Contains(email));
            }

            if (chvu != 0)
            {
                query = query.Where(item => item.MaCV == chvu);
            }

            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Tính toán thông tin phân trang
            var totalItemCount = query.Count();
            var pagedList = new StaticPagedList<TaiKhoanChucVu>(model, page, pageSize, totalItemCount);

            ViewBag.PageStartItem = (page - 1) * pageSize + 1;
            ViewBag.PageEndItem = Math.Min(page * pageSize, totalItemCount);
            ViewBag.Page = page;
            ViewBag.TotalItemCount = totalItemCount;
            ViewBag.tenctk = tenctk;
            ViewBag.tendc = tendc;
            ViewBag.sdt = sdt;
            ViewBag.chvu = chvu;
            ViewBag.matk = matk;
            ViewBag.email=email;

            return View(pagedList);
        }

        public IActionResult SuaCV(int matk, int macv)
        {
            var tk = obj.Taikhoans.Find(matk);
            if(tk!=null)
            tk.MaCv = macv;
            obj.SaveChanges();
            return RedirectToAction("QuanLyTK","Admin");
        }
        public IActionResult XoaTK(int MaTK)
        {
            var tk = obj.Taikhoans.Find(MaTK);
            if (tk != null)
            {
                obj.Taikhoans.Remove(tk);
                obj.SaveChanges();
                return Json(new
                {
                    status = true
                });
            }
            else
            {
                // Xử lý trường hợp tk là null (nếu cần)
                return Json(new
                {
                    status = false,
                    message = "Không tìm thấy tài khoản"
                });
            }

        }

        #endregion
        #region Quản lý sản phẩm
        public IActionResult QuanLySP(string tenSP, int tenh, int tendm, int slc, int ban, decimal minPrice, decimal maxPrice, string sortOrder, int page = 1, int pageSize = 5)
        {
            var dsdm = obj.Danhmucsanphams.ToList();
            var dsh = obj.Hangsanxuats.ToList();
            ViewBag.dsdm = dsdm;
            ViewBag.dsh = dsh;
            if (!string.IsNullOrEmpty(tenSP))
            {
                var query = from sp in obj.Sanphams
                            join h in obj.Hangsanxuats on sp.MaHang equals h.MaHang
                            join dm in obj.Danhmucsanphams on sp.MaDanhMuc equals dm.MaDanhMuc
                            where sp.TenSp.Contains(tenSP)
                            select new SanPhamct()
                            {
                                MaSp = sp.MaSp,
                                TenSp = sp.TenSp,
                                MoTa = sp.MoTa,
                                Anh1 = sp.Anh1,
                                Anh2 = sp.Anh2,
                                Anh3 = sp.Anh3,
                                Anh4 = sp.Anh4,
                                Anh5 = sp.Anh5,
                                Anh6 = sp.Anh6,
                                SoLuongDaBan = sp.SoLuongDaBan,
                                SoLuongTrongKho = sp.SoLuongTrongKho,
                                GiaTien = sp.GiaTien,
                                Hang = h.TenHang,
                                DanhMuc = dm.TenDanhMuc,
                                MaH = h.MaHang,
                                MaDM = dm.MaDanhMuc,
                            };

                // Áp dụng bộ lọc theo giá
                if (maxPrice != 0)
                {
                    query = query.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice);
                }
                if (tenh != 0)
                {
                    query = query.Where(item => item.MaH == tenh);
                }
                if (tendm != 0)
                {
                    query = query.Where(item => item.MaDM == tendm);
                }
               
                if (slc != 0)
                {
                    query = query.Where(item => item.SoLuongTrongKho < slc);
                }
                if (ban != 0)
                {
                    query = query.Where(item => item.SoLuongDaBan < ban);
                }
                // Thực hiện phân trang
                var totalItemCount = query.Count();
                var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

               
                var pagedList = new StaticPagedList<SanPhamct>(model, page, pageSize, totalItemCount);

                // Truyền dữ liệu phân trang, kết quả tìm kiếm và các thông tin tùy chọn vào view
                ViewBag.PageStartItem = (page - 1) * pageSize + 1;
                ViewBag.PageEndItem = Math.Min(page * pageSize, totalItemCount);
                ViewBag.Page = page;
                ViewBag.SearchTerm = tenSP;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.SortOrder = sortOrder;
                ViewBag.TotalItemCount = totalItemCount;
                ViewBag.tenSP = tenSP;
                ViewBag.tenh = tenh;
                ViewBag.tendm = tendm;
                ViewBag.slc = slc;
                ViewBag.ban = ban;
                return View(pagedList);
            }
            else
            {
                var query = from sp in obj.Sanphams
                            join h in obj.Hangsanxuats on sp.MaHang equals h.MaHang
                            join dm in obj.Danhmucsanphams on sp.MaDanhMuc equals dm.MaDanhMuc
                            select new SanPhamct()
                            {
                                MaSp = sp.MaSp,
                                TenSp = sp.TenSp,
                                MoTa = sp.MoTa,
                                Anh1 = sp.Anh1,
                                Anh2 = sp.Anh2,
                                Anh3 = sp.Anh3,
                                Anh4 = sp.Anh4,
                                Anh5 = sp.Anh5,
                                Anh6 = sp.Anh6,
                                SoLuongDaBan = sp.SoLuongDaBan,
                                SoLuongTrongKho = sp.SoLuongTrongKho,
                                GiaTien = sp.GiaTien,
                                Hang = h.TenHang,
                                DanhMuc = dm.TenDanhMuc,
                                MaH = h.MaHang,
                                MaDM = dm.MaDanhMuc,
                            };
                // Áp dụng bộ lọc theo giá
                if (maxPrice != 0)
                {
                    query = query.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice);

                }
                if (tenh != 0)
                {
                    query = query.Where(item => item.MaH == tenh);
                }
                if (tendm != 0)
                {
                    query = query.Where(item => item.MaDM == tendm);
                }
                if (sortOrder == "tang")
                {
                    query = query.OrderBy(item => item.GiaTien);
                }
                if (sortOrder == "giam")
                {
                    query = query.OrderByDescending(item => item.GiaTien);
                }
                if (slc != 0)
                {
                    query = query.Where(item => item.SoLuongTrongKho < slc);
                }
                if (ban != 0)
                {
                    query = query.Where(item => item.SoLuongDaBan < ban);
                }
                // Thực hiện truy vấn và phân trang
                var totalItemCount = query.Count();
                var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
               
                // Tính toán thông tin phân trang

                var pagedList = new StaticPagedList<SanPhamct>(model, page, pageSize, totalItemCount);
                ViewBag.PageStartItem = (page - 1) * pageSize + 1;
                ViewBag.PageEndItem = Math.Min(page * pageSize, totalItemCount);
                ViewBag.Page = page;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.SortOrder = sortOrder;
                ViewBag.TotalItemCount = totalItemCount;
                ViewBag.tenh = tenh;
                ViewBag.tendm = tendm;
                ViewBag.slc = slc;
                ViewBag.ban = ban;
                return View(pagedList);
            }
        }
        [HttpPost]
        public ActionResult ThemSP(string TenSP, string MoTa, long GiaTien, int SoLuongTrongKho, IFormFile image1, IFormFile image2, IFormFile image3, IFormFile image4, IFormFile image5, IFormFile image6, string DanhMuc, string Hang)
        {
            var dsdm = obj.Danhmucsanphams.ToList();
            var dsh = obj.Hangsanxuats.ToList();
            ViewBag.dsdm = dsdm;
            ViewBag.dsh = dsh;
            var spmoi = new Models.Sanpham();
            spmoi.TenSp = TenSP;
            spmoi.MoTa = MoTa;
            spmoi.GiaTien = GiaTien;
            spmoi.SoLuongTrongKho = SoLuongTrongKho;
            spmoi.SoLuongDaBan = 0;

            if (image1 != null && image1.Length > 0)
            {
                string fileName = Path.GetFileName(image1.FileName);
                string uploadPath = Path.Combine("wwwroot", "Admin", "images", fileName);

                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    image1.CopyTo(fileStream);
                }

                spmoi.Anh1 = fileName;
            }

            if (image2 != null && image2.Length > 0)
            {
                string fileName = Path.GetFileName(image2.FileName);
                string uploadPath = Path.Combine("wwwroot", "Admin", "images", fileName);

                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    image2.CopyTo(fileStream);
                }

                spmoi.Anh2 = fileName;
            }

            if (image3 != null && image3.Length > 0)
            {
                string fileName = Path.GetFileName(image3.FileName);
                string uploadPath = Path.Combine("wwwroot", "Admin", "images", fileName);

                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    image3.CopyTo(fileStream);
                }

                spmoi.Anh3 = fileName;
            }
            if (image4 != null && image4.Length > 0)
            {
                string fileName = Path.GetFileName(image4.FileName);
                string uploadPath = Path.Combine("wwwroot", "Admin", "images", fileName);

                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    image4.CopyTo(fileStream);
                }

                spmoi.Anh4 = fileName;
            }
            if (image5 != null && image5.Length > 0)
            {
                string fileName = Path.GetFileName(image5.FileName);
                string uploadPath = Path.Combine("wwwroot", "Admin", "images", fileName);

                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    image5.CopyTo(fileStream);
                }

                spmoi.Anh5 = fileName;
            }
            if (image6 != null && image6.Length > 0)
            {
                string fileName = Path.GetFileName(image6.FileName);
                string uploadPath = Path.Combine("wwwroot", "Admin", "images", fileName);

                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    image6.CopyTo(fileStream);
                }

                spmoi.Anh6 = fileName;
            }
            // Lặp lại cho image3, image4, image5, và image6

            var dm = obj.Danhmucsanphams.FirstOrDefault(s => s.TenDanhMuc == DanhMuc);
            if (dm != null)
            {
                spmoi.MaDanhMuc = dm.MaDanhMuc;
            }

            var hang = obj.Hangsanxuats.FirstOrDefault(s => s.TenHang == Hang);
            if (hang != null)
            {
                spmoi.MaHang = hang.MaHang;
            }

            obj.Sanphams.Add(spmoi);
            obj.SaveChanges();

            return View();
        }


        public IActionResult XoaSP(int maSP)
        {
            var tk = obj.Sanphams.Find(maSP);
            if (tk != null)
            {
                obj.Sanphams.Remove(tk);
                obj.SaveChanges();
                return Json(new
                {
                    status = true
                });
            }
            else
            {
                // Xử lý trường hợp tk là null (nếu cần)
                return Json(new
                {
                    status = false,
                    message = "Không tìm thấy sản phẩm"
                });
            }
        }
        public IActionResult SuaSP(int ma)
        {
            var sp = obj.Sanphams.Find(ma);
            var dshang = obj.Hangsanxuats.ToList();
            var dsdm = obj.Danhmucsanphams.ToList();
            ViewBag.sp = sp;
            if (sp != null)
            {
                var dm = obj.Danhmucsanphams.Find(sp.MaDanhMuc);
                var hang = obj.Hangsanxuats.Find(sp.MaHang);
                if (dm != null && hang != null)
                {
                    ViewBag.tendm = dm.TenDanhMuc;
                    ViewBag.tenhang = hang.TenHang;
                    ViewBag.dsdm = dsdm;
                    ViewBag.dshang = dshang;
                    return View();
                }
                else
                {
                    // Xử lý trường hợp tk là null (nếu cần)
                    return Json(new
                    {
                        status = false,
                        message = "Không tìm thấy sản phẩm"
                    });
                }

            }
            else
            {
                // Xử lý trường hợp tk là null (nếu cần)
                return Json(new
                {
                    status = false,
                    message = "Không tìm thấy sản phẩm"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SuaSP(Models.Sanpham sp, IFormFile image1, IFormFile image2, IFormFile image3, IFormFile image4, IFormFile image5, IFormFile image6, string DanhMuc, string Hang)
        {
            var spmoi = obj.Sanphams.Find(sp.MaSp);
            if (spmoi != null)
            {
                spmoi.TenSp = sp.TenSp;
                spmoi.MoTa = sp.MoTa;
                spmoi.GiaTien = sp.GiaTien;
                spmoi.SoLuongTrongKho = sp.SoLuongTrongKho;
                spmoi.SoLuongDaBan = sp.SoLuongDaBan;

                // Đường dẫn đến thư mục lưu trữ tệp ảnh
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                string uploadPath = Path.Combine(currentDirectory, "wwwroot", "Admin", "images");


                // Hàm để lưu tệp ảnh
                async Task SaveImage(IFormFile image, string imageName, string propertyName)
                {
                    if (image != null)
                    {
                        string imagePath = Path.Combine(uploadPath, imageName);
                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }
                        // Gán tên tệp ảnh vào thuộc tính tương ứng
                        typeof(Models.Sanpham).GetProperty(propertyName)?.SetValue(spmoi, imageName);
                    }
                    else
                    {
                        // Giữ nguyên ảnh cũ nếu không có ảnh mới
                        typeof(Models.Sanpham).GetProperty(propertyName)?.SetValue(spmoi, typeof(Models.Sanpham).GetProperty(propertyName)?.GetValue(sp));
                    }
                }

                // Lưu từng ảnh

                if (spmoi.Anh1 != null)
                {
                    await SaveImage(image1, spmoi.Anh1, nameof(spmoi.Anh1));
                }

                if (spmoi.Anh2 != null)
                {
                    await SaveImage(image2, spmoi.Anh2, nameof(spmoi.Anh2));
                }
                if (spmoi.Anh3 != null)
                {
                    await SaveImage(image3, spmoi.Anh3, nameof(spmoi.Anh3));
                }

                if (spmoi.Anh4 != null)
                {
                    await SaveImage(image4, spmoi.Anh4, nameof(spmoi.Anh4));
                }
                if (spmoi.Anh5 != null)
                {
                    await SaveImage(image5, spmoi.Anh5, nameof(spmoi.Anh5));
                }

                if (spmoi.Anh6 != null)
                {
                    await SaveImage(image6, spmoi.Anh6, nameof(spmoi.Anh6));
                }
                // Tìm danh mục và hãng dựa trên tên
                var dm = obj.Danhmucsanphams.FirstOrDefault(s => s.TenDanhMuc == DanhMuc);
                if (dm != null)
                {
                    spmoi.MaDanhMuc = dm.MaDanhMuc;
                }

                var hang = obj.Hangsanxuats.FirstOrDefault(s => s.TenHang == Hang);
                if (hang != null)
                {
                    spmoi.MaHang = hang.MaHang;
                }

                obj.SaveChanges();
                return RedirectToAction("QuanLySP");
            }
            else
            {
                return Json(new
                {
                    status = false
                });
            }
        }
        /*   public IActionResult timKiemSanPham(string tenSP, int page = 1, int pageSize = 10)
           {
               if (!string.IsNullOrEmpty(tenSP))
               {
                   var query = from sp in obj.Sanphams where sp.TenSp.Contains(tenSP) select sp;

                   // Thực hiện phân trang
                   var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                   // Tính toán thông tin phân trang
                   var totalItemCount = query.Count();

                   if (model.Count == 0)
                   {
                       // Nếu không tìm thấy kết quả, điều hướng đến trang thông báo
                       return RedirectToAction("ThongBaoRong", "Admin");
                   }

                   var pagedList = new StaticPagedList<Sanpham>(model, page, pageSize, totalItemCount);

                   // Truyền dữ liệu phân trang và kết quả tìm kiếm vào view
                   ViewBag.PageStartItem = (page - 1) * pageSize + 1;
                   ViewBag.PageEndItem = Math.Min(page * pageSize, totalItemCount);
                   ViewBag.Page = page;
                   ViewBag.SearchTerm = tenSP;
                   ViewBag.TotalItemCount = totalItemCount;
                   return View(pagedList);
               }
               else
               {
                   return RedirectToAction("ThongBaoRong", "Admin");
               }
           }*/

        public IActionResult ThongBaoRong()
        {
            return View();
        }

        #endregion
        #region Quản lý hãng
        public IActionResult QuanLyHang(string tenhang, int mahang,int page = 1, int pageSize = 10)
        {

            var query = obj.Hangsanxuats.AsQueryable(); // Chuyển đổi sang IQueryable
            if (!string.IsNullOrEmpty(tenhang))
            {
                query = query.Where(dm => dm.TenHang.Contains(tenhang)); // hoặc OrderByDescending(dm => dm.MaDanhMuc)
            }

            if (mahang != 0)
            {
                query = query.Where(item => item.MaHang == mahang);
            }

            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Tính toán thông tin phân trang
            var totalItemCount = query.Count();
            var pagedList = new StaticPagedList<Hangsanxuat>(model, page, pageSize, totalItemCount);
            ViewBag.PageStartItem = (page - 1) * pageSize + 1;
            ViewBag.PageEndItem = Math.Min(page * pageSize, totalItemCount);
            ViewBag.Page = page;
            ViewBag.th = tenhang;
            ViewBag.mahang = mahang;

            return View(pagedList);
        }
        [HttpPost]
        public IActionResult ThemHang(string tenhang)
        {
            Models.Hangsanxuat hsx = new Models.Hangsanxuat();
            hsx.TenHang = tenhang;
            obj.Hangsanxuats.Add(hsx);
            obj.SaveChanges();
            return Json(new
            {
                status = true
            });
        }
        public IActionResult XoaHang(int matk)
        {
            var hsx = obj.Hangsanxuats.Find(matk);
            if (hsx != null)
            {
                obj.Hangsanxuats.Remove(hsx);
                obj.SaveChanges();
                return Json(new
                {
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
        public IActionResult SuaHang(int id)
        {
            var model = obj.Hangsanxuats.Find(id);
            return View(model);
        }
        [HttpPost]
        public IActionResult SuaHang(int id, string name)
        {
            var hsx = obj.Hangsanxuats.Find(id);
            if (hsx != null)
            {
                hsx.TenHang = name;
                obj.SaveChanges();
                return Json(new
                {
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
        #endregion
        #region Quản lý danh mục
        public IActionResult QuanLyDM( string tendm, int madm, int page = 1, int pageSize = 10)
        {
            var query = obj.Danhmucsanphams.AsQueryable(); // Chuyển đổi sang IQueryable
            if (!string.IsNullOrEmpty(tendm))
            {
                query = query.Where(dm => dm.TenDanhMuc.Contains(tendm)); // hoặc OrderByDescending(dm => dm.MaDanhMuc)
            }

            if (madm != 0)
            {
                query = query.Where(item => item.MaDanhMuc == madm);
            }

            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Tính toán thông tin phân trang
            var totalItemCount = query.Count();
            var pagedList = new StaticPagedList<Danhmucsanpham>(model, page, pageSize, totalItemCount);
            ViewBag.PageStartItem = (page - 1) * pageSize + 1;
            ViewBag.PageEndItem = Math.Min(page * pageSize, totalItemCount);
            ViewBag.Page = page;
            ViewBag.tdm = tendm;
            ViewBag.madm = madm;

            return View(pagedList);
        }
        public IActionResult XoaDM(int madm)
        {
            var hsx = obj.Danhmucsanphams.Find(madm);
            if (hsx != null)
            {
                obj.Danhmucsanphams.Remove(hsx);
                obj.SaveChanges();
                return Json(new
                {
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
        public IActionResult ThemDM(string tendm)
        {
            Models.Danhmucsanpham dm = new Danhmucsanpham();
            dm.TenDanhMuc = tendm;
            obj.Danhmucsanphams.Add(dm);
            obj.SaveChanges();
            return Json(new
            {
                status = true
            });
        }
        public IActionResult SuaDM(int id)
        {
            var dm = obj.Danhmucsanphams.Find(id);
            return PartialView(dm);
        }
        [HttpPost]
        public IActionResult SuaDM(int id, string name)
        {
            var dm = obj.Danhmucsanphams.Find(id);
            if (dm != null)
            {
                dm.TenDanhMuc = name;
                obj.SaveChanges();
                return Json(new
                {
                    status = true
                });
            }
            else
            {
                return Json(new { status = false });
            }
        }
        #endregion
        #region Quản lý đơn hàng
        public IActionResult QuanLyDH(string tennn, int madh, string dc,DateTime ngaymua, decimal minPrice, decimal maxPrice, int page = 1, int pageSize = 5)
        {
            // Lấy tất cả đơn hàng và thực hiện kết hợp
            var allOrders = from a in obj.Donhangs
                            join b in obj.Vanchuyens on a.MaDonHang equals b.MaDonHang
                            select new MyOrder()
                            {
                                MaDonHang = a.MaDonHang,
                                TongTien = a.TongTien,
                                NguoiNhan = b.NguoiNhan,
                                DiaChi = b.DiaChi,
                                NgayMua = a.NgayLap,
                                TinhTrang = a.TinhTrang
                            };
            if (madh != 0)
            {
                allOrders = allOrders.Where(item => item.MaDonHang == madh);
            }

            if (!string.IsNullOrEmpty(tennn))
            {
                allOrders = allOrders.Where(dm => dm.NguoiNhan.Contains(tennn));
            }
            if (!string.IsNullOrEmpty(dc))
            {
                allOrders = allOrders.Where(dm => dm.DiaChi.Contains(dc));
            }
            if (ngaymua != null && ngaymua != DateTime.MinValue)
            {
                allOrders = allOrders.Where(item => item.NgayMua == ngaymua);
            }
            if (maxPrice != 0)
            {
                allOrders = allOrders.Where(item => item.TongTien < maxPrice && item.TongTien > minPrice);
            }
           
            var tinhTrang = allOrders.OrderByDescending(o => o.MaDonHang).ToList();
            var tinhTrang0 = allOrders.Where(o => o.TinhTrang == 0).OrderByDescending(o => o.TongTien).ToList();
            var tinhTrang1 = allOrders.Where(o => o.TinhTrang == 1).OrderByDescending(o => o.TongTien).ToList();
            var tinhTrang2 = allOrders.Where(o => o.TinhTrang == 2).OrderByDescending(o => o.TongTien).ToList();
            var tinhTrang3 = allOrders.Where(o => o.TinhTrang == 3).OrderByDescending(o => o.TongTien).ToList();
            var tinhTrang4 = allOrders.Where(o => o.TinhTrang == 4).OrderByDescending(o => o.TongTien).ToList();

            // Tạo danh sách phân trang cho từng tình trạng
            var pagedList = tinhTrang.ToPagedList(page, pageSize);
            var pagedList0 = tinhTrang0.ToPagedList(page, pageSize);
            var pagedList1 = tinhTrang1.ToPagedList(page, pageSize);
            var pagedList2 = tinhTrang2.ToPagedList(page, pageSize);
            var pagedList3 = tinhTrang3.ToPagedList(page, pageSize);
            var pagedList4 = tinhTrang4.ToPagedList(page, pageSize);

            ViewBag.pagedList = pagedList;
            ViewBag.pagedList0 = pagedList0;
            ViewBag.pagedList1 = pagedList1;
            ViewBag.pagedList2 = pagedList2;
            ViewBag.pagedList3 = pagedList3;
            ViewBag.pagedList4 = pagedList4;
            ViewBag.PageStartItem = (page - 1) * pageSize + 1;
            ViewBag.PageEndItem = Math.Min(page * pageSize, pagedList.TotalItemCount);
            ViewBag.Page = page;

            int totalOrders = obj.Donhangs.Count();
            int unpaidOrdersCount = obj.Donhangs.Count(o => o.TinhTrang == 0);
            int pendingOrdersCount = obj.Donhangs.Count(o => o.TinhTrang == 1);
            int shippingOrdersCount = obj.Donhangs.Count(o => o.TinhTrang == 2);
            int completedOrdersCount = obj.Donhangs.Count(o => o.TinhTrang == 3);
            int canceledOrdersCount = obj.Donhangs.Count(o => o.TinhTrang == 4);

            ViewBag.TotalOrdersCount = totalOrders;
            ViewBag.UnpaidOrdersCount = unpaidOrdersCount;
            ViewBag.PendingOrdersCount = pendingOrdersCount;
            ViewBag.ShippingOrdersCount = shippingOrdersCount;
            ViewBag.CompletedOrdersCount = completedOrdersCount;
            ViewBag.CanceledOrdersCount = canceledOrdersCount;

            ViewBag.madh = madh;
            ViewBag.tennn = tennn;
            ViewBag.dc = dc;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.ngaymua= ngaymua;
            return View();
        }


        public IActionResult XacNhanDH(int madh)
        {
            var dh = obj.Donhangs.Find(madh);
            if (dh != null)
            {
                dh.TinhTrang = 2;
                obj.SaveChanges();
                return Json(new
                {
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
        public IActionResult VanChuyenDH(int madh)
        {
            var dh = obj.Donhangs.Find(madh);
            if (dh != null)
            {
                dh.TinhTrang = 3;
                obj.SaveChanges();
                return Json(new
                {
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
        public IActionResult HuyDH(int madh)
        {
            var dh = obj.Donhangs.Find(madh);
            if (dh != null)
            {
                dh.TinhTrang = 4;
                obj.SaveChanges();
                return Json(new
                {
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
        public IActionResult MyOrderDetail(int id)
        {
            var kq = from a in obj.Chitietdonhangs
                     join b in obj.Sanphams on a.MaSp equals b.MaSp
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
        #endregion
        #endregion
        #region Thống kê
        #region Thống kê doanh số bán ra
       
        [HttpPost]
        public IActionResult Index(int year)
        {
            var ds = obj.Donhangs.Where(s => s.NgayLap != null && s.NgayLap.Value.Year.ToString().Equals(year.ToString())).ToList();
            var list = new List<ThongKeDoanhThu>();
            long? sum1 = 0;
            long? sum2 = 0;
            long? sum3 = 0;
            long? sum4 = 0;
            long? sum5 = 0;
            long? sum6 = 0;
            long? sum7 = 0;
            long? sum8 = 0;
            long? sum9 = 0;
            long? sum10 = 0;
            long? sum11 = 0;
            long? sum12 = 0;
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "1")
                {
                    sum1 += item.TongTien;
                }
            }
            ThongKeDoanhThu tk1 = new ThongKeDoanhThu();
            tk1.Thang = 1;
            tk1.DoanhThu = sum1;
            list.Add(tk1);
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "2")
                {
                    sum2 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk2 = new ThongKeDoanhThu();
            tk2.Thang = 2;
            tk2.DoanhThu = sum2;
            list.Add(tk2);
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "3")
                {
                    sum3 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk3 = new ThongKeDoanhThu();
            tk3.Thang = 3;
            tk3.DoanhThu = sum3;
            list.Add(tk3);
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "4")
                {
                    sum4 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk4 = new ThongKeDoanhThu();
            tk4.Thang = 4;
            tk4.DoanhThu = sum4;
            list.Add(tk4);
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "5")
                {
                    sum5 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk5 = new ThongKeDoanhThu();
            tk5.Thang = 5;
            tk5.DoanhThu = sum5;
            list.Add(tk5);
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "6")
                {
                    sum6 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk6 = new ThongKeDoanhThu();
            tk6.Thang = 6;
            tk6.DoanhThu = sum6;
            list.Add(tk6);
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "7")
                {
                    sum7 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk7 = new ThongKeDoanhThu();
            tk7.Thang = 7;
            tk7.DoanhThu = sum7;
            list.Add(tk7);
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "8")
                {
                    sum8 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk8 = new ThongKeDoanhThu();
            tk8.Thang = 8;
            tk8.DoanhThu = sum8;
            list.Add(tk8);
            foreach (var item in ds)
            {
                ;
                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "9")
                {
                    sum9 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk9 = new ThongKeDoanhThu();
            tk9.Thang = 9;
            tk9.DoanhThu = sum9;
            list.Add(tk9);
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "10")
                {
                    sum10 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk10 = new ThongKeDoanhThu();
            tk10.Thang = 10;
            tk10.DoanhThu = sum10;
            list.Add(tk10);
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "11")
                {
                    sum11 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk11 = new ThongKeDoanhThu();
            tk11.Thang = 11;
            tk11.DoanhThu = sum11;
            list.Add(tk11);
            foreach (var item in ds)
            {

                if (item.NgayLap.HasValue && item.NgayLap.Value.Month.ToString() == "12")
                {
                    sum12 += item.TongTien;
                }

            }
            ThongKeDoanhThu tk12 = new ThongKeDoanhThu();
            tk12.Thang = 12;
            tk12.DoanhThu = sum12;
            list.Add(tk12);

            return Json(new
            {

                status = true,
                data = list
            });
        }
        #endregion
        #region Thống kê sản phẩm
        #endregion
        #endregion
    }
}
