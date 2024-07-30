
using System;
using System.Collections.Generic;
using System.IO;
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

                    // Lấy hoặc thêm mới ChuXe và lấy IDCX
                    var existingCX = db.ChuXe.SingleOrDefault(x => x.IDKH == guest.IDKH);
                    if (existingCX == null)
                    {
                        var newCX = new ChuXe
                        {
                            IDKH = guest.IDKH,
                            Ten = guest.Ten,
                            Email = guest.Email,
                            SDT = guest.SDT,
                            DiaChi = guest.DiaChi,
                            MatKhau = guest.MatKhau,
                            GioiTinh = guest.GioiTinh,
                            NgaySinh = guest.NgaySinh,
                            SoGPLX = guest.SoGPLX,
                            CCCD = guest.CCCD,
                            HinhAnh = guest.HinhAnh,
                            TrangThai = "Yes"
                        };
                        db.ChuXe.Add(newCX);
                        db.SaveChanges();
                        existingCX = newCX;
                    }

                    // Xử lý ảnh
                    var hinhAnhList = new List<string>();
                    foreach (string fileName in Request.Files)
                    {
                        var file = Request.Files[fileName];
                        if (file != null && file.ContentLength > 0)
                        {
                            var uploadedFileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/CarImages/"), uploadedFileName);
                            file.SaveAs(path);
                            hinhAnhList.Add(uploadedFileName);
                        }
                    }

                    // Lưu thông tin xe vào cơ sở dữ liệu
                    var xe = new Xe
                    {
                        IDCX = existingCX.IDCX,
                        BienSoXe = cx.BienSoXe,
                        HangXe = cx.HangXe,
                        MauXe = cx.MauXe,
                        SoGhe = cx.SoGhe,
                        TinhNang = cx.TinhNang,
                        GiaThue = cx.GiaThue,
                        NamSanXuat = cx.NamSanXuat,
                        KhuVuc = cx.KhuVuc,
                        DonGiaVanChuyen = 0,
                        TrangThai = "Sẵn sàng",
                        HinhAnh = string.Join(";", hinhAnhList)
                    };

                    db.Xe.Add(xe);
                    db.SaveChanges();
                    Session["ChuXe"] = existingCX;
                    return RedirectToAction("Home", "Home");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu: " + ex.Message);
            }
            return View(cx);
        }

    }
}
