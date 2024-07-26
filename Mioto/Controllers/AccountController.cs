using Mioto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using BCrypt.Net;
using System.Web.Security;
using System.Linq.Expressions;

namespace Mioto.Controllers
{
    public class AccountController : Controller
    {
        DB_MiotoEntities db = new DB_MiotoEntities();
        List<SelectListItem> gioitinh = new List<SelectListItem>
        {
         new SelectListItem { Text = "Nam", Value = "Nam" },
         new SelectListItem { Text = "Nữ", Value = "Nữ" }
        };

        // GET: Account/Login
        public ActionResult Login()
        {
            if (Session["KhachHang"] != null || Session["NhanVien"] != null)
                return Logout();
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(MD_Login _user)
        {
            var IsGuest = db.KhachHang.SingleOrDefault(s => s.Email == _user.Email && s.MatKhau == _user.MatKhau);
            var IsChuXe = db.ChuXe.SingleOrDefault(s => s.Email == _user.Email && s.MatKhau == _user.MatKhau);
            var IsNhanVien = db.NhanVien.SingleOrDefault(s => s.Email == _user.Email && s.MatKhau == _user.MatKhau);
            var GPLX = db.GPLX.SingleOrDefault(x => x.IDKH == IsGuest.IDKH);
            // Khách hàng
            if(IsGuest != null)
            {
                Session["KhachHang"] = IsGuest;
                Session["ChuXe"] = IsChuXe;
                Session["GPLX"] = GPLX;
                return RedirectToAction("Home", "Home");
            }

            // Nhân viên
            if(IsNhanVien != null)
            {
                Session["NhanVien"] = IsNhanVien;
                return RedirectToAction("Home", "Home");
            }
            
            ViewBag.ErrorLogin = "Email hoặc mật khẩu không chính xác";
            return View();
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            ViewBag.GioiTinh = gioitinh;
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(MD_Register kh)
        {
            ViewBag.GioiTinh = gioitinh;
            try
            {
                if (ModelState.IsValid)
                {
                    if (db.KhachHang.Any(x => x.Email == kh.Email))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại. Vui lòng sử dụng email khác.");
                        return View(kh);
                    }
                    var newKhachHang = new KhachHang
                    {
                        Ten = kh.Ten,
                        Email = kh.Email,
                        GioiTinh = kh.GioiTinh,
                        DiaChi = kh.DiaChi,
                        SDT = kh.SDT,
                        SoGPLX = "No",
                        NgaySinh = kh.NgaySinh,
                        MatKhau = kh.MatKhau,
                        CCCD = "No"
                    };
                    db.KhachHang.Add(newKhachHang);
                    db.SaveChanges();

                    var newCCCD = new CCCD
                    {
                        SoCCCD = "No",
                        Ten = kh.Ten,
                        NgaySinh = kh.NgaySinh,
                        GioiTinh = kh.GioiTinh,
                        DiaChi = kh.DiaChi,
                        IDKH = newKhachHang.IDKH,
                        TrangThai = newKhachHang.CCCD
                    };
                    db.CCCD.Add(newCCCD);
                    db.SaveChanges();

                    var newGPLX = new GPLX
                    {
                        IDKH = newKhachHang.IDKH,
                        Ten = kh.Ten,
                        NgaySinh = kh.NgaySinh,
                        SoGPLX = newKhachHang.SoGPLX,
                        TrangThai = newKhachHang.SoGPLX
                    };
                    db.GPLX.Add(newGPLX);
                    db.SaveChanges();

                    TempData["Message"] = "Đăng ký thành công!";
                    return RedirectToAction("Login");
                }
                return View(kh);
            }
            catch
            {
                ViewBag.ErrorRegister = "Đăng ký không thành công. Vui lòng thử lại.";
                return View(kh);
            }
        }

        //GET : Home/Logout
        public ActionResult Logout()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Home");
        }

    }

}

