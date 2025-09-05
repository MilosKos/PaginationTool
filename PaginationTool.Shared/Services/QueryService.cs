using PaginationTool.Shared.Models;
using System.Diagnostics;

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
                var currentUrl = _httpService.BuildUrlWithParameters(config.ApiUrl, config.PageSize);
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
                            var nextToken = _httpService.ExtractNextTokenFromUrl(pageResponse.NextLink);
                            
                            // Add the nextToken using BuildUrlWithParameters to ensure proper encoding
                            if (!string.IsNullOrEmpty(nextToken))
                            {
                                currentUrl = _httpService.BuildUrlWithParameters(config.ApiUrl, config.PageSize, nextToken);
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

                progress?.Report($"Data retrieval completed. Saving to file...");

                // Save data to file
                var fileName = $"api_data_{DateTime.Now:yyyyMMdd_HHmmss}.json";
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
                var testUrl = _httpService.BuildUrlWithParameters(config.ApiUrl, 1); // Request just 1 item for testing
                
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
    }
}
