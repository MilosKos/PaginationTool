namespace PaginationTool.Shared.Models
{
    public class AuthenticationConfig
    {
        public string? BearerToken { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string TokenEndpoint { get; set; } = string.Empty;
    }
}
