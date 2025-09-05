namespace PaginationTool.Shared.Models
{
    public class QueryResult
    {
        public List<object> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PagesProcessed { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FilePath { get; set; }
    }
}
