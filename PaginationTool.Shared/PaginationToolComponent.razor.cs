using Microsoft.AspNetCore.Components;
using PaginationTool.Shared.Models;
using PaginationTool.Shared.Services;

namespace PaginationTool.Shared
{
    public partial class PaginationToolComponent : ComponentBase
    {
        [Inject] private QueryService QueryService { get; set; } = null!;

        private ApiQueryConfig Config { get; set; } = new();
        private bool UseDirectToken { get; set; } = true;
        private bool IsProcessing { get; set; } = false;
        private string StatusMessage { get; set; } = string.Empty;
        private QueryResult? LastResult { get; set; }
        private CancellationTokenSource? _cancellationTokenSource;

        private void SetAuthType(bool useDirectToken)
        {
            UseDirectToken = useDirectToken;
            
            // Clear the other authentication method's data
            if (useDirectToken)
            {
                Config.Authentication.ClientId = string.Empty;
                Config.Authentication.ClientSecret = string.Empty;
                Config.Authentication.TokenEndpoint = string.Empty;
            }
            else
            {
                Config.Authentication.BearerToken = string.Empty;
            }

            StateHasChanged();
        }

        private async Task TestConnection()
        {
            if (!ValidateConfiguration())
                return;

            IsProcessing = true;
            StatusMessage = "Testing connection...";
            LastResult = null;
            StateHasChanged();

            try
            {
                var result = await QueryService.TestConnectionAsync(Config);
                LastResult = result;
                
                if (result.Success)
                {
                    StatusMessage = $"Connection successful! Retrieved {result.TotalRecords} sample record(s) in {result.Duration.TotalSeconds:F2}s";
                }
                else
                {
                    StatusMessage = "Connection test failed. Check the error details below.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Connection test failed with an exception.";
                LastResult = new QueryResult 
                { 
                    Success = false, 
                    ErrorMessage = ex.Message,
                    Duration = TimeSpan.Zero
                };
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        private async Task StartQuery()
        {
            if (!ValidateConfiguration())
                return;

            IsProcessing = true;
            StatusMessage = "Initializing query...";
            LastResult = null;
            _cancellationTokenSource = new CancellationTokenSource();
            StateHasChanged();

            try
            {
                var progress = new Progress<string>(message =>
                {
                    StatusMessage = message;
                    InvokeAsync(StateHasChanged);
                });

                var result = await QueryService.QueryAllPagesAsync(Config, progress, _cancellationTokenSource.Token);
                LastResult = result;

                if (result.Success)
                {
                    StatusMessage = "Query completed successfully!";
                }
                else
                {
                    StatusMessage = "Query failed. Check the error details below.";
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Query was cancelled by user.";
                LastResult = new QueryResult 
                { 
                    Success = false, 
                    ErrorMessage = "Operation was cancelled by user",
                    Duration = TimeSpan.Zero
                };
            }
            catch (Exception ex)
            {
                StatusMessage = "Query failed with an exception.";
                LastResult = new QueryResult 
                { 
                    Success = false, 
                    ErrorMessage = ex.Message,
                    Duration = TimeSpan.Zero
                };
            }
            finally
            {
                IsProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                StateHasChanged();
            }
        }

        private void CancelQuery()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancelling query...";
            StateHasChanged();
        }

        private bool ValidateConfiguration()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Config.ApiUrl))
                errors.Add("API URL is required");

            if (string.IsNullOrWhiteSpace(Config.TenantId))
                errors.Add("Tenant ID is required");

            if (UseDirectToken)
            {
                if (string.IsNullOrWhiteSpace(Config.Authentication.BearerToken))
                    errors.Add("Bearer Token is required");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Config.Authentication.TokenEndpoint))
                    errors.Add("Token Endpoint is required");
                if (string.IsNullOrWhiteSpace(Config.Authentication.ClientId))
                    errors.Add("Client ID is required");
                if (string.IsNullOrWhiteSpace(Config.Authentication.ClientSecret))
                    errors.Add("Client Secret is required");
            }

            if (errors.Any())
            {
                StatusMessage = "Validation failed: " + string.Join(", ", errors);
                LastResult = new QueryResult 
                { 
                    Success = false, 
                    ErrorMessage = string.Join(", ", errors),
                    Duration = TimeSpan.Zero
                };
                StateHasChanged();
                return false;
            }

            return true;
        }

        protected override void OnInitialized()
        {
            // Set default values
            Config.PageSize = 100;
            base.OnInitialized();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}
