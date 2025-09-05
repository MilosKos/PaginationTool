namespace PaginationTool.Shared.Contexts
{
    internal class TokenContext
    {
        public string? AccessToken { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
    }
}
