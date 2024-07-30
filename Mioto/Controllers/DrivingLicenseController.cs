using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

public class DrivingLicenseController : Controller
{
    private readonly ApiService _apiService;

    public DrivingLicenseController()
    {
        _apiService = new ApiService();
    }

    public async Task<ActionResult> GetLicenseDetails(string username, string password, string maHoSo)
    {
        // Lấy session
        var sessionResponse = await _apiService.GetSessionAsync(username, password);
        var session = ExtractSessionFromResponse(sessionResponse); 

        if (string.IsNullOrEmpty(session))
        {
            return View("Error");
        }

        // Tra cứu hồ sơ
        var recordResponse = await _apiService.LookupRecordAsync(session, "TraCuuHoSo", "Mã Đơn Vị", maHoSo);
        var recordDetails = ExtractRecordDetailsFromResponse(recordResponse); 

        // Trả về kết quả cho view
        return View(recordDetails);
    }

    private string ExtractSessionFromResponse(string responseBody)
    {
        // Xử lý JSON và trả về session
        dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
        return jsonResponse.session;
    }

    private object ExtractRecordDetailsFromResponse(string responseBody)
    {
        // Xử lý JSON và trả về thông tin hồ sơ
        dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
        return jsonResponse.data;
    }
}

public class ApiService
{
    private readonly HttpClient _client;
    private readonly string _baseAddress = "https://gplx.gov.vn/";

    public ApiService()
    {
        _client = new HttpClient
        {
            BaseAddress = new System.Uri(_baseAddress)
        };
    }

    public async Task<string> GetSessionAsync(string username, string password)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/mapi/g")
        {
            Content = new StringContent(
                $"{{ \"username\": \"{username}\", \"password\": \"{password}\" }}",
                Encoding.UTF8,
                "application/json")
        };

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody; 
    }

    public async Task<string> LookupRecordAsync(string session, string service, string donViXuLy, string maHoSo)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/mapi/g")
        {
            Content = new StringContent(
                $"{{ \"session\": \"{session}\", \"service\": \"{service}\", \"donViXuLy\": \"{donViXuLy}\", \"mahoso\": \"{maHoSo}\" }}",
                Encoding.UTF8,
                "application/json")
        };

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody; 
    }
}
