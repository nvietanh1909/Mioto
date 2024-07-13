using Mioto.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
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
            if (guest == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Khách hàng không tồn tại");
            var cars = db.Xe.Where(x => x.IDCX == guest.IDKH).ToList();
            return View(cars);
        }

        // GET: EditCar/MyCar
        public ActionResult EditCar(string BienSoXe)
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
            ViewBag.TinhThanhPho = tinhThanhPho;
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
            try
            {
                if (ModelState.IsValid)
                {
                    var guest = Session["KhachHang"] as KhachHang;
                    xe.IDCX = guest.IDKH;
                    xe.KhuVuc = xe.KhuVuc.ToString();
                    xe.TrangThai = "Sẵn sàng";
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

        public ActionResult DeleteCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            try
            {
                var xe = db.Xe.FirstOrDefault(x => x.BienSoXe == BienSoXe);
                if (xe != null)
                {
                    db.Xe.Remove(xe);
                    db.SaveChanges();
                    return RedirectToAction("MyCar");
                }
                return HttpNotFound();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi hoặc xử lý lỗi
                ViewBag.ErrorMessage = "Không thể xóa xe: " + ex.Message;
                return View("Error"); // Hoặc trang lỗi tùy chỉnh của bạn
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
                    db.Entry(kh).State = EntityState.Modified;
                    db.SaveChanges();

                    if(chuxe != null)
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
                            TrangThai = "Hoạt động"
                        };
                        db.Entry(newChuXe).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    return RedirectToAction("InfoAccount");
                }
                return View(kh);
            }
            catch
            {
                return View(kh);
            }
        }
        public ActionResult MyTrip()
        {
            return View();
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
            return View();
        }
       
    }
}