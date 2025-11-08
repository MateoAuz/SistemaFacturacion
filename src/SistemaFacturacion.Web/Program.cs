using SistemaFacturacion.Web.Components;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Infrastructure.Persistence;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Infrastructure.Repositories;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 🔧 CONFIGURACIÓN DE HTTPCLIENT (AGREGA ESTO)
builder.Services.AddHttpClient("LocalApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? builder.Configuration["Urls"]?.Split(';').First() ?? "https://localhost:7001/");
});

// También agrega HttpClient genérico con BaseAddress
builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return httpClientFactory.CreateClient("LocalApi");
});

builder.Services.AddControllers();

// 🔧 REPOSITORIOS
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

// 🔧 CONFIGURACIÓN JSON
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

// 💾 CONFIGURAR LA CONEXIÓN A POSTGRESQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 🔄 REDIRECCIÓN TEMPORAL: DE "/" A "/LOGIN"
app.MapGet("/", () => Results.Redirect("/login"));

// CONFIGURE THE HTTP REQUEST PIPELINE.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();