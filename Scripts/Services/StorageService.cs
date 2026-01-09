using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Lender.Services.Constants;

namespace Lender.Services;

public class StorageService
{
    private readonly HttpClient _httpClient = new();

    public async Task<string?> UploadImageAsync(Stream imageStream, string fileName, string contentType = "image/jpeg")
    {
        try
        {
            var token = await SecureStorage.GetAsync(AuthenticationConstants.FirebaseTokenKey);
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("[StorageService] No Firebase token found for upload");
                return null;
            }

            var objectName = $"collateral/{Guid.NewGuid()}_{fileName}";
            var url = $"https://firebasestorage.googleapis.com/v0/b/{StorageConstants.Bucket}/o?name={Uri.EscapeDataString(objectName)}";

            var content = new StreamContent(imageStream);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[StorageService] Upload response {response.StatusCode}: {body}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = JsonSerializer.Deserialize<JsonElement>(body);
            var downloadToken = json.TryGetProperty("downloadTokens", out var dt) ? dt.GetString() : null;
            var encodedPath = Uri.EscapeDataString(objectName);
            var downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/{StorageConstants.Bucket}/o/{encodedPath}?alt=media";
            if (!string.IsNullOrEmpty(downloadToken)) downloadUrl += $"&token={downloadToken}";
            return downloadUrl;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StorageService] Upload exception: {ex.Message}");
            return null;
        }
    }
}
