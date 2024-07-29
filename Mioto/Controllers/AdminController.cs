using Mioto.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Mioto.Controllers
{
    public class AdminController : Controller
    {

        DB_MiotoEntities db = new DB_MiotoEntities();
        public bool IsLoggedIn { get => Session["NhanVien"] != null;}

        // GET: Admin
        public ActionResult AdminPaymentVerification()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var ds_thanhtoan = db.ThanhToan.ToList();
            return View(ds_thanhtoan);
        }

        public ActionResult ApprovePayment(int idtt)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            try
            {
                var thanhtoan = db.ThanhToan.Find(idtt);
                if (thanhtoan == null)
                    return HttpNotFound();

                var donthuexe = db.DonThueXe.FirstOrDefault(x => x.IDDT == thanhtoan.IDDT);
                if (donthuexe == null)
                    return HttpNotFound();

                var xe = db.Xe.FirstOrDefault(x => x.BienSoXe == donthuexe.BienSoXe);
                if (xe == null)
                    return HttpNotFound();

                var chuxe = db.ChuXe.FirstOrDefault(x => x.IDCX == xe.IDCX);
                if (chuxe == null)
                    return HttpNotFound();

                thanhtoan.TrangThai = "Đã thanh toán";
                db.Entry(thanhtoan).State = EntityState.Modified;

                var doanhThuChuXe = db.DoanhThuChuXe.FirstOrDefault(d => d.IDCX == chuxe.IDCX);
                if (doanhThuChuXe == null)
                {
                    doanhThuChuXe = new DoanhThuChuXe
                    {
                        IDCX = chuxe.IDCX,
                        DoanhThuNgay = 0,
                        DoanhThuTuan = 0,
                        DoanhThuThang = 0,
                        DoanhThuNam = 0,
                        NgayCapNhat = DateTime.Now,
                        SoTuanTrongNam = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                    };
                    db.DoanhThuChuXe.Add(doanhThuChuXe);
                    db.SaveChanges();
                }

                var ngayHienTai = DateTime.Now;
                var soTienThanhToan = thanhtoan.SoTien;

                if (doanhThuChuXe.NgayCapNhat.Day != ngayHienTai.Day)
                {
                    doanhThuChuXe.DoanhThuNgay = 0;
                    doanhThuChuXe.NgayCapNhat = ngayHienTai;
                }
                doanhThuChuXe.DoanhThuNgay += soTienThanhToan;

                var soTuanTrongNam = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(ngayHienTai, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                if (doanhThuChuXe.SoTuanTrongNam != soTuanTrongNam)
                {
                    doanhThuChuXe.DoanhThuTuan = 0;
                    doanhThuChuXe.SoTuanTrongNam = soTuanTrongNam;
                }
                doanhThuChuXe.DoanhThuTuan += soTienThanhToan;

                if (doanhThuChuXe.NgayCapNhat.Month != ngayHienTai.Month)
                {
                    doanhThuChuXe.DoanhThuThang = 0;
                }
                doanhThuChuXe.DoanhThuThang += soTienThanhToan;

                if (doanhThuChuXe.NgayCapNhat.Year != ngayHienTai.Year)
                {
                    doanhThuChuXe.DoanhThuNam = 0;
                }
                doanhThuChuXe.DoanhThuNam += soTienThanhToan;

                db.Entry(doanhThuChuXe).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("AdminPaymentVerification");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ModelState.AddModelError("", "Xảy ra lỗi đồng bộ hóa lạc quan: " + ex.Message);
                return RedirectToAction("AdminPaymentVerification");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Xảy ra lỗi: " + ex.Message);
                return RedirectToAction("AdminPaymentVerification");
            }
        }

        // GET: DetailAccount
        public ActionResult InfoAccount()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var guest = Session["NhanVien"] as NhanVien;
            if (guest == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Khách hàng không tồn tại");
            var kh = db.KhachHang.Where(x => x.IDKH == guest.IDNV);
            return View(kh);
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
                    var path = Path.Combine(Server.MapPath("~/AvatarAdmin/"), fileName);

                    // Lưu file lên server
                    avatar.SaveAs(path);

                    // Cập nhật thông tin hình ảnh của nhân viên trong cơ sở dữ liệu
                    var guest = Session["NhanVien"] as NhanVien;
                    var existingNV = db.NhanVien.Find(guest.IDNV);

                    if (existingNV != null)
                    {
                        existingNV.HinhAnh = fileName;
                        // Cập nhật cơ sở dữ liệu
                        Session["NhanVien"] = existingNV;
                        db.Entry(existingNV).State = EntityState.Modified;
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

        public ActionResult ManagerDiscount()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var mgg = db.MaGiamGia.ToList();
            return View(mgg);
        }

        // GET: Discount/AddDiscount
        [HttpGet]
        public ActionResult AddDiscount()
        {
            return View();
        }

        // POST: Discount/AddDiscount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDiscount(MD_MaGiamGia model)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra điều kiện ngày bắt đầu và ngày kết thúc
                    if (model.NgayBatDau >= model.NgayKetThuc)
                    {
                        ModelState.AddModelError("", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
                        return View(model);
                    }

                    var newDiscount = new MaGiamGia
                    {
                        Ma = model.Ma,
                        PhanTramGiam = model.PhanTramGiam,
                        NgayBatDau = model.NgayBatDau,
                        NgayKetThuc = model.NgayKetThuc,
                        SoLanSuDung = model.SolanSuDung
                    };

                    db.MaGiamGia.Add(newDiscount);
                    db.SaveChanges();

                    return RedirectToAction("ManagerDiscount");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Xảy ra lỗi khi thêm mã giảm giá: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: Discount/EditDiscount/5
        [HttpGet]
        public ActionResult EditDiscount(int idmgg)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            // Tìm mã giảm giá theo id
            var discount = db.MaGiamGia.Find(idmgg);
            if (discount == null)
            {
                return HttpNotFound();
            }

            return View(discount);
        }

        // POST: Discount/EditDiscount/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDiscount(MaGiamGia model)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra điều kiện ngày
                    if (model.NgayBatDau >= model.NgayKetThuc)
                    {
                        ModelState.AddModelError("", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
                        return View(model);
                    }

                    // Tìm mã giảm giá theo id
                    var existingDiscount = db.MaGiamGia.Find(model.IDMGG);
                    if (existingDiscount == null)
                    {
                        return HttpNotFound();
                    }

                    // Cập nhật các thuộc tính của mã giảm giá
                    existingDiscount.Ma = model.Ma;
                    existingDiscount.PhanTramGiam = model.PhanTramGiam;
                    existingDiscount.NgayBatDau = model.NgayBatDau;
                    existingDiscount.NgayKetThuc = model.NgayKetThuc;
                    existingDiscount.SoLanSuDung = model.SoLanSuDung;

                    db.Entry(existingDiscount).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("ManagerDiscount");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Xảy ra lỗi khi cập nhật mã giảm giá: " + ex.Message);
                }
            }
            return View(model);
        }

        public ActionResult DeleteDiscount(int idmgg)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            try
            {
                var discount = db.MaGiamGia.Find(idmgg);
                if (discount == null)
                {
                    return HttpNotFound();
                }
                db.MaGiamGia.Remove(discount);
                db.SaveChanges();

                return RedirectToAction("ManagerDiscount");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Xảy ra lỗi khi xóa mã giảm giá: " + ex.Message);
                return RedirectToAction("ManagerDiscount");
            }
        }

        public ActionResult ListCarsInSystem()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var xe = db.Xe.ToList();
            return View();
        }

        public ActionResult RentedCompany()
        {
            // Kiểm tra người dùng đã đăng nhập
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            // Lấy ID từ Session NhanVien đang đăng nhập hiện tại
            var nhanvien = Session["NhanVien"] as NhanVien;

            // Lấy ngày, tuần, tháng, năm hiện tại
            var ngayHienTai = DateTime.Now;
            var tuanHienTai = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(ngayHienTai, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var thangHienTai = ngayHienTai.Month;
            var namHienTai = ngayHienTai.Year;

            // Tính doanh thu công ty theo ngày, tuần, tháng, năm
            var thanhToanDaThanhToan = db.ThanhToan
                                         .Where(x => x.TrangThai == "Đã thanh toán")
                                         .ToList();

            decimal doanhThuNgay = 0;
            decimal doanhThuTuan = 0;
            decimal doanhThuThang = 0;
            decimal doanhThuNam = 0;

            foreach (var thanhToan in thanhToanDaThanhToan)
            {
                var donThueXe = db.DonThueXe.FirstOrDefault(x => x.IDDT == thanhToan.IDDT);
                if (donThueXe != null)
                {
                    decimal hoaHong = thanhToan.SoTien * (donThueXe.PhanTramHoaHongCTyNhan / 100);

                    // Xác định thời gian của giao dịch
                    var ngayGiaoDich = thanhToan.NgayTT; 
                    var tuanGiaoDich = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(ngayGiaoDich, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    var thangGiaoDich = ngayGiaoDich.Month;
                    var namGiaoDich = ngayGiaoDich.Year;

                    if (ngayGiaoDich.Date == ngayHienTai.Date)
                        doanhThuNgay += hoaHong;

                    if (tuanGiaoDich == tuanHienTai && namGiaoDich == namHienTai)
                        doanhThuTuan += hoaHong;

                    if (thangGiaoDich == thangHienTai && namGiaoDich == namHienTai)
                        doanhThuThang += hoaHong;

                    if (namGiaoDich == namHienTai)
                        doanhThuNam += hoaHong;
                }
            }

            // Kiểm tra xem đã có doanh thu công ty trong cơ sở dữ liệu chưa
            var doanhThuCongTy = db.DoanhThuCongTy
                .FirstOrDefault(d =>
                    d.NgayCapNhat.Year == namHienTai &&
                    d.NgayCapNhat.Month == thangHienTai &&
                    d.NgayCapNhat.Day == ngayHienTai.Day);

            if (doanhThuCongTy == null)
            {
                // Nếu chưa có, tạo mới và lưu vào cơ sở dữ liệu
                doanhThuCongTy = new DoanhThuCongTy
                {
                    IDNV = nhanvien.IDNV,
                    DoanhThuNgay = doanhThuNgay,
                    DoanhThuTuan = doanhThuTuan,
                    DoanhThuThang = doanhThuThang,
                    DoanhThuNam = doanhThuNam,
                    NgayCapNhat = ngayHienTai
                };
                db.DoanhThuCongTy.Add(doanhThuCongTy);
            }
            else
            {
                // Cập nhật doanh thu hiện tại
                doanhThuCongTy.DoanhThuNgay = doanhThuNgay;
                doanhThuCongTy.DoanhThuTuan = doanhThuTuan;
                doanhThuCongTy.DoanhThuThang = doanhThuThang;
                doanhThuCongTy.DoanhThuNam = doanhThuNam;
                doanhThuCongTy.NgayCapNhat = ngayHienTai;
                db.Entry(doanhThuCongTy).State = EntityState.Modified;
            }

            db.SaveChanges();

            // Truyền dữ liệu doanh thu tới ViewModel
            var model = new DoanhThuCongTy
            {
                DoanhThuNgay = doanhThuCongTy.DoanhThuNgay,
                DoanhThuTuan = doanhThuCongTy.DoanhThuTuan,
                DoanhThuThang = doanhThuCongTy.DoanhThuThang,
                DoanhThuNam = doanhThuCongTy.DoanhThuNam
            };

            return View(model);
        }


    }
}