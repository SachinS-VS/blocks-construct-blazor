// using Blocks.Genesis;
// using Server.Components.Layout;
// using Server.Extensions;

// const string serviceName = "blocks-construct-server";
// // TODO: Uncomment Genesis bootstrap when ready
// // await ApplicationConfigurations.ConfigureLogAndSecretsAsync(serviceName, VaultType.Azure);

// var builder = WebApplication.CreateBuilder(args);

// // TODO: Uncomment Genesis configuration when ready
// // ApplicationConfigurations.ConfigureApiEnv(builder, args);

// var services = builder.Services;

// var apiBaseUrl = builder.Configuration["ApiClient:BaseUrl"] ?? "https://localhost:7075";
// var xBlocksKey = builder.Configuration["ApiClient:XBlocksKey"];

// var messageConfiguration = new MessageConfiguration
// {

// };

// // TODO: Uncomment Genesis service configuration when ready
// // ApplicationConfigurations.ConfigureServices(services, messageConfiguration);
// // ApplicationConfigurations.ConfigureApi(services);

// services.AddRazorComponents()
//     .AddInteractiveServerComponents()
//     .AddInteractiveWebAssemblyComponents();

// services.AddHttpClient();

// services.AddHttpContextAccessor();

// // Default HttpClient for same-host API calls during SSR.
// services.AddScoped(sp =>
// {
//     var request = sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request;
//     var baseAddress = request is null
//         ? "https://localhost:7075/"
//         : $"{request.Scheme}://{request.Host}/";

//     return new HttpClient
//     {
//         BaseAddress = new Uri(baseAddress)
//     };
// });

// // Named client for cross-service calls requiring x-blocks-key.
// services.AddHttpClient("BlocksExternalApi", httpClient =>
// {
//     httpClient.BaseAddress = new Uri(apiBaseUrl);

//     if (!string.IsNullOrWhiteSpace(xBlocksKey))
//     {
//         httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-blocks-key", xBlocksKey);
//     }
// });

// services.AddControllers();

// services.AddApplicationServices(builder.Environment.WebRootPath);

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseWebAssemblyDebugging();
// }
// else
// {
//     app.UseExceptionHandler("/Error", createScopeForErrors: true);
//     app.UseHsts();
// }

// app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
// app.UseHttpsRedirection();
// app.UseAntiforgery();

// // Configure middleware pipeline (adapted from Genesis for Blazor Server + API)
// app.UseCors(corsPolicyBuilder =>
//     corsPolicyBuilder
//         .AllowAnyHeader()
//         .AllowAnyMethod()
//         .SetIsOriginAllowed(_ => true)
//         .AllowCredentials()
//         .SetPreflightMaxAge(TimeSpan.FromDays(365)));

// // Health checks endpoint
// app.UseHealthChecks("/ping", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
// {
//     Predicate = _ => true,
//     ResponseWriter = async (context, _) =>
//     {
//         context.Response.ContentType = "application/json";
//         await context.Response.WriteAsJsonAsync(new { message = "pong from blocks-construct-server" });
//     }
// });

// // Swagger (mirrors Genesis ConfigureMiddleware pattern)
// // if (app.Configuration.GetSection("SwaggerOptions").Exists())
// // {
// //     app.UseSwagger();
// //     app.UseSwaggerUI();
// // }

// app.UseRouting();

// // TODO: Uncomment Genesis middleware once auth is properly scoped
// // Apply Genesis middleware only for /api/* routes — pages and static files bypass this
// // app.UseWhen(
// //     context => context.Request.Path.StartsWithSegments("/api"),
// //     apiApp =>
// //     {
// //         apiApp.UseMiddleware<TenantValidationMiddleware>();
// //         apiApp.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// //         apiApp.UseAuthentication();
// //         apiApp.UseAuthorization();
// //     });

// app.MapControllers();
// app.MapStaticAssets();
// app.MapRazorComponents<App>()
//     .AddInteractiveServerRenderMode()
//     .AddInteractiveWebAssemblyRenderMode()
//     .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

// await app.RunAsync();
