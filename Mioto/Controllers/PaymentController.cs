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

namespace Mioto.Controllers
{
    public class PaymentController : Controller
    {
        private readonly DB_MiotoEntities db = new DB_MiotoEntities();
        private readonly CalendarService _calendarService;

        public PaymentController()
        {
            // Khởi tạo dịch vụ CalendarService bất đồng bộ
            _calendarService = InitializeCalendarService().GetAwaiter().GetResult();
        }

        private async Task<CalendarService> InitializeCalendarService()
        {
            try
            {
                return await GoogleCalendarService.GetCalendarServiceAsync();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi khởi tạo dịch vụ
                Console.WriteLine($"Error initializing Google Calendar service: {ex.Message}");
                throw;
            }
        }

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

        public async Task<ActionResult> BookingCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var khachHang = Session["KhachHang"] as KhachHang;
            var xe = db.Xe.FirstOrDefault(x => x.BienSoXe == BienSoXe);
            var chuXe = db.ChuXe.FirstOrDefault(x => x.IDCX == xe.IDCX);

            if (xe == null || chuXe == null)
                return HttpNotFound();

            var bookingCarModel = new MD_BookingCar
            {
                Xe = xe,
                ChuXe = chuXe,
            };
            return View(bookingCarModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BookingCar(MD_BookingCar bookingCar)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var khachHang = Session["KhachHang"] as KhachHang;
            if (ModelState.IsValid)
            {
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

                // Kiểm tra lịch trình xe trước khi đặt xe
                var googleEvent = new Event
                {
                    Summary = $"Booking for {donThueXe.BienSoXe}",
                    Start = new EventDateTime()
                    {
                        DateTime = donThueXe.NgayThue,
                        TimeZone = "Asia/Ho_Chi_Minh"
                    },
                    End = new EventDateTime()
                    {
                        DateTime = donThueXe.NgayTra,
                        TimeZone = "Asia/Ho_Chi_Minh"
                    }
                };

                var addEventResponse = await AddEventToGoogleCalendar(googleEvent);

                if (addEventResponse != null)
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
                    TempData["ErrorMessage"] = "Xe không khả dụng trong khoảng thời gian đã chọn.";
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
            var donThueXe = db.DonThueXe.FirstOrDefault(t => t.IDDT == thanhToan.IDDT);
            var xe = db.Xe.FirstOrDefault(t => t.BienSoXe == donThueXe.BienSoXe);

            if (thanhToan == null || donThueXe == null || xe == null)
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

                    var donThueXe = db.DonThueXe.FirstOrDefault(t => t.IDDT == existingThanhToan.IDDT);
                    if (donThueXe != null)
                    {
                        var googleEvent = new Event
                        {
                            Summary = $"Booking for {donThueXe.BienSoXe}",
                            Start = new EventDateTime()
                            {
                                DateTime = donThueXe.NgayThue,
                                TimeZone = "Asia/Ho_Chi_Minh"
                            },
                            End = new EventDateTime()
                            {
                                DateTime = donThueXe.NgayTra,
                                TimeZone = "Asia/Ho_Chi_Minh"
                            }
                        };

                        var addEventResponse = await AddEventToGoogleCalendar(googleEvent);

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

        private async Task<string> AddEventToGoogleCalendar(Event googleEvent)
        {
            try
            {
                var calendarId = "primary"; // Thay đổi nếu bạn muốn thêm vào lịch khác
                var request = _calendarService.Events.Insert(googleEvent, calendarId);
                var eventResponse = await request.ExecuteAsync();
                return eventResponse.HtmlLink; // Trả về liên kết sự kiện trong lịch
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding event to Google Calendar: {ex.Message}");
                return null;
            }
        }
    }
}
