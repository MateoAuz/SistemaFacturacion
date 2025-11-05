using SistemaFacturacion.Web.Components;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Infrastructure.Persistence;
using SistemaFacturacion.Application.Contracts;  // ✅ AGREGAR ESTE
using SistemaFacturacion.Infrastructure.Repositories;  // ✅ AGREGAR ESTE

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddScoped<SistemaFacturacion.Application.Contracts.IUsuarioRepository, 
                           SistemaFacturacion.Infrastructure.Repositories.UsuarioRepository>();
builder.Services.AddScoped<SistemaFacturacion.Application.Contracts.IClienteRepository,
                           SistemaFacturacion.Infrastructure.Repositories.ClienteRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 💾 Configurar la conexión a PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 🔄 REDIRECCIÓN TEMPORAL: De "/" a "/login"
app.MapGet("/", () => Results.Redirect("/login"));

// Configure the HTTP request pipeline.
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