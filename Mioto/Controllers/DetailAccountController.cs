﻿using Mioto.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
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
                    kh.CCCD = kh.CCCD;
                    kh.SoGPLX = guest.SoGPLX;
                    kh.MatKhau = guest.MatKhau;
                    kh.IDKH = kh.IDKH;
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
                            CCCD = kh.CCCD,
                            NgaySinh = kh.NgaySinh,
                            SoGPLX = kh.SoGPLX,
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
        public ActionResult ChangeAvatarUser(string filename)
        {
            var kh = Session["KhachHang"] as KhachHang;
            if(kh.HinhAnh == null)
            {

            }
            return View();

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
        // API Check GPLX

    }
}