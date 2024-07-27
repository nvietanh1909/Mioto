using Google;
using Mioto.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace Mioto.Controllers
{
    public class DetailAccountController : Controller
    {
        DB_MiotoEntities db = new DB_MiotoEntities();
        public bool IsLoggedIn { get => Session["KhachHang"] != null || Session["ChuXe"] != null; }
        private static readonly HttpClient client = new HttpClient();

        List<SelectListItem> tinhThanhPho = new List<SelectListItem>
        {
            new SelectListItem { Text = "TP Hồ Chí Minh", Value = "TP Hồ Chí Minh" },
            new SelectListItem { Text = "Hà Nội", Value = "Hà Nội" },
            new SelectListItem { Text = "Đà Nẵng", Value = "Đà Nẵng" },
            new SelectListItem { Text = "Bình Dương", Value = "Bình Dương" },
            new SelectListItem { Text = "Cần Thơ", Value = "Cần Thơ" },
            new SelectListItem { Text = "Đà Lạt", Value = "Đà Lạt" },
            new SelectListItem { Text = "Nha Trang", Value = "Nha Trang" },
            new SelectListItem { Text = "Quy Nhơn", Value = "Quy Nhơn" },
            new SelectListItem { Text = "Phú Quốc", Value = "Phú Quốc" },
            new SelectListItem { Text = "Hải Phòng", Value = "Hải Phòng" },
            new SelectListItem { Text = "Vũng Tàu", Value = "Vũng Tàu" },
            new SelectListItem { Text = "Thành phố khác", Value = "Thành phố khác" },
        };

        List<SelectListItem> gioitinh = new List<SelectListItem>
        {
         new SelectListItem { Text = "Nam", Value = "Nam" },
         new SelectListItem { Text = "Nữ", Value = "Nữ" }
        };


        List<SelectListItem> TrangThaiXe = new List<SelectListItem>
        {
         new SelectListItem { Text = "Sẵn sàng", Value = "Sẵn sàng" },
         new SelectListItem { Text = "Bảo trì", Value = "Bảo trì" },
         new SelectListItem { Text = "Ngưng cho thuê", Value = "Ngưng cho thuê" }
        };

        // GET: DetailAccount
        public ActionResult InfoAccount()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var guest = Session["KhachHang"] as KhachHang;
            if (guest == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Khách hàng không tồn tại");
            var kh = db.KhachHang.Where(x => x.IDKH == guest.IDKH);
            return View(kh);
        }

        // GET: EditCCCD/InfoAccount
        public ActionResult EditCCCD(int IDKH)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var id = db.CCCD.FirstOrDefault(x => x.IDKH == IDKH);
            return View(id);
        }
        // POST: EditGPLX/InfoAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCCCD(CCCD cccd)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Home");
            var id = db.CCCD.FirstOrDefault(x => x.IDKH == cccd.IDKH);
            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy ID khách hàng và chủ xe
                    var guest = db.KhachHang.FirstOrDefault(x => x.IDKH == cccd.IDKH);
                    var chuxe = db.ChuXe.FirstOrDefault(x => x.IDCX == guest.IDKH);

                    id.IDKH = cccd.IDKH;
                    id.SoCCCD = cccd.SoCCCD;
                    id.Ten = cccd.Ten;
                    id.NgaySinh = cccd.NgaySinh;
                    id.TrangThai = "No";
                    db.Entry(id).State = EntityState.Modified;
                    db.SaveChanges();

                    if (guest != null)
                    {
                        guest.CCCD = cccd.SoCCCD;
                        db.Entry(guest).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    if (chuxe != null)
                    {
                        chuxe.CCCD = cccd.SoCCCD;
                        db.Entry(chuxe).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    // Cập nhật lại Session
                    Session["KhachHang"] = guest;
                    Session["ChuXe"] = chuxe;
                    return RedirectToAction("InfoAccount");
                }
                return View(id);
            }
            catch
            {
                return View(id);
            }
        }
        // GET: EditGPLX/InfoAccount
        public ActionResult EditGPLX(int IDKH)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var id = db.GPLX.FirstOrDefault(x => x.IDKH == IDKH);
            return View(id);
        }

        // POST: EditGPLX/InfoAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditGPLX(GPLX gplx)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Home");
            var id = db.GPLX.FirstOrDefault(x => x.IDKH == gplx.IDKH);
            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy ID khách hàng và chủ xe
                    var guest = db.KhachHang.FirstOrDefault(x => x.IDKH == gplx.IDKH);
                    var chuxe = db.ChuXe.FirstOrDefault(x => x.IDCX == guest.IDKH);

                    id.IDKH = gplx.IDKH;
                    id.SoGPLX = gplx.SoGPLX;
                    id.Ten = gplx.Ten;
                    id.NgaySinh = gplx.NgaySinh;
                    id.TrangThai = "No";
                    db.Entry(id).State = EntityState.Modified;
                    db.SaveChanges();

                    if (guest != null)
                    {
                        guest.SoGPLX = gplx.SoGPLX;
                        db.Entry(guest).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    if (chuxe != null)
                    {
                        chuxe.SoGPLX = gplx.SoGPLX;
                        db.Entry(chuxe).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    // Cập nhật lại Session
                    Session["KhachHang"] = guest;
                    Session["ChuXe"] = chuxe;
                    return RedirectToAction("InfoAccount");
                }
                return View(id);
            }
            catch
            {
                return View(id);
            }
        }

        // GET: EditInfoUser/InfoAccount
        public ActionResult EditInfoUser(int IDKH)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var id = db.KhachHang.FirstOrDefault(x => x.IDKH == IDKH);
            ViewBag.GioiTinh = gioitinh;
            return View(id);
        }
        // POST: EditInfoUser/InfoAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInfoUser(KhachHang kh)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Home");
            try
            {
                if (ModelState.IsValid)
                {
                    var guest = Session["KhachHang"] as KhachHang;
                    var chuxe = Session["ChuXe"] as ChuXe;
                    kh.SoGPLX = guest.SoGPLX;
                    kh.MatKhau = guest.MatKhau;
                    kh.IDKH = kh.IDKH;
                    kh.CCCD = guest.CCCD;
                    kh.HinhAnh = guest.HinhAnh;
                    db.Entry(kh).State = EntityState.Modified;
                    db.SaveChanges();

                    var existingGPLX = db.GPLX.Find(kh.IDKH);
                    if (existingGPLX != null)
                    {
                        existingGPLX.Ten = kh.Ten;
                        existingGPLX.NgaySinh = kh.NgaySinh;
                        db.Entry(existingGPLX).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    if (chuxe != null)
                    {
                        var newChuXe = new ChuXe
                        {
                            IDCX = kh.IDKH,
                            Ten = kh.Ten,
                            Email = kh.Email,
                            SDT = kh.SDT,
                            DiaChi = kh.DiaChi,
                            MatKhau = kh.MatKhau,
                            GioiTinh = kh.GioiTinh,
                            NgaySinh = kh.NgaySinh,
                            SoGPLX = kh.SoGPLX,
                            CCCD = kh.CCCD,
                            HinhAnh = guest.HinhAnh,
                            TrangThai = "Hoạt động"
                        };
                        db.Entry(newChuXe).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    guest = db.KhachHang.FirstOrDefault(x => x.IDKH == guest.IDKH);
                    chuxe = db.ChuXe.FirstOrDefault(x => x.IDCX == guest.IDKH);

                    // Cập nhật lại Session
                    Session["KhachHang"] = guest;
                    Session["ChuXe"] = chuxe;
                    return RedirectToAction("InfoAccount");
                }
                return View(kh);
            }
            catch
            {
                return View(kh);
            }
        }
        public ActionResult FavoriteCar()
        {
            return View();
        }
        public ActionResult Gift()
        {
            return View();
        }

        // GET: Detailt/MyCar
        public ActionResult MyCar()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var guest = Session["KhachHang"] as KhachHang;
            var chuxe = Session["ChuXe"] as ChuXe;
            if (guest == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Khách hàng không tồn tại");
            var cars = db.Xe.Where(x => x.IDCX == guest.IDKH).ToList();
            if (chuxe != null)
            {
                cars = db.Xe.Where(x => x.IDCX == chuxe.IDCX).ToList();
                return View(cars);
            }
            return View(cars);
        }

        // GET: EditCar/MyCar
        public ActionResult EditCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            ViewBag.TinhThanhPho = tinhThanhPho;
            ViewBag.TrangThaiXe = TrangThaiXe;
            if (String.IsNullOrEmpty(BienSoXe))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var xe = db.Xe.FirstOrDefault(x => x.BienSoXe == BienSoXe);
            if (xe == null)
            {
                return HttpNotFound();
            }
            return View(xe);
        }
        // POST: EditCar/MyCar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCar(Xe xe)
        {
            if (!IsLoggedIn)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.TinhThanhPho = tinhThanhPho;
            try
            {
                if (ModelState.IsValid)
                {
                    var guest = Session["KhachHang"] as KhachHang;
                    xe.IDCX = guest.IDKH;
                    xe.KhuVuc = xe.KhuVuc;
                    db.Entry(xe).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("MyCar");
                }
                return View(xe);
            }
            catch
            {
                return View(xe);
            }
        }


        public ActionResult MyTrip()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var guest = Session["KhachHang"] as KhachHang;
            var chuxe = Session["ChuXe"] as ChuXe;

            if (guest == null && chuxe == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Khách hàng hoặc chủ xe không tồn tại");

            List<MD_MyTrip> myTrips = new List<MD_MyTrip>();

            if (guest != null)
            {
                myTrips = db.DonThueXe
                    .Where(x => x.IDKH == guest.IDKH)
                    .Select(dtx => new MD_MyTrip
                    {
                        IDDT = dtx.IDDT,
                        BienSoXe = dtx.BienSoXe,
                        NgayThue = dtx.NgayThue,
                        NgayTra = dtx.NgayTra,
                        BDT = dtx.BDT,
                        TongTien = dtx.TongTien,
                        TrangThai = dtx.TrangThai,
                        ThanhToan = dtx.ThanhToan.FirstOrDefault(x => x.IDDT == dtx.IDDT),
                        ChuXe = db.ChuXe.FirstOrDefault(cx => cx.IDCX == dtx.Xe.IDCX),
                    }).ToList();
            }

            if (chuxe != null)
            {
                myTrips = db.DonThueXe
                    .Where(x => x.IDKH == chuxe.IDKH)
                    .Select(dtx => new MD_MyTrip
                    {
                        IDDT = dtx.IDDT,
                        BienSoXe = dtx.BienSoXe,
                        NgayThue = dtx.NgayThue,
                        NgayTra = dtx.NgayTra,
                        BDT = dtx.BDT,
                        TrangThai = dtx.TrangThai,
                        TongTien = dtx.TongTien,
                        ThanhToan = dtx.ThanhToan.FirstOrDefault(x => x.IDDT == dtx.IDDT),
                        ChuXe = db.ChuXe.FirstOrDefault(cx => cx.IDCX == dtx.Xe.IDCX)
                    }).ToList();
            }
            return View(myTrips);
        }

        public ActionResult LongTermOrder()
        {
            return View();
        }
        public ActionResult RequestDeleteAccount()
        {
            return View();
        }
        public ActionResult MyAddress()
        {
            return View();
        }

        public ActionResult ChangePassword()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(MD_ChangePassword model)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid)
                return View(model);

            var guest = Session["KhachHang"] as KhachHang;
            var existingKH = db.KhachHang.Find(guest.IDKH);
            var existingCX = db.ChuXe.Find(guest.IDKH);

            if (guest == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra mật khẩu cũ
            if (VerifyPassword(guest, model.OldPassword))
            {
                if (existingCX == null)
                {
                    // Cập nhật mật khẩu mới cho Khách hàng
                    existingKH.MatKhau = model.NewPassword;
                    db.Entry(existingKH).State = EntityState.Modified;
                    db.SaveChanges();
                    Session["KhachHang"] = existingKH;
                }
                else
                {
                    // Cập nhật mật khẩu mới cho Khách hàng
                    existingKH.MatKhau = model.NewPassword;
                    db.Entry(existingKH).State = EntityState.Modified;
                    db.SaveChanges();
                    Session["KhachHang"] = existingKH;
                    // Cập nhật mật khẩu mới cho Chủ xe
                    existingCX.MatKhau = model.NewPassword;
                    db.Entry(existingCX).State = EntityState.Modified;
                    db.SaveChanges();
                    Session["ChuXe"] = existingCX;
                }
                return RedirectToAction("InfoAccount");
            }
            else
            {
                // Mật khẩu cũ không đúng
                ViewBag.ErrorMessage = "Mật khẩu hiện tại không đúng. Vui lòng thử lại.";
                return View(model);
            }
        }

        // Thay đổi avatar user
        [HttpPost]
        public ActionResult ChangeAvatarUser(HttpPostedFileBase avatar)
        {
            // Kiểm tra xem file có tồn tại không
            if (avatar != null && avatar.ContentLength > 0)
            {
                // Kiểm tra định dạng file (chỉ cho phép các định dạng ảnh phổ biến)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(avatar.FileName).ToLower();

                if (allowedExtensions.Contains(extension))
                {
                    // Tạo tên file duy nhất để tránh trùng lặp
                    var fileName = Guid.NewGuid().ToString() + extension;

                    // Đường dẫn lưu file
                    var path = Path.Combine(Server.MapPath("~/AvatarUser/"), fileName);

                    // Lưu file lên server
                    avatar.SaveAs(path);

                    // Cập nhật thông tin hình ảnh của khách hàng trong cơ sở dữ liệu
                    var guest = Session["KhachHang"] as KhachHang;
                    var existingKH = db.KhachHang.Find(guest.IDKH);
                    var existingCX = db.ChuXe.Find(guest.IDKH);

                    if (existingKH != null)
                    {
                        existingKH.HinhAnh = fileName;

                        // Kiểm tra chủ xe
                        if(existingCX != null)
                        {
                            existingCX.HinhAnh = fileName;
                            db.Entry(existingCX).State = EntityState.Modified;
                            Session["ChuXe"] = existingCX;
                        }
                        // Cập nhật cơ sở dữ liệu
                        Session["KhachHang"] = existingKH;
                        db.Entry(existingKH).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    return RedirectToAction("InfoAccount");
                }
                else
                {
                    ModelState.AddModelError("", "Định dạng file không hợp lệ. Vui lòng chọn một file ảnh.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Vui lòng chọn một file ảnh.");
            }

            // Trả về view với các lỗi nếu có
            return View("InfoAccount");
        }
        
        public ActionResult Logout()
        {
            // Xóa tất cả các session của người dùng
            Session.Clear(); 
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account"); 
        }
        private bool VerifyPassword(KhachHang user, string password)
        {
            return user.MatKhau == password;
        }

        public ActionResult DeleteTrip(int id)
        {
            // Kiểm tra người dùng đã đăng nhập
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            // Lấy thông tin chuyến thuê xe
            var donThueXe = db.DonThueXe.FirstOrDefault(x => x.IDDT == id);
            if (donThueXe == null)
                return HttpNotFound("Chuyến thuê không tồn tại");

            var currentDateTime = DateTime.Now;
            var bookingDateTime = donThueXe.BDT;
            var timeDifference = (currentDateTime - donThueXe.BDT).TotalDays;

            // Xác định người dùng hủy chuyến
            var guest = Session["KhachHang"] as KhachHang;

            decimal hoanTien = 0;
            if (guest != null)
            {
                // Khách hàng hủy chuyến
                if (currentDateTime <= bookingDateTime.AddHours(1))
                {
                    // <= 1 giờ sau giữ chỗ
                    hoanTien = donThueXe.TongTien; // Hoàn tiền 100%
                }
                else if (timeDifference > 7)
                {
                    // Hủy trước chuyến đi > 7 ngày
                    hoanTien = donThueXe.TongTien * 70/100; // Hoàn tiền 70%
                }
                else
                {
                    // Hủy <= 7 ngày trước chuyến đi
                    hoanTien = 0; // Không hoàn tiền
                }

                // Lưu thông tin phí hủy chuyến cho khách hàng
                var phiHuyChuyen = new PhiHuyChuyen
                {
                    IDDT = id,
                    LoaiHuyChuyen = 1, // Khách hàng
                    ThoiGianHuy = (currentDateTime <= bookingDateTime.AddHours(1)) ? 1 : (timeDifference > 7 ? 2 : 3),
                    HoanTien = hoanTien,
                    DenTien = 0,
                    MoTa = "Hủy chuyến do khách hàng yêu cầu."
                };

                db.PhiHuyChuyen.Add(phiHuyChuyen);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Người dùng không hợp lệ");
            }

            db.SaveChanges();

            // Xóa đơn thanh toán
            var thanhtoan = db.ThanhToan.FirstOrDefault(x => x.IDDT == donThueXe.IDDT);
            if (thanhtoan != null)
            {
                db.ThanhToan.Remove(thanhtoan);
                db.SaveChanges();
            }

            // Xóa đơn thuê xe
            donThueXe.TrangThai = 2;
            db.Entry(donThueXe).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("MyTrip");
        }
        // GET: RentedCar
        public ActionResult RentedCar()
        {
            // Kiểm tra người dùng đã đăng nhập
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var chuxe = Session["ChuXe"] as ChuXe;
            var khachhang = Session["KhachHang"] as KhachHang;

            if(chuxe != null)
            {
                // Lấy danh sách các xe đang được thuê của chủ xe
                var rentedCars = db.DonThueXe
                    .Where(d => d.Xe.IDCX == chuxe.IDCX)
                    .Select(d => new MD_RentedCar
                    {
                        IDDT = d.IDDT,
                        BienSoXe = d.BienSoXe,
                        HangXe = d.Xe.HangXe,
                        MauXe = d.Xe.MauXe,
                        NgayThue = d.NgayThue,
                        NgayTra = d.NgayTra,
                        TongTien = d.TongTien,
                        TrangThai = d.TrangThai,
                    }).ToList();

                return View(rentedCars);
            }
            return View();
        }

        public ActionResult DeleteRentedCar(int id)
        {
            // Kiểm tra người dùng đã đăng nhập
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            // Lấy thông tin chuyến thuê xe
            var donThueXe = db.DonThueXe.FirstOrDefault(x => x.IDDT == id);
            if (donThueXe == null)
                return HttpNotFound("Chuyến thuê không tồn tại");

            var currentDateTime = DateTime.Now;
            var bookingDateTime = donThueXe.BDT;
            var timeDifference = (currentDateTime - bookingDateTime).TotalDays;

            // Xác định người dùng hủy chuyến
            var guest = Session["KhachHang"] as KhachHang;
            var chuxe = Session["ChuXe"] as ChuXe;

            decimal denTien = 0;
            
            if (chuxe != null)
            {
                // Chủ xe hủy chuyến
                if (currentDateTime <= bookingDateTime.AddHours(1))
                {
                    // <= 1 giờ sau giữ chỗ
                    denTien = 0; // Không mất phí
                }
                else if (timeDifference > 7)
                {
                    // Hủy trước chuyến đi > 7 ngày
                    denTien = donThueXe.TongTien * 30/100; // Đền tiền 30%
                }
                else
                {
                    // Hủy <= 7 ngày trước chuyến đi
                    denTien = donThueXe.TongTien; // Đền tiền 100%
                }

                // Lưu thông tin phí hủy chuyến cho chủ xe
                var phiHuyChuyen = new PhiHuyChuyen
                {
                    IDDT = id,
                    LoaiHuyChuyen = 2, // Chủ xe
                    ThoiGianHuy = (currentDateTime <= bookingDateTime.AddHours(1)) ? 1 : (timeDifference > 7 ? 2 : 3),
                    HoanTien = 0,
                    DenTien = denTien,
                    MoTa = "Hủy chuyến do chủ xe yêu cầu."
                };

                db.PhiHuyChuyen.Add(phiHuyChuyen);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Người dùng không hợp lệ");
            }

            db.SaveChanges();

            // Xóa đơn thanh toán
            var thanhtoan = db.ThanhToan.FirstOrDefault(x => x.IDDT == donThueXe.IDDT);
            if (thanhtoan != null)
            {
                db.ThanhToan.Remove(thanhtoan);
                db.SaveChanges();
            }

            // Xóa đơn thuê xe
            donThueXe.TrangThai = 2; // Đánh dấu là đã hủy
            db.Entry(donThueXe).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("RentedCar");
        }

    }
}