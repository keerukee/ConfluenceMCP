using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConfluenceMcpServer.Configuration;

namespace ConfluenceMcpServer.Services;

/// <summary>
/// HTTP client for Confluence REST API operations supporting both Cloud and Datacenter.
/// </summary>
public static class ConfluenceClient
{
    private static readonly HttpClient _httpClient = new();
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    static ConfluenceClient()
    {
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static void EnsureConfigured()
    {
        var (isValid, error) = ConfluenceConfiguration.Validate();
        if (!isValid)
            throw new InvalidOperationException(error);

        _httpClient.DefaultRequestHeaders.Authorization =
            AuthenticationHeaderValue.Parse(ConfluenceConfiguration.GetAuthorizationHeader());
    }

    private static string BuildUrl(string endpoint, bool useV2 = false)
    {
        var baseUrl = ConfluenceConfiguration.BaseUrl.TrimEnd('/');
        var apiPath = ConfluenceConfiguration.GetApiBasePath(useV2);
        return $"{baseUrl}{apiPath}/{endpoint.TrimStart('/')}";
    }

    public static async Task<string> GetAsync(string endpoint, bool useV2 = false)
    {
        EnsureConfigured();
        var url = BuildUrl(endpoint, useV2);
        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Confluence API error ({response.StatusCode}): {content}");

        return content;
    }

    public static async Task<string> PostAsync(string endpoint, object data, bool useV2 = false)
    {
        EnsureConfigured();
        var url = BuildUrl(endpoint, useV2);
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, httpContent);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Confluence API error ({response.StatusCode}): {content}");

        return content;
    }

    public static async Task<string> PutAsync(string endpoint, object data, bool useV2 = false)
    {
        EnsureConfigured();
        var url = BuildUrl(endpoint, useV2);
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(url, httpContent);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Confluence API error ({response.StatusCode}): {content}");

        return content;
    }

    public static async Task<string> DeleteAsync(string endpoint, bool useV2 = false)
    {
        EnsureConfigured();
        var url = BuildUrl(endpoint, useV2);
        var response = await _httpClient.DeleteAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Confluence API error ({response.StatusCode}): {content}");

        return string.IsNullOrEmpty(content) ? "Deleted successfully" : content;
    }

    public static async Task<byte[]> GetBytesAsync(string endpoint, bool useV2 = false)
    {
        EnsureConfigured();
        var url = BuildUrl(endpoint, useV2);
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Confluence API error ({response.StatusCode}): {errorContent}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }

    public static async Task<string> PostMultipartAsync(string endpoint, MultipartFormDataContent content, bool useV2 = false)
    {
        EnsureConfigured();
        var url = BuildUrl(endpoint, useV2);

        // Remove default content-type for multipart
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.PostAsync(url, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Confluence API error ({response.StatusCode}): {responseContent}");

        return responseContent;
    }

    public static string FormatJsonResponse(string json)
    {
        try
        {
            var node = JsonNode.Parse(json);
            return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? json;
        }
        catch
        {
            return json;
        }
    }

    public static string ExtractResultsSummary(string json, string resultsKey = "results")
    {
        try
        {
            var node = JsonNode.Parse(json);
            var results = node?[resultsKey]?.AsArray();
            if (results == null) return json;

            var summary = new JsonObject
            {
                ["totalCount"] = results.Count,
                [resultsKey] = results
            };

            if (node?["_links"] != null)
                summary["_links"] = node["_links"]!.DeepClone();

            return summary.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }
}
