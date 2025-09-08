namespace PaginationTool.Shared.Models
{
    public class ApiQueryConfig
    {
        public string ApiUrl { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public int PageSize { get; set; } = 100;
        public string PaginationTokenParameter { get; set; } = "nextToken";
        public AuthenticationConfig Authentication { get; set; } = new();
        
        // Sorting options
        public bool EnableSorting { get; set; } = false;
        public string SortField { get; set; } = string.Empty;
        public bool SortAscending { get; set; } = true;
        
        // File naming options
        public string CustomFilenameString { get; set; } = string.Empty;
    }
}
