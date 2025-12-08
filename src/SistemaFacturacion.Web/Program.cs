using SistemaFacturacion.Web.Components;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Infrastructure.Persistence;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Infrastructure.Repositories;
using SistemaFacturacion.Application.Services;
using SistemaFacturacion.Web.Services; 
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Services;
using SistemaFacturacion.Domain.Configuration;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Polly;

using System.Globalization; 

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);


// CONFIGURACIÓN DE HTTPCLIENT
builder.Services.AddHttpClient("LocalApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? builder.Configuration["Urls"]?.Split(';').First() ?? "https://localhost:7001/");
});

builder.Services.AddHttpClient("SriSoap", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "text/xml");
})
.AddTransientHttpErrorPolicy(builder => 
    builder.WaitAndRetryAsync(
        retryCount: 3, 
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
    ));

builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return httpClientFactory.CreateClient("LocalApi");
});

builder.Services.AddControllers();
// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar las opciones del SRI
var sriConfig = new SriConfiguracion();
builder.Configuration.GetSection("SRI").Bind(sriConfig);
builder.Services.AddSingleton(sriConfig);

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<ILoteRepository, LoteRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IComprobanteElectronicoRepository, ComprobanteElectronicoRepository>();

builder.Services.AddScoped<ITaxCalculator, SistemaFacturacion.Application.Services.TaxCalculator>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IXmlValidationService, XmlValidationService>();
builder.Services.AddScoped<IValidacionFacturaService, ValidacionFacturaService>();

builder.Services.AddScoped<IStockService, SistemaFacturacion.Infrastructure.Services.StockService>();


builder.Services.AddScoped<IClaveAccesoService, ClaveAccesoService>();
builder.Services.AddScoped<IXmlGeneratorService, XmlGeneratorService>();

builder.Services.AddScoped<ISriApiService, SistemaFacturacion.Infrastructure.Services.SriApiService>();
builder.Services.AddScoped<IFacturacionElectronicaService, FacturacionElectronicaService>();

builder.Services.AddScoped<IRideGeneratorService, RideGeneratorService>();
builder.Services.AddScoped<IEmailService, EmailService>();
// En Program.cs, busca donde están los otros services y agrega:
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<SistemaFacturacion.Application.Contracts.IReporteService, SistemaFacturacion.Infrastructure.Services.ReporteService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

app.MapGet("/", () => Results.Redirect("/login"));

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
    
app.UseAntiforgery();

app.Use(async (context, next) =>
{
    await next();
    
    if (context.Response.StatusCode == 404 && 
        !context.Request.Path.StartsWithSegments("/_blazor") &&
        !context.Request.Path.StartsWithSegments("/_framework"))
    {
        context.Response.Redirect("/error-404");
    }
});

app.Run();
