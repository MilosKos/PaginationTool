using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PaginationTool.Web.Components;
using PaginationTool.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add HTTP client
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register custom services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<HttpService>();
builder.Services.AddScoped<JsonService>();
builder.Services.AddScoped<QueryService>();

await builder.Build().RunAsync();
