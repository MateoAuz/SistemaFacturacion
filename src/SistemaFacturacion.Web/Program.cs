using SistemaFacturacion.Web.Components;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Infrastructure.Persistence;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Infrastructure.Repositories; // <<-- AÑADE ESTO
using SistemaFacturacion.Application.Services;     // <<-- AÑADE ESTO
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


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
// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// 🔧 REPOSITORIOS
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<ILoteRepository, LoteRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IComprobanteElectronicoRepository, ComprobanteElectronicoRepository>();

// 🔧 SERVICIOS DE APLICACIÓN (Lógica Pura)
builder.Services.AddScoped<ITaxCalculator, SistemaFacturacion.Application.Services.TaxCalculator>();
builder.Services.AddScoped<IPagoService, PagoService>();

// 🔧 SERVICIOS DE INFRAESTRUCTURA (Conexión a BD)
builder.Services.AddScoped<IStockService, SistemaFacturacion.Infrastructure.Services.StockService>();
// ==========================================
// SERVICIOS DE FACTURACIÓN ELECTRÓNICA (NUEVOS)
// ==========================================
builder.Services.AddScoped<IClaveAccesoService, ClaveAccesoService>(); // ✅ NUEVO
builder.Services.AddScoped<IXmlGeneratorService, XmlGeneratorService>();

// 🔧 CONFIGURACIÓN JSON
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 💾 CONFIGURAR LA CONEXIÓN A POSTGRESQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });


var app = builder.Build();

// 🔄 REDIRECCIÓN TEMPORAL: DE "/" A "/LOGIN"
app.MapGet("/", () => Results.Redirect("/login"));

// CONFIGURE THE HTTP REQUEST PIPELINE.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
// --- Middleware Swagger ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ ORDEN CORRECTO: Primero mapear componentes, luego el manejo de 404
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
    
app.UseAntiforgery();

// ✅ MIDDLEWARE PARA RUTAS NO MANEJADAS - DEBE IR AL FINAL
app.Use(async (context, next) =>
{
    await next();
    
    // Si después de procesar la request sigue siendo 404 y no es una ruta de Blazor
    if (context.Response.StatusCode == 404 && 
        !context.Request.Path.StartsWithSegments("/_blazor") &&
        !context.Request.Path.StartsWithSegments("/_framework"))
    {
        // Redirigir a la página de error 404 de Blazor
        context.Response.Redirect("/error-404");
    }
});

app.Run();