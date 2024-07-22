using Mioto.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mioto.Controllers
{
    public class PaymentController : Controller
    {
        private DB_MiotoEntities db = new DB_MiotoEntities();
        private readonly ApiServices _apiServices = new ApiServices();

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

        public async Task<ActionResult> BookingCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var KhachHang = Session["KhachHang"] as KhachHang;
            var xe = db.Xe.FirstOrDefault(x => x.BienSoXe == BienSoXe);
            var cx = db.ChuXe.FirstOrDefault(x => x.IDCX == xe.IDCX);
            var gplx = db.GPLX.FirstOrDefault(x => x.IDKH == KhachHang.IDKH);

            if (xe == null || cx == null)
                return HttpNotFound();

            //Check GPLX
            if (KhachHang != null)
            {
                var session = "your_session"; 
                var donViXuLy = "your_donViXuLy"; 
                var maHoSo = gplx.SoGPLX;

                var response = await _apiServices.VerifyDrivingLicenseAsync(session, donViXuLy, maHoSo);
                var jsonResponse = JObject.Parse(response);

                if (jsonResponse["status"].ToString() == "success")
                {
                    var bookingCarModel = new MD_BookingCar
                    {
                        Xe = xe,
                        ChuXe = cx,
                    };
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
        public async Task<ActionResult> BookingCar(MD_BookingCar bookingCar)
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
                    TrangThai = 1,
                    PhanTramHoaHongCTyNhan = 10,
                    TongTien = (bookingCar.Xe.GiaThue * (bookingCar.NgayTra - bookingCar.NgayThue).Days)
                };

                // Kiểm tra lịch trình xe trước khi đặt xe
                var eventRequest = new ApiServices.EventRequest
                {
                    BienSoXe = donThueXe.BienSoXe,
                    IDKH = donThueXe.IDKH,
                    NgayThue = donThueXe.NgayThue,
                    NgayTra = donThueXe.NgayTra
                };

                var addEventResponse = await _apiServices.AddEventAsync(eventRequest);

                if (addEventResponse.IsSuccessStatusCode)
                {
                    db.DonThueXe.Add(donThueXe);
                    db.SaveChanges();

                    var thanhToan = new ThanhToan
                    {
                        IDDT = donThueXe.IDDT,
                        NgayTT = DateTime.Now,
                        SoTien = donThueXe.TongTien,
                        TrangThai = "No",
                        PhuongThuc = "Online"
                    };

                    db.ThanhToan.Add(thanhToan);
                    db.SaveChanges();

                    TempData["Message"] = "Đặt xe thành công! Vui lòng thanh toán để hoàn tất giao dịch.";
                    return RedirectToAction("Payment", new { idtt = thanhToan.IDTT });
                }
                else
                {
                    var errorMessage = await addEventResponse.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Xe không khả dụng trong khoảng thời gian đã chọn: " + errorMessage;
                    return RedirectToAction("InfoCar", new { BienSoXe = bookingCar.Xe.BienSoXe });
                }
            }

            return View(bookingCar);
        }

        public ActionResult Payment(int idtt)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var thanhToan = db.ThanhToan.FirstOrDefault(t => t.IDTT == idtt);
            var dtx = db.DonThueXe.FirstOrDefault(t => t.IDDT == thanhToan.IDDT);
            var xe = db.Xe.FirstOrDefault(t => t.BienSoXe == dtx.BienSoXe);

            if (thanhToan == null || dtx == null || xe == null)
            {
                return HttpNotFound();
            }

            Session["Xe"] = xe;
            return View(thanhToan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Payment(ThanhToan thanhToan)
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

                    var dtx = db.DonThueXe.FirstOrDefault(t => t.IDDT == existingThanhToan.IDDT);
                    if (dtx != null)
                    {
                        var addEventResponse = await AddEventCalendar(dtx);
                        if (addEventResponse != null)
                        {
                            TempData["Message"] = "Thanh toán thành công và sự kiện đã được thêm vào lịch!";
                        }
                        else
                        {
                            TempData["Message"] = "Thanh toán thành công nhưng không thể thêm sự kiện vào lịch.";
                        }
                    }
                }

                TempData["Message"] = "Thanh toán thành công!";
                return RedirectToAction("Home", "Home");
            }

            return View(thanhToan);
        }

        private async Task<string> AddEventCalendar(DonThueXe donThueXe)
        {
            var eventRequest = new ApiServices.EventRequest
            {
                BienSoXe = donThueXe.BienSoXe,
                IDKH = donThueXe.IDKH,
                NgayThue = donThueXe.NgayThue,
                NgayTra = donThueXe.NgayTra
            };

            var response = await _apiServices.AddEventAsync(eventRequest);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null;
        }

    }
}
