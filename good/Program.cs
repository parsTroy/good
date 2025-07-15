using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using good;
using good.Services;
using LiveChartsCore.SkiaSharpView.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped<LocalStorageService>(sp =>
    new LocalStorageService(
        sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>(),
        sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LocalStorageService>>()
    )
);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();