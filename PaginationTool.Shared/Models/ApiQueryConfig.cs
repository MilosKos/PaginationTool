namespace PaginationTool.Shared.Models
{
    public class ApiQueryConfig
    {
        public string ApiUrl { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public int PageSize { get; set; } = 100;
        public AuthenticationConfig Authentication { get; set; } = new();
    }
}
