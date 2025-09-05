using PaginationTool.Shared.Models;
using System.Text;
using System.Text.Json;

namespace PaginationTool.Shared.Services
{
    public class TokenService
    {
        private readonly HttpClient _httpClient;
        private string? _cachedToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public TokenService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetAccessTokenAsync(AuthenticationConfig config)
        {
            // If Bearer token is provided directly, use it
            if (!string.IsNullOrEmpty(config.BearerToken))
            {
                return config.BearerToken;
            }

            // If we have a cached token that's still valid, use it
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            // Request new token using client credentials
            if (string.IsNullOrEmpty(config.ClientId) || string.IsNullOrEmpty(config.ClientSecret))
            {
                throw new InvalidOperationException("Either BearerToken or ClientId/ClientSecret must be provided");
            }

            var tokenResponse = await RequestTokenAsync(config);
            _cachedToken = tokenResponse.AccessToken;
            int expiresIn = int.TryParse(tokenResponse.ExpiresIn, out int expiresInOut) ? expiresInOut : 3600;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60); // Refresh 1 minute early

            return _cachedToken;
        }

        private async Task<TokenResponse> RequestTokenAsync(AuthenticationConfig config)
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new("client_id", config.ClientId!),
                new("client_secret", config.ClientSecret!),
                new("grant_type", "client_credentials")
            };

            using var formContent = new FormUrlEncodedContent(formData);
            
            var response = await _httpClient.PostAsync(config.TokenEndpoint, formContent);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Token request failed: {response.StatusCode} - {errorContent}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException("Invalid token response received");
            }

            return tokenResponse;
        }

        public void ClearCache()
        {
            _cachedToken = null;
            _tokenExpiry = DateTime.MinValue;
        }
    }
}
