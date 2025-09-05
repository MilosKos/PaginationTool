using System.Text.Json;
using System.Text.Encodings.Web;

namespace PaginationTool.Shared.Services
{
    public class JsonService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public async Task<string> SaveDataAsync(object data, string fileName)
        {
            var downloadsPath = GetDownloadsPath();
            var filePath = Path.Combine(downloadsPath, fileName);
            
            var jsonString = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonString);
            
            return filePath;
        }

        private static string GetDownloadsPath()
        {
            // Try to get the Downloads folder, fallback to user's home directory
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            
            if (!Directory.Exists(downloadsPath))
            {
                // Fallback to Documents folder if Downloads doesn't exist
                downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            // Create PaginationTool subdirectory
            var toolPath = Path.Combine(downloadsPath, "PaginationTool");
            if (!Directory.Exists(toolPath))
            {
                Directory.CreateDirectory(toolPath);
            }

            return toolPath;
        }

        public static string GetDefaultSavePath()
        {
            return GetDownloadsPath();
        }
    }
}
