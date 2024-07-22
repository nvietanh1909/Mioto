using Mioto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Mioto.Controllers
{
    public class PaymentController : Controller
    {
        private DB_MiotoEntities db = new DB_MiotoEntities();
        public bool IsLoggedIn { get => Session["KhachHang"] != null || Session["ChuXe"] != null; }

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
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            return View(xe);
        }

        public ActionResult BookingCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var KhachHang = Session["KhachHang"] as KhachHang;
            var xe = db.Xe.FirstOrDefault(x => x.BienSoXe == BienSoXe);
            var cx = db.ChuXe.FirstOrDefault(x => x.IDCX == xe.IDCX);
            var gplx = db.GPLX.FirstOrDefault(x => x.IDKH == KhachHang.IDKH);

            if (xe == null || cx == null)
                return HttpNotFound();

            var bookingCarModel = new MD_BookingCar
            {
                Xe = xe,
                ChuXe = cx,
            };

            if (KhachHang != null)
            {
                if (gplx.TrangThai == "Yes")
                {
                    return View(bookingCarModel);
                }
                else
                {
                    TempData["ErrorMessage"] = "Giấy phép lái xe chưa được xác thực! Vui lòng xác thực giấy phép lái xe trước khi thuê xe.";
                    return RedirectToAction("InfoCar", new { BienSoXe = xe.BienSoXe });
                }
            }
            return View();
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
                    TrangThai = 1
                };

                db.DonThueXe.Add(donThueXe);
                db.SaveChanges();

                var thanhToan = new ThanhToan
                {
                    IDDT = donThueXe.IDDT,
                    NgayTT = DateTime.Now,
                    SoTien = bookingCar.Xe.GiaThue,
                    TrangThai = "No",
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
                    existingThanhToan.TrangThai = "Yes";
                    db.SaveChanges();
                }
                TempData["Message"] = "Thanh toán thành công!";
                return RedirectToAction("Home", "Home");
            }
            return View(thanhToan);
        }
    }
}