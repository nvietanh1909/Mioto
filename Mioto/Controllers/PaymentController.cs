using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Mioto.Models;
using System.Net;
using System.Web.Mvc;
using System.Linq;
using System.Data.Entity;

namespace Mioto.Controllers
{
    public class PaymentController : Controller
    {
        private readonly DB_MiotoEntities db = new DB_MiotoEntities();
        public bool IsLoggedIn => Session["KhachHang"] != null || Session["ChuXe"] != null;

        public ActionResult InfoCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(BienSoXe))
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

        public ActionResult Alert()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            return View();
        }

        public ActionResult BookingCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var khachHang = Session["KhachHang"] as KhachHang;
            var xe = db.Xe.FirstOrDefault(x => x.BienSoXe == BienSoXe);
            var chuXe = db.ChuXe.FirstOrDefault(x => x.IDCX == xe.IDCX);

            if (xe == null || chuXe == null)
                return HttpNotFound();
            if (khachHang != null)
            {
                if (khachHang.CCCD == "No" || khachHang.SoGPLX == "No")
                {
                    return RedirectToAction("Alert", "Payment");
                }
            }
            else if (chuXe != null)
            {
                if (chuXe.CCCD == "No" || chuXe.SoGPLX == "No")
                {
                    return RedirectToAction("Alert", "Payment");
                }
            }
            var bookingCarModel = new MD_BookingCar
            {
                Xe = xe,
                ChuXe = chuXe,
            };
            return View(bookingCarModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookingCar(MD_BookingCar bookingCar)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var khachHang = Session["KhachHang"] as KhachHang;
            if (ModelState.IsValid)
            {
                // Kiểm tra xe
                var isAvailableXe = !db.DonThueXe.Any(d => d.BienSoXe == bookingCar.Xe.BienSoXe &&
                                              ((d.NgayThue < bookingCar.NgayTra && d.NgayTra > bookingCar.NgayThue) ||
                                               (d.NgayThue < bookingCar.NgayThue && d.NgayTra > bookingCar.NgayThue) ||
                                               (d.NgayThue < bookingCar.NgayTra && d.NgayTra > bookingCar.NgayTra)));

                if (!isAvailableXe)
                {
                    ModelState.AddModelError("", "Xe không còn khả dụng trong khoảng thời gian này.");
                    return View(bookingCar);
                }

                // Tạo đơn thuê xe
                var donThueXe = new DonThueXe
                {
                    IDKH = khachHang.IDKH,
                    BienSoXe = bookingCar.Xe.BienSoXe,
                    NgayThue = bookingCar.NgayThue,
                    NgayTra = bookingCar.NgayTra,
                    BDT = bookingCar.BDT,
                    TrangThai = 1, 
                    PhanTramHoaHongCTyNhan = 10,
                    TongTien = bookingCar.Xe.GiaThue * (bookingCar.NgayTra - bookingCar.NgayThue).Days
                };

                // Thêm đơn thuê xe vào cơ sở dữ liệu
                db.DonThueXe.Add(donThueXe);
                db.SaveChanges();

                // Có thể chuyển hướng hoặc thông báo thành công
                return RedirectToAction("Payment", new { iddt = donThueXe.IDDT });
            }

            // Nếu model không hợp lệ, trở lại view với thông báo lỗi
            return View(bookingCar);
        }


        public ActionResult Payment(int iddt)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var khachHang = Session["KhachHang"] as KhachHang;
            if (khachHang == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var donThueXe = db.DonThueXe.FirstOrDefault(t => t.IDDT == iddt);
            if (donThueXe == null)
            {
                return HttpNotFound();
            }

            var xe = db.Xe.FirstOrDefault(t => t.BienSoXe == donThueXe.BienSoXe);
            if (xe == null)
            {
                return HttpNotFound();
            }

            var thanhToan = new MD_Payment
            {
                IDDT = donThueXe.IDDT,
                PhuongThuc = "Chưa xác định", // Có thể để người dùng chọn phương thức thanh toán
                NgayTT = DateTime.Now,
                SoTien = donThueXe.TongTien,
                TrangThai = "Chưa thanh toán",
            };

            // Lưu trữ xe trong Session nếu cần thiết
            Session["Xe"] = xe;

            // Truyền thông tin thanh toán tới View để hiển thị form thanh toán
            return View(thanhToan);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(MD_Payment thanhToan)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                // Tìm mã giảm giá (nếu có)
                MaGiamGia maGiamGia = null;
                if (!string.IsNullOrEmpty(thanhToan.MaGiamGia))
                {
                    maGiamGia = db.MaGiamGia.FirstOrDefault(m => m.Ma == thanhToan.MaGiamGia);
                }

                // Tìm đơn thuê xe
                var donThueXe = db.DonThueXe.FirstOrDefault(t => t.IDDT == thanhToan.IDDT);
                if (donThueXe == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy đơn thuê xe.");
                    return View(thanhToan);
                }

                // Tính toán số tiền thanh toán
                var soTien = donThueXe.TongTien;
                if (maGiamGia != null)
                {
                    // Áp dụng giảm giá
                    soTien = thanhToan.SoTien;
                    if (soTien < 0) soTien = 0;
                    donThueXe.TongTien = soTien;
                    donThueXe.IDMGG = maGiamGia.IDMGG;
                    db.Entry(donThueXe).State = EntityState.Modified;
                    db.SaveChanges();
                }

                // Cập nhật thanh toán trong cơ sở dữ liệu
                var existingThanhToan = db.ThanhToan.FirstOrDefault(t => t.IDTT == thanhToan.IDTT);
                if (existingThanhToan != null)
                {
                    existingThanhToan.TrangThai = "Đã thanh toán";
                    existingThanhToan.SoTien = soTien;
                    existingThanhToan.NgayTT = DateTime.Now;
                    db.Entry(existingThanhToan).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    var newThanhToan = new ThanhToan
                    {
                        NgayTT = thanhToan.NgayTT,
                        TrangThai = "Đã thanh toán",
                        PhuongThuc = thanhToan.PhuongThuc,
                        IDDT = donThueXe.IDDT,
                        SoTien = thanhToan.SoTien,
                        IDMGG = maGiamGia.IDMGG
                    };
                    db.ThanhToan.Add(newThanhToan);
                    db.SaveChanges();
                }
                TempData["Message"] = "Thanh toán thành công!";
                return View("CongratulationPaymentDone");
            }
            return RedirectToAction("Home", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ApplyDiscount(string discountCode, decimal SoTien)
        {
            var discount = db.MaGiamGia.FirstOrDefault(m => m.Ma == discountCode);

            if (discount == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ." });
            }
            else if (discount.SoLanSuDung <= 0)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn sử dụng." });
            }
            else
            {
                // Giảm số lần sử dụng mã giảm giá
                discount.SoLanSuDung--;
                db.Entry(discount).State = EntityState.Modified;
                db.SaveChanges();

                // Tính toán số tiền mới
                var discountedAmount = SoTien - (SoTien * discount.PhanTramGiam / 100);
                if (discountedAmount < 0) discountedAmount = 0;

                // Trả số tiền đã giảm
                return Json(new { success = true, discountedAmount });
            }
        }

        public ActionResult CongratulationPaymentDone()
        {
            return View();
        }
    }
}
