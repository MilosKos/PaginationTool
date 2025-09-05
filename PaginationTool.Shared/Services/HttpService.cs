using PaginationTool.Shared.Models;
using System.Text.Json;

namespace PaginationTool.Shared.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;

        public HttpService(HttpClient httpClient, TokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
        }

        public async Task<PaginatedResponse<T>> GetPaginatedDataAsync<T>(
            string url, 
            string tenantId, 
            AuthenticationConfig authConfig)
        {
            var token = await _tokenService.GetAccessTokenAsync(authConfig);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("x-raet-tenant-id", tenantId);

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PaginatedResponse<T>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new PaginatedResponse<T>();
        }

        public async Task<PaginatedResponse<object>> GetPaginatedDataAsync(
            string url, 
            string tenantId, 
            AuthenticationConfig authConfig)
        {
            return await GetPaginatedDataAsync<object>(url, tenantId, authConfig);
        }

        public string BuildUrlWithParameters(string baseUrl, int? take = null, string? nextToken = null)
        {
            var uriBuilder = new UriBuilder(baseUrl);
            var queryParams = new List<string>();

            // Parse existing query parameters
            if (!string.IsNullOrEmpty(uriBuilder.Query))
            {
                var existingQuery = uriBuilder.Query.TrimStart('?');
                if (!string.IsNullOrEmpty(existingQuery))
                {
                    queryParams.Add(existingQuery);
                }
            }

            if (take.HasValue)
            {
                queryParams.Add($"take={take.Value}");
            }

            if (!string.IsNullOrEmpty(nextToken))
            {
                queryParams.Add($"nextToken={Uri.EscapeDataString(nextToken)}");
            }

            uriBuilder.Query = string.Join("&", queryParams);
            return uriBuilder.ToString();
        }

        public string? ExtractNextTokenFromUrl(string? nextLink)
        {
            if (string.IsNullOrEmpty(nextLink))
                return null;

            try
            {
                // Extract query string part directly from the URL string
                var queryStart = nextLink.IndexOf('?');
                if (queryStart == -1)
                    return null;

                var queryString = nextLink.Substring(queryStart + 1);
                if (string.IsNullOrEmpty(queryString))
                    return null;

                // Parse query parameters manually
                var pairs = queryString.Split('&');
                
                foreach (var pair in pairs)
                {
                    var parts = pair.Split('=', 2);
                    if (parts.Length == 2 && parts[0] == "nextToken")
                    {
                        return Uri.UnescapeDataString(parts[1]);
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
