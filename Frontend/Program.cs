using StarlinkSystem.Blazor.Components;
using StarlinkSystem.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<AuthSession>();
builder.Services.AddScoped<MockStore>();
builder.Services.AddScoped<MockStarlinkService>();
builder.Services.AddScoped<ApiStarlinkService>();
builder.Services.AddHttpClient("StarlinkApi", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001/";
    client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
    client.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddScoped<IStarlinkService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return config.GetValue<bool>("UseMockData")
        ? sp.GetRequiredService<MockStarlinkService>()
        : sp.GetRequiredService<ApiStarlinkService>();
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
