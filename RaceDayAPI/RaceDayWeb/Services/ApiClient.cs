using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDayWeb.Services
{
    public class ApiResponse
    {
        public bool Ok { get; set; }
        public int Status { get; set; }
        public string Raw { get; set; } = "";
        public string Error { get; set; } = "";

        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        public T? Data<T>()
        {
            if (string.IsNullOrWhiteSpace(Raw)) return default;
            try { return JsonSerializer.Deserialize<T>(Raw, Options); }
            catch { return default; }
        }
    }

    // All communication with the Part 2 API goes through this class.
    // The MVC app never touches the database or Azure directly.
    public class ApiClient
    {
        private const string CookieKey = "ApiCookie";
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _accessor;

        public ApiClient(HttpClient http, IHttpContextAccessor accessor)
        {
            _http = http;
            _accessor = accessor;
        }

        private ISession Session => _accessor.HttpContext!.Session;

        public Task<ApiResponse> GetAsync(string path) => SendAsync(HttpMethod.Get, path, null);
        public Task<ApiResponse> PostAsync(string path, object? body = null) => SendAsync(HttpMethod.Post, path, body);
        public Task<ApiResponse> PutAsync(string path, object? body) => SendAsync(HttpMethod.Put, path, body);
        public Task<ApiResponse> DeleteAsync(string path) => SendAsync(HttpMethod.Delete, path, null);

        public async Task<ApiResponse> SendAsync(HttpMethod method, string path, object? body)
        {
            var request = new HttpRequestMessage(method, path);
            if (body != null)
                request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            return await ExecuteAsync(request);
        }

        public async Task<ApiResponse> UploadAsync(string path, IFormFile file)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);
            var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
            return await ExecuteAsync(request);
        }

        private async Task<ApiResponse> ExecuteAsync(HttpRequestMessage request)
        {
            // Forward this user's API session cookie, stored in their MVC session.
            var cookie = Session.GetString(CookieKey);
            if (!string.IsNullOrEmpty(cookie))
                request.Headers.Add("Cookie", cookie);

            try
            {
                var response = await _http.SendAsync(request);

                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    var sessionCookie = cookies.FirstOrDefault(c => c.StartsWith(".AspNetCore.Session="));
                    if (sessionCookie != null)
                        Session.SetString(CookieKey, sessionCookie.Split(';')[0]);
                }

                var raw = await response.Content.ReadAsStringAsync();
                return new ApiResponse
                {
                    Ok = response.IsSuccessStatusCode,
                    Status = (int)response.StatusCode,
                    Raw = raw,
                    Error = response.IsSuccessStatusCode ? "" : ExtractError(raw, (int)response.StatusCode)
                };
            }
            catch (Exception)
            {
                return new ApiResponse
                {
                    Ok = false,
                    Status = 0,
                    Error = "We couldn't reach the RaceDay service. Please try again in a moment."
                };
            }
        }

        private static string ExtractError(string raw, int status)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                if (root.TryGetProperty("message", out var msg)) return msg.GetString() ?? "";
                if (root.TryGetProperty("errors", out var errors))
                {
                    foreach (var prop in errors.EnumerateObject())
                        foreach (var item in prop.Value.EnumerateArray())
                            return item.GetString() ?? "";
                }
            }
            catch { }

            return status switch
            {
                401 => "Please log in to continue.",
                403 => "You don't have permission to do that.",
                404 => "We couldn't find what you were looking for.",
                _ => "Something went wrong. Please try again."
            };
        }

        public void ClearCookie() => Session.Remove(CookieKey);
    }
}