using Mioto.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Mioto.Controllers
{
    public class DetailAccountController : Controller
    {
        DB_MiotoEntities db = new DB_MiotoEntities();
        public bool IsLoggedIn { get => Session["KhachHang"] != null; }
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
        // GET: DetailAccount
        public ActionResult InfoAccount()
        {
            return View();
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