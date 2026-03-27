using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var apiBaseUrl = builder.Configuration["ApiClient:BaseUrl"] ?? builder.HostEnvironment.BaseAddress;
var xBlocksKey = builder.Configuration["ApiClient:XBlocksKey"];

// Default HttpClient for same-host app/API calls with x-blocks-key header.
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
    
    if (!string.IsNullOrWhiteSpace(xBlocksKey))
    {
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-blocks-key", xBlocksKey);
    }
    
    return httpClient;
});

// Named client for cross-service API calls that require x-blocks-key.
builder.Services.AddHttpClient("BlocksExternalApi", httpClient =>
{
    httpClient.BaseAddress = new Uri(apiBaseUrl);

    if (!string.IsNullOrWhiteSpace(xBlocksKey))
    {
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-blocks-key", xBlocksKey);
    }
});


await builder.Build().RunAsync();
