using System.Net;
using System.Net.Http.Json;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Services;

public class RaceDayApiService
{
    private readonly HttpClient _http;

    public RaceDayApiService(HttpClient http)
    {
        _http = http;
    }

    // ---- AUTH ----
    public async Task<ApiResult<UserVm>> RegisterAsync(RegisterVm vm)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/register", vm);
        return await ReadAsync<UserVm>(resp);
    }

    public async Task<ApiResult<UserVm>> LoginAsync(LoginVm vm)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/login", vm);
        return await ReadAsync<UserVm>(resp);
    }

    public async Task LogoutAsync()
    {
        await _http.PostAsync("api/auth/logout", null);
    }

    // ---- EVENTS ----
    public async Task<List<EventVm>> GetEventsAsync()
        => await _http.GetFromJsonAsync<List<EventVm>>("api/events") ?? new();

    public async Task<EventVm?> GetEventAsync(int id)
        => await _http.GetFromJsonAsync<EventVm>($"api/events/{id}");

    public async Task<List<EventVm>> GetMyEventsAsync()
        => await _http.GetFromJsonAsync<List<EventVm>>("api/events/my-events") ?? new();

    public async Task<ApiResult<int>> CreateEventAsync(CreateEventVm vm)
    {
        var resp = await _http.PostAsJsonAsync("api/events", vm);
        return await ReadCreatedAsync(resp, "eventId");
    }

    public async Task<ApiResult> UpdateEventAsync(int id, CreateEventVm vm)
    {
        var resp = await _http.PutAsJsonAsync($"api/events/{id}", vm);
        return await ReadEmptyAsync(resp);
    }

    public async Task<ApiResult> DeleteEventAsync(int id)
    {
        var resp = await _http.DeleteAsync($"api/events/{id}");
        return await ReadEmptyAsync(resp);
    }

    public async Task<ApiResult<string>> UploadBannerAsync(int eventId, Stream fileStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var resp = await _http.PostAsync($"api/events/{eventId}/banner", content);
        return await ReadStringAsync(resp);
    }

    // ---- CATEGORIES ----
    public async Task<List<CategoryVm>> GetCategoriesAsync(int eventId)
        => await _http.GetFromJsonAsync<List<CategoryVm>>($"api/events/{eventId}/categories") ?? new();

    public async Task<ApiResult> CreateCategoryAsync(int eventId, CreateCategoryVm vm)
    {
        var resp = await _http.PostAsJsonAsync($"api/events/{eventId}/categories", vm);
        return await ReadEmptyAsync(resp);
    }

    public async Task<ApiResult> DeleteCategoryAsync(int eventId, int categoryId)
    {
        var resp = await _http.DeleteAsync($"api/events/{eventId}/categories/{categoryId}");
        return await ReadEmptyAsync(resp);
    }

    // ---- ENROLMENTS ----
    public async Task<List<EnrolmentVm>> GetEnrolmentsAsync()
        => await _http.GetFromJsonAsync<List<EnrolmentVm>>("api/enrolments") ?? new();

    public async Task<ApiResult> CreateEnrolmentAsync(CreateEnrolmentVm vm)
    {
        var resp = await _http.PostAsJsonAsync("api/enrolments", vm);
        return await ReadEmptyAsync(resp);
    }

    public async Task<ApiResult> CancelEnrolmentAsync(int id)
    {
        var resp = await _http.DeleteAsync($"api/enrolments/{id}");
        return await ReadEmptyAsync(resp);
    }

    // ---- RESULTS ----
    public async Task<List<ResultVm>> GetResultsAsync()
        => await _http.GetFromJsonAsync<List<ResultVm>>("api/results") ?? new();

    public async Task<ApiResult> CreateResultAsync(CreateResultVm vm)
    {
        var resp = await _http.PostAsJsonAsync("api/results", vm);
        return await ReadEmptyAsync(resp);
    }

    // ---- PROFILE ----
    public async Task<UserVm?> GetProfileAsync()
        => await _http.GetFromJsonAsync<UserVm>("api/profile");

    public async Task<ApiResult> UpdateProfileAsync(UpdateProfileVm vm)
    {
        var resp = await _http.PutAsJsonAsync("api/profile", vm);
        return await ReadEmptyAsync(resp);
    }

    public async Task<ApiResult<string>> UploadProfilePictureAsync(Stream fileStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var resp = await _http.PostAsync("api/profile/picture", content);
        return await ReadStringAsync(resp);
    }

    // ---- HELPERS ----
    private static async Task<ApiResult<T>> ReadAsync<T>(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode)
        {
            var data = await resp.Content.ReadFromJsonAsync<T>();
            return ApiResult<T>.Ok(data);
        }
        return ApiResult<T>.Fail(await ExtractErrorAsync(resp));
    }

    private static async Task<ApiResult<int>> ReadCreatedAsync(HttpResponseMessage resp, string idField)
    {
        if (resp.IsSuccessStatusCode)
        {
            var doc = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (doc != null && doc.TryGetValue(idField, out var val))
            {
                return ApiResult<int>.Ok(0);
            }
            return ApiResult<int>.Ok(0);
        }
        return ApiResult<int>.Fail(await ExtractErrorAsync(resp));
    }

    private static async Task<ApiResult<string>> ReadStringAsync(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode)
        {
            var doc = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (doc != null && doc.TryGetValue("url", out var url))
                return ApiResult<string>.Ok(url);
            return ApiResult<string>.Ok(string.Empty);
        }
        return ApiResult<string>.Fail(await ExtractErrorAsync(resp));
    }

    private static async Task<ApiResult> ReadEmptyAsync(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return ApiResult.Ok();
        return ApiResult.Fail(await ExtractErrorAsync(resp));
    }

    private static async Task<string> ExtractErrorAsync(HttpResponseMessage resp)
    {
        if (resp.StatusCode == HttpStatusCode.Unauthorized) return "You are not signed in.";
        if (resp.StatusCode == HttpStatusCode.Forbidden) return "You don't have permission for that.";
        try
        {
            var body = await resp.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(body))
            {
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                if (dict != null && dict.TryGetValue("message", out var msg)) return msg;
                return body;
            }
        }
        catch { }
        return $"Error {(int)resp.StatusCode}: {resp.ReasonPhrase}";
    }
}

