using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<DetalleFactura> DetallesFactura => Set<DetalleFactura>();
    public DbSet<ComprobanteElectronico> ComprobantesElectronicos => Set<ComprobanteElectronico>();
    public DbSet<ConfiguracionEmpresa> ConfiguracionEmpresa => Set<ConfiguracionEmpresa>();
    public DbSet<HistorialPrecio> HistorialPrecios => Set<HistorialPrecio>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Aquí podrás personalizar relaciones o restricciones
    }
}
