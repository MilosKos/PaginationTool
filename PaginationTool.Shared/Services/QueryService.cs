using PaginationTool.Shared.Models;
using System.Diagnostics;
using System.Text.Json;

namespace PaginationTool.Shared.Services
{
    public class QueryService
    {
        private readonly HttpService _httpService;
        private readonly JsonService _jsonService;

        public QueryService(HttpService httpService, JsonService jsonService)
        {
            _httpService = httpService;
            _jsonService = jsonService;
        }

        public async Task<QueryResult> QueryAllPagesAsync(
            ApiQueryConfig config, 
            IProgress<string>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var result = new QueryResult { Success = false };
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                progress?.Report("Starting data retrieval...");
                
                var allData = new List<object>();
                var currentUrl = _httpService.BuildUrlWithParameters(config.ApiUrl, config.PageSize, null, config.PaginationTokenParameter);
                var pageCount = 0;

                while (!string.IsNullOrEmpty(currentUrl) && !cancellationToken.IsCancellationRequested)
                {
                    pageCount++;
                    progress?.Report($"Processing page {pageCount}...");

                    var pageResponse = await _httpService.GetPaginatedDataAsync(
                        currentUrl, 
                        config.TenantId, 
                        config.Authentication);

                    if (pageResponse.Value?.Count > 0)
                    {
                        allData.AddRange(pageResponse.Value);
                        progress?.Report($"Retrieved {pageResponse.Value.Count} records from page {pageCount}. Total: {allData.Count}");
                    }

                    // Check for next page
                    if (!string.IsNullOrEmpty(pageResponse.NextLink))
                    {
                        if (pageResponse.NextLink.StartsWith("/"))
                        {
                            // Relative URL - build from base URL preserving subdomain and other components
                            var baseUri = new Uri(config.ApiUrl);
                            var nextToken = _httpService.ExtractNextTokenFromUrl(pageResponse.NextLink, config.PaginationTokenParameter);
                            
                            // Add the nextToken using BuildUrlWithParameters to ensure proper encoding
                            if (!string.IsNullOrEmpty(nextToken))
                            {
                                currentUrl = _httpService.BuildUrlWithParameters(config.ApiUrl, config.PageSize, nextToken, config.PaginationTokenParameter);
                            }
                        }
                        else
                        {
                            // Absolute URL - use as is
                            currentUrl = pageResponse.NextLink;
                        }
                    }
                    else
                    {
                        currentUrl = null; // No more pages
                    }

                    // Small delay to avoid overwhelming the API
                    await Task.Delay(100, cancellationToken);
                }

                stopwatch.Stop();

                if (cancellationToken.IsCancellationRequested)
                {
                    progress?.Report("Operation was cancelled.");
                    result.ErrorMessage = "Operation was cancelled by user";
                    return result;
                }

                progress?.Report($"Data retrieval completed. Processing data...");

                // Apply sorting if enabled
                if (config.EnableSorting && !string.IsNullOrWhiteSpace(config.SortField))
                {
                    try
                    {
                        progress?.Report($"Sorting data by '{config.SortField}' ({(config.SortAscending ? "ascending" : "descending")})...");
                        allData = SortData(allData, config.SortField, config.SortAscending);
                        progress?.Report($"Data sorted successfully.");
                    }
                    catch (Exception ex)
                    {
                        progress?.Report($"Warning: Could not sort data by '{config.SortField}': {ex.Message}");
                    }
                }

                progress?.Report($"Saving to file...");

                // Save data to file
                var fileName = GenerateFileName(config);
                var filePath = await _jsonService.SaveDataAsync(allData, fileName);

                result.Success = true;
                result.Data = allData;
                result.TotalRecords = allData.Count;
                result.PagesProcessed = pageCount;
                result.Duration = stopwatch.Elapsed;
                result.FilePath = filePath;

                progress?.Report($"Successfully retrieved {allData.Count} records from {pageCount} pages in {stopwatch.Elapsed.TotalSeconds:F2} seconds. Saved to: {fileName}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ErrorMessage = ex.Message;
                result.Duration = stopwatch.Elapsed;
                progress?.Report($"Error occurred: {ex.Message}");
            }

            return result;
        }

        public async Task<QueryResult> TestConnectionAsync(ApiQueryConfig config)
        {
            var result = new QueryResult { Success = false };
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var testUrl = _httpService.BuildUrlWithParameters(config.ApiUrl, 1, null, config.PaginationTokenParameter); // Request just 1 item for testing
                
                var response = await _httpService.GetPaginatedDataAsync(
                    testUrl, 
                    config.TenantId, 
                    config.Authentication);

                result.Success = true;
                result.TotalRecords = response.Value?.Count ?? 0;
                result.PagesProcessed = 1;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
            }

            return result;
        }

        private string GenerateFileName(ApiQueryConfig config)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            // Sanitize the custom filename string to be safe for filenames
            var customPart = string.Empty;
            if (!string.IsNullOrWhiteSpace(config.CustomFilenameString))
            {
                // Remove invalid filename characters and trim
                var sanitized = config.CustomFilenameString.Trim();
                var invalidChars = Path.GetInvalidFileNameChars();
                foreach (var invalidChar in invalidChars)
                {
                    sanitized = sanitized.Replace(invalidChar, '_');
                }
                
                // Replace spaces with underscores and remove multiple consecutive underscores
                sanitized = sanitized.Replace(' ', '_');
                while (sanitized.Contains("__"))
                {
                    sanitized = sanitized.Replace("__", "_");
                }
                
                // Trim underscores from beginning and end
                sanitized = sanitized.Trim('_');
                
                if (!string.IsNullOrWhiteSpace(sanitized))
                {
                    customPart = sanitized;
                }
            }
            
            // New format: [custom]_[tenant]_[timestamp] or api_data_[tenant]_[timestamp] if no custom name
            var prefix = !string.IsNullOrWhiteSpace(customPart) ? customPart : "api_data";
            return $"{prefix}_{config.TenantId}_{timestamp}.json";
        }

        private List<object> SortData(List<object> data, string sortField, bool ascending)
        {
            if (data.Count == 0)
                return data;

            try
            {
                var sortedData = data.OrderBy(item =>
                {
                    if (item == null)
                        return null;

                    // Handle JsonElement (common when deserializing to object)
                    if (item is JsonElement jsonElement)
                    {
                        if (jsonElement.TryGetProperty(sortField, out var property))
                        {
                            return property.ValueKind switch
                            {
                                JsonValueKind.String => property.GetString(),
                                JsonValueKind.Number => property.GetDouble().ToString("000000000000.000000"),
                                JsonValueKind.True => "1",
                                JsonValueKind.False => "0",
                                JsonValueKind.Null => null,
                                _ => property.ToString()
                            };
                        }
                    }
                    // Handle Dictionary<string, object> (alternative deserialization format)
                    else if (item is Dictionary<string, object> dict)
                    {
                        if (dict.TryGetValue(sortField, out var value))
                        {
                            return value?.ToString();
                        }
                    }
                    // Handle dynamic objects or other types using reflection
                    else
                    {
                        var property = item.GetType().GetProperty(sortField, 
                            System.Reflection.BindingFlags.IgnoreCase | 
                            System.Reflection.BindingFlags.Public | 
                            System.Reflection.BindingFlags.Instance);
                        
                        if (property != null)
                        {
                            var value = property.GetValue(item);
                            return value?.ToString();
                        }
                    }

                    return null;
                }).AsEnumerable();

                if (!ascending)
                {
                    sortedData = sortedData.Reverse();
                }

                return sortedData.ToList();
            }
            catch (Exception)
            {
                // If sorting fails, return original data
                return data;
            }
        }
    }
}
