using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mioto.Models
{
    public class ApiServices : ApiController
    {
        private readonly HttpClient _client;
        private readonly DB_MiotoEntities _context;

        public ApiServices()
        {
            _client = new HttpClient();
            _context = new DB_MiotoEntities();
        }

        // API Kiểm tra GPLX
        public async Task<string> VerifyDrivingLicenseAsync(string session, string donViXuLy, string maHoSo)
        {
            try
            {
                var requestBody = new
                {
                    session = session,
                    service = "TraCuuHoSo",
                    donViXuLy = donViXuLy,
                    mahoso = maHoSo
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync("ADAPTER_URL/mapi/g", content);
                var responseString = await response.Content.ReadAsStringAsync();

                return responseString;
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.WriteLine($"Error verifying driving license: {ex.Message}");
                return null;
            }
        }

        // API Kiểm tra CCCD
        public async Task<JObject> VerifyCCCDAsync(string imagePath, string apiKey)
        {
            try
            {
                var requestContent = new MultipartFormDataContent();
                requestContent.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(imagePath)), "image", "image.jpg");

                _client.DefaultRequestHeaders.Add("api-key", apiKey);
                var response = await _client.PostAsync("https://api.fpt.ai/vision/idr/vnm/", requestContent);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseBody);

                return jsonResponse;
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.WriteLine($"Error verifying CCCD: {ex.Message}");
                return null;
            }
        }

        [HttpPost]
        [Route("api/calendar/addEvent")]
        public async Task<IHttpActionResult> AddEventToGoogleCalendarAsync([FromBody] EventRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.BienSoXe) || request.IDKH <= 0 || request.NgayThue == DateTime.MinValue || request.NgayTra == DateTime.MinValue)
            {
                return BadRequest("Thông tin không hợp lệ.");
            }

            // Kiểm tra xem xe có tồn tại không
            var xe = _context.Xe.SingleOrDefault(x => x.BienSoXe == request.BienSoXe);
            if (xe == null)
            {
                return NotFound();
            }

            // Kiểm tra xem xe có bị trùng lịch không
            var overlap = _context.DonThueXe.Any(dtx =>
                dtx.BienSoXe == request.BienSoXe &&
                ((request.NgayThue >= dtx.NgayThue && request.NgayThue <= dtx.NgayTra) ||
                (request.NgayTra >= dtx.NgayThue && request.NgayTra <= dtx.NgayTra)));

            // Trả về lỗi trùng lịch
            if (overlap) return Conflict();

            // Thêm sự kiện vào lịch Google
            var calendarService = await GoogleCalendarService.GetCalendarServiceAsync();
            var newEvent = new Event()
            {
                Summary = $"Đặt xe {request.BienSoXe}",
                Location = "Địa chỉ không xác định",
                Description = $"Khách hàng ID: {request.IDKH}, Ngày thuê: {request.NgayThue.ToShortDateString()}, Ngày trả: {request.NgayTra.ToShortDateString()}",
                Start = new EventDateTime()
                {
                    DateTime = request.NgayThue,
                    TimeZone = "Asia/Ho_Chi_Minh",
                },
                End = new EventDateTime()
                {
                    DateTime = request.NgayTra,
                    TimeZone = "Asia/Ho_Chi_Minh",
                }
            };

            var calendarId = "primary";
            var insertRequest = calendarService.Events.Insert(newEvent, calendarId);
            var createdEvent = await insertRequest.ExecuteAsync();

            // Lưu đơn thuê xe vào cơ sở dữ liệu
            var donThueXe = new DonThueXe
            {
                IDKH = request.IDKH,
                BienSoXe = request.BienSoXe,
                NgayThue = request.NgayThue,
                NgayTra = request.NgayTra,
                TongTien = CalculateTotalAmount(request.BienSoXe, request.NgayThue, request.NgayTra),
                TrangThai = 1,
                PhanTramHoaHongCTyNhan = 10 //Hoa hồng 10%
            };

            _context.DonThueXe.Add(donThueXe);
            _context.SaveChanges();

            return Ok($"Sự kiện đã được thêm vào lịch Google thành công: {createdEvent.HtmlLink}");
        }

        private decimal CalculateTotalAmount(string bienSoXe, DateTime ngayThue, DateTime ngayTra)
        {
            var xe = _context.Xe.SingleOrDefault(x => x.BienSoXe == bienSoXe);
            if (xe == null)
            {
                throw new ArgumentException("Xe không tồn tại.");
            }

            var soNgayThue = (ngayTra - ngayThue).Days;
            if (soNgayThue <= 0)
            {
                throw new ArgumentException("Ngày trả phải lớn hơn ngày thuê.");
            }
            return soNgayThue * xe.GiaThue;
        }

        public async Task<HttpResponseMessage> AddEventAsync(EventRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("https://api.example.com/api/calendar/addEvent", content);
            return response;
        }

        public class EventRequest
        {
            public string BienSoXe { get; set; }
            public int IDKH { get; set; }
            public DateTime NgayThue { get; set; }
            public DateTime NgayTra { get; set; }
        }
    }
}
