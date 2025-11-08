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

        // USUARIOS
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.IdUsuario).HasColumnName("idusuario").UseIdentityAlwaysColumn();
            e.Property(x => x.NombreUsuario).HasColumnName("nombreusuario").HasMaxLength(40).IsRequired();
            e.Property(x => x.ClaveHash).HasColumnName("clavehash").HasMaxLength(200).IsRequired();
            e.Property(x => x.Correo).HasColumnName("correo").HasMaxLength(80);
            e.Property(x => x.Rol).HasColumnName("rol").HasColumnType("char(1)").IsRequired();
            e.Property(x => x.Estado).HasColumnName("estado").HasDefaultValue(true);
        });

        // CLIENTES
        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("clientes");
            e.HasKey(x => x.IdCliente);
            e.Property(x => x.IdCliente).HasColumnName("idcliente").UseIdentityAlwaysColumn();
            e.Property(x => x.TipoIdentificacion).HasColumnName("tipoidentificacion").HasColumnType("char(5)");
            e.Property(x => x.Identificacion).HasColumnName("identificacion").HasColumnType("char(13)").IsRequired();
            e.HasIndex(x => x.Identificacion).IsUnique().HasDatabaseName("idx_clientes_identificacion");
            e.Property(x => x.Nombres).HasColumnName("nombres").HasMaxLength(60).IsRequired();
            e.Property(x => x.Apellidos).HasColumnName("apellidos").HasMaxLength(60);
            e.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(120);
            e.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(15);
            e.Property(x => x.Correo).HasColumnName("correo").HasMaxLength(80);
            e.Property(x => x.Estado).HasColumnName("estado").HasDefaultValue(true);
        });

        // PRODUCTOS
        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("productos");
            e.HasKey(x => x.IdProducto);
            e.Property(x => x.IdProducto).HasColumnName("idproducto").UseIdentityAlwaysColumn();
            e.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Codigo).IsUnique().HasDatabaseName("idx_productos_codigo");
            e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(80).IsRequired();
            e.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(40);
            e.Property(x => x.PrecioUnitario).HasColumnName("preciounitario").HasColumnType("numeric(12,2)").IsRequired();
            e.Property(x => x.PrecioVenta).HasColumnName("precioventa").HasColumnType("numeric(12,2)").IsRequired();
            e.Property(x => x.StockActual).HasColumnName("stockactual");
            e.Property(x => x.FechaExpiracion).HasColumnName("fechaexpiracion");
            e.Property(x => x.Estado).HasColumnName("estado").HasDefaultValue(true);
        });

        // FACTURAS (principal)
        modelBuilder.Entity<Factura>(e =>
        {
            e.ToTable("facturas");
            e.HasKey(x => x.IdFactura);
            e.Property(x => x.IdFactura).HasColumnName("idfactura").UseIdentityAlwaysColumn();
            e.Property(x => x.NumeroFactura).HasColumnName("numerofactura").HasColumnType("char(17)").IsRequired();
            e.HasIndex(x => x.NumeroFactura).IsUnique().HasDatabaseName("idx_facturas_numero");
            e.Property(x => x.IdCliente).HasColumnName("idcliente");
            e.Property(x => x.IdUsuario).HasColumnName("idusuario");
            e.Property(x => x.FechaEmision).HasColumnName("fechaemision").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(12,2)").IsRequired();
            e.Property(x => x.Iva).HasColumnName("iva").HasColumnType("numeric(12,2)").IsRequired();
            e.Property(x => x.Total).HasColumnName("total").HasColumnType("numeric(12,2)").IsRequired();
            e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(12).HasDefaultValue("PENDIENTE");

            e.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.SetNull);

            // 1–1 Factura ↔ Comprobante (dependiente: Comprobante)
            e.HasOne(x => x.Comprobante)
            .WithOne(c => c.Factura)
            .HasForeignKey<ComprobanteElectronico>(c => c.IdFactura)
            .OnDelete(DeleteBehavior.Cascade);
        });

        // DETALLE FACTURA
        modelBuilder.Entity<DetalleFactura>(e =>
        {
            e.ToTable("detallefactura");
            e.HasKey(x => x.IdDetalle);
            e.Property(x => x.IdDetalle).HasColumnName("iddetalle").UseIdentityAlwaysColumn();
            e.Property(x => x.IdFactura).HasColumnName("idfactura");
            e.Property(x => x.IdProducto).HasColumnName("idproducto");
            e.Property(x => x.Cantidad).HasColumnName("cantidad");
            e.Property(x => x.PrecioUnitario).HasColumnName("preciounitario").HasColumnType("numeric(12,2)");
            e.Property(x => x.TotalLinea)
                .HasColumnName("totallinea")
                .HasColumnType("numeric(12,2)")
                .HasComputedColumnSql("(cantidad * preciounitario)", stored: true);

            e.HasOne(x => x.Factura).WithMany(f => f.Detalles).HasForeignKey(x => x.IdFactura).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.IdProducto).OnDelete(DeleteBehavior.Restrict);
        });

        // COMPROBANTES (dependiente 1–1)
        modelBuilder.Entity<ComprobanteElectronico>(e =>
        {
            e.ToTable("comprobanteselectronicos");
            e.HasKey(x => x.IdComprobante);
            e.Property(x => x.IdComprobante).HasColumnName("idcomprobante").UseIdentityAlwaysColumn();
            e.Property(x => x.IdFactura).HasColumnName("idfactura");
            e.Property(x => x.ClaveAcceso).HasColumnName("claveacceso").HasColumnType("char(49)");
            e.HasIndex(x => x.ClaveAcceso).HasDatabaseName("idx_comprobantes_clave");
            e.Property(x => x.XmlGenerado).HasColumnName("xmlgenerado").HasColumnType("text");
            e.Property(x => x.XmlFirmado).HasColumnName("xmlfirmado").HasColumnType("text");
            e.Property(x => x.EstadoEnvio).HasColumnName("estadoenvio").HasMaxLength(15).HasDefaultValue("NO_ENVIADO");
            e.Property(x => x.MensajeRespuesta).HasColumnName("mensajerespuesta").HasMaxLength(250);
            e.Property(x => x.FechaEnvio).HasColumnName("fechaenvio");
            e.Property(x => x.FechaAutorizacion).HasColumnName("fechaautorizacion");
            e.Property(x => x.NumeroAutorizacion).HasColumnName("numeroautorizacion").HasMaxLength(50);
        });

        // CONFIGURACION EMPRESA
        modelBuilder.Entity<ConfiguracionEmpresa>(e =>
        {
            e.ToTable("configuracionempresa");
            e.HasKey(x => x.IdConfiguracion);
            e.Property(x => x.IdConfiguracion).HasColumnName("idconfiguracion").UseIdentityAlwaysColumn();
            e.Property(x => x.RazonSocial).HasColumnName("razonsocial").HasMaxLength(80).IsRequired();
            e.Property(x => x.NombreComercial).HasColumnName("nombrecomercial").HasMaxLength(80);
            e.Property(x => x.Ruc).HasColumnName("ruc").HasColumnType("char(13)").IsRequired();
            e.HasIndex(x => x.Ruc).IsUnique();
            e.Property(x => x.DireccionMatriz).HasColumnName("direccionmatriz").HasMaxLength(120);
            e.Property(x => x.PuntoEmision).HasColumnName("puntoemision").HasColumnType("char(3)");
            e.Property(x => x.Ambiente).HasColumnName("ambiente").HasColumnType("char(1)");
            e.Property(x => x.RutaCertificado).HasColumnName("rutacertificado").HasMaxLength(150);
            e.Property(x => x.ClaveCertificado).HasColumnName("clavecertificado").HasMaxLength(80);
            e.Property(x => x.CorreoEmpresa).HasColumnName("correoempresa").HasMaxLength(80);
        });

        // HISTORIAL PRECIOS
        modelBuilder.Entity<HistorialPrecio>(e =>
        {
            e.ToTable("historialprecios");
            e.HasKey(x => x.IdHistorial);
            e.Property(x => x.IdHistorial).HasColumnName("idhistorial").UseIdentityAlwaysColumn();
            e.Property(x => x.IdProducto).HasColumnName("idproducto");
            e.HasIndex(x => x.IdProducto).HasDatabaseName("idx_historial_productos");
            e.Property(x => x.PrecioAnterior).HasColumnName("precioanterior").HasColumnType("numeric(12,2)");
            e.Property(x => x.PrecioNuevo).HasColumnName("precionuevo").HasColumnType("numeric(12,2)");
            e.Property(x => x.FechaCambio).HasColumnName("fechacambio").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.IdUsuario).HasColumnName("idusuario");
            e.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(150);

            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.IdProducto).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.SetNull);
        });
    }

}
