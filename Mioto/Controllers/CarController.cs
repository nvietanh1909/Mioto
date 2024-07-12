using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mioto.Models;

namespace Mioto.Controllers
{
    public class CarController : Controller
    {
        private DB_MiotoEntities db = new DB_MiotoEntities();

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

        // GET: Car/RegisterOwner
        public ActionResult RegisterOwner()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Home", "Home");
            ViewBag.TinhThanhPho = tinhThanhPho;
            return View();
        }

        // POST: Car/RegisterOwner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterOwner(MD_Xe xe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Home");
            ViewBag.TinhThanhPho = tinhThanhPho;
            try
            {
                if (ModelState.IsValid)
                {
                    if (db.Xe.Any(x => x.BienSoXe == xe.BienSoXe))
                    {
                        ModelState.AddModelError("BienSoXe", "Biển số xe đã đăng ký trên hệ thống");
                        return View(xe);
                    }
                    var newCar = new Xe
                    {
                        BienSoXe = xe.BienSoXe,
                        HangXe = xe.HangXe,
                        MauXe = xe.MauXe,
                        SoGhe = xe.SoGhe,
                        TinhNang = xe.TinhNang,
                        GiaThue = xe.GiaThue,
                        NamSanXuat = xe.NamSanXuat,
                        KhuVuc = xe.KhuVuc,
                    };

                    db.Xe.Add(newCar);
                    db.SaveChanges();
                    TempData["Message"] = "Đăng ký thành công!";
                    return RedirectToAction("Home", "Home");
                }
                return View(xe);
            }
            catch
            {
                ViewBag.ErrorRegister = "Đăng ký không thành công. Vui lòng thử lại.";
                ViewBag.TinhThanhPho = tinhThanhPho;
                return View(xe);
            }
        }
    }
}
