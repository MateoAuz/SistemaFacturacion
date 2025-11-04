using SistemaFacturacion.Web.Components;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddScoped<SistemaFacturacion.Application.Contracts.IUsuarioRepository, SistemaFacturacion.Infrastructure.Repositories.UsuarioRepository>();

builder.Services.AddScoped<SistemaFacturacion.Application.Contracts.IClienteRepository,
                           SistemaFacturacion.Infrastructure.Repositories.ClienteRepository>();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 💾 Configurar la conexión a PostgreSQL (antes de builder.Build())
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

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
