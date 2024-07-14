using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Mioto.Models;

namespace Mioto.Controllers
{
    public class CarController : Controller
    {
        private DB_MiotoEntities db = new DB_MiotoEntities();

        public bool IsLoggedIn { get => Session["KhachHang"] != null || Session["ChuXe"] != null; }

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

        // GET: Car/RegisterOwner
        public ActionResult RegisterOwner()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            ViewBag.TinhThanhPho = tinhThanhPho;
            return View();
        }

        // POST: Car/RegisterOwner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterOwner(MD_ChuXe cx)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            ViewBag.TinhThanhPho = tinhThanhPho;
            try
            {
                if (ModelState.IsValid)
                {
                    var guest = Session["KhachHang"] as KhachHang;
                    if (db.Xe.Any(x => x.BienSoXe == cx.BienSoXe))
                    {
                        ModelState.AddModelError("BienSoXe", "Biển số xe đã đăng ký trên hệ thống");
                        return View(cx);
                    }
                    // Kiểm tra nếu không tồn tại IDCX trùng với IDKH thì mới tạo mới ChuXe
                    if (!db.ChuXe.Any(x => x.IDCX == guest.IDKH))
                    {
                        var newCX = new ChuXe
                        {
                            IDCX = guest.IDKH,
                            Ten = guest.Ten,
                            Email = guest.Email,
                            SDT = guest.SDT,
                            DiaChi = guest.DiaChi,
                            MatKhau = guest.MatKhau,
                            GioiTinh = guest.GioiTinh,
                            NgaySinh = guest.NgaySinh,
                            TrangThai = "Hoạt động"
                        };
                        db.ChuXe.Add(newCX);
                    }
                    var newCar = new Xe
                    {
                        IDCX = guest.IDKH,
                        BienSoXe = cx.BienSoXe,
                        HangXe = cx.HangXe,
                        MauXe = cx.MauXe,
                        SoGhe = cx.SoGhe,
                        TinhNang = cx.TinhNang,
                        GiaThue = cx.GiaThue,
                        NamSanXuat = cx.NamSanXuat,
                        KhuVuc = cx.KhuVuc,
                        TrangThai = "Sẵn sàng"
                    };
                    db.Xe.Add(newCar);
                    db.SaveChanges();
                    var IsGuest = db.KhachHang.SingleOrDefault(s => s.Email == guest.Email && s.MatKhau == guest.MatKhau);
                    var IsChuXe = db.ChuXe.SingleOrDefault(s => s.Email == guest.Email && s.MatKhau == guest.MatKhau);
                    Session["KhachHang"] = IsGuest;
                    Session["ChuXe"] = IsChuXe;
                    TempData["Message"] = "Đăng ký thành công!";
                    return RedirectToAction("Home", "Home");
                }
                return View(cx);
            }
            catch
            {
                ViewBag.ErrorRegister = "Đăng ký không thành công. Vui lòng thử lại.";
                ViewBag.TinhThanhPho = tinhThanhPho;
                return View(cx);
            }
        }

        public ActionResult InfoCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
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

        public ActionResult BookingCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var xe = db.Xe.FirstOrDefault(x => x.BienSoXe == BienSoXe);
            var cx = db.ChuXe.FirstOrDefault(x => x.IDCX == xe.IDCX);

            if (xe == null || cx == null)
                return HttpNotFound();

            var bookingCarModel = new MD_BookingCar
            {
                Xe = xe,
                ChuXe = cx,
            };
            return View(bookingCarModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookingCar(MD_BookingCar bookingCar)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var donThueXe = new DonThueXe
                {
                    IDKH = bookingCar.ChuXe.IDCX,
                    BienSoXe = bookingCar.Xe.BienSoXe,
                    NgayThue = bookingCar.NgayThue,
                    NgayTra = bookingCar.NgayTra,
                    BDT = bookingCar.BDT,
                    TrangThai = "Chờ thanh toán"
                };

                db.DonThueXe.Add(donThueXe);
                db.SaveChanges();

                var thanhToan = new ThanhToan
                {
                    IDDT = donThueXe.IDDT,
                    NgayTT = DateTime.Now,
                    SoTien = bookingCar.Xe.GiaThue,
                    TrangThai = "Chưa thanh toán",
                    PhuongThuc = "Online"
                };

                db.ThanhToan.Add(thanhToan);
                db.SaveChanges();

                TempData["Message"] = "Đặt xe thành công! Vui lòng thanh toán để hoàn tất giao dịch.";
                return RedirectToAction("Payment", new { idtt = thanhToan.IDTT });
            }
            return View(bookingCar);
        }

        public ActionResult Payment(int idtt)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var thanhToan = db.ThanhToan.FirstOrDefault(t => t.IDTT == idtt);
            var dtx = db.DonThueXe.FirstOrDefault(t => t.IDDT == idtt);
            var xe = db.Xe.FirstOrDefault(t => t.IDCX == dtx.IDKH);

            if (thanhToan == null || dtx == null || xe == null)
            {
                return HttpNotFound();
            }

            Session["Xe"] = xe;
            return View(thanhToan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(ThanhToan thanhToan)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var existingThanhToan = db.ThanhToan.FirstOrDefault(t => t.IDTT == thanhToan.IDTT);
                if (existingThanhToan != null)
                {
                    existingThanhToan.TrangThai = "Đã thanh toán";
                    db.SaveChanges();
                }

                TempData["Message"] = "Thanh toán thành công!";
                return RedirectToAction("Home", "Home");
            }
            return View(thanhToan);
        }
    }
}
