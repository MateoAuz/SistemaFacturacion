using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaFacturacion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    idcliente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    tipoidentificacion = table.Column<string>(type: "char(5)", nullable: false),
                    identificacion = table.Column<string>(type: "char(13)", nullable: false),
                    nombres = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    direccion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    telefono = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    correo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    estado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.idcliente);
                });

            migrationBuilder.CreateTable(
                name: "configuracionempresa",
                columns: table => new
                {
                    idconfiguracion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    razonsocial = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    nombrecomercial = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ruc = table.Column<string>(type: "char(13)", nullable: false),
                    direccionmatriz = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    puntoemision = table.Column<string>(type: "char(3)", nullable: true),
                    ambiente = table.Column<char>(type: "char(1)", nullable: false),
                    rutacertificado = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    clavecertificado = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    correoempresa = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracionempresa", x => x.idconfiguracion);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    idproducto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    categoria = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    preciounitario = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    precioventa = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    stockactual = table.Column<short>(type: "smallint", nullable: false),
                    fechaexpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.idproducto);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    idusuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nombreusuario = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    clavehash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    correo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    rol = table.Column<char>(type: "char(1)", nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.idusuario);
                });

            migrationBuilder.CreateTable(
                name: "facturas",
                columns: table => new
                {
                    idfactura = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    numerofactura = table.Column<string>(type: "char(17)", nullable: false),
                    idcliente = table.Column<int>(type: "integer", nullable: false),
                    idusuario = table.Column<int>(type: "integer", nullable: false),
                    fechaemision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false, defaultValue: "PENDIENTE"),
                    ClienteIdCliente = table.Column<int>(type: "integer", nullable: true),
                    UsuarioIdUsuario = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas", x => x.idfactura);
                    table.ForeignKey(
                        name: "FK_facturas_clientes_ClienteIdCliente",
                        column: x => x.ClienteIdCliente,
                        principalTable: "clientes",
                        principalColumn: "idcliente");
                    table.ForeignKey(
                        name: "FK_facturas_clientes_idcliente",
                        column: x => x.idcliente,
                        principalTable: "clientes",
                        principalColumn: "idcliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_facturas_usuarios_UsuarioIdUsuario",
                        column: x => x.UsuarioIdUsuario,
                        principalTable: "usuarios",
                        principalColumn: "idusuario");
                    table.ForeignKey(
                        name: "FK_facturas_usuarios_idusuario",
                        column: x => x.idusuario,
                        principalTable: "usuarios",
                        principalColumn: "idusuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "historialprecios",
                columns: table => new
                {
                    idhistorial = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    idproducto = table.Column<int>(type: "integer", nullable: false),
                    precioanterior = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    precionuevo = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    fechacambio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    idusuario = table.Column<int>(type: "integer", nullable: true),
                    motivo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProductoIdProducto = table.Column<int>(type: "integer", nullable: true),
                    UsuarioIdUsuario = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historialprecios", x => x.idhistorial);
                    table.ForeignKey(
                        name: "FK_historialprecios_productos_ProductoIdProducto",
                        column: x => x.ProductoIdProducto,
                        principalTable: "productos",
                        principalColumn: "idproducto");
                    table.ForeignKey(
                        name: "FK_historialprecios_productos_idproducto",
                        column: x => x.idproducto,
                        principalTable: "productos",
                        principalColumn: "idproducto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_historialprecios_usuarios_UsuarioIdUsuario",
                        column: x => x.UsuarioIdUsuario,
                        principalTable: "usuarios",
                        principalColumn: "idusuario");
                    table.ForeignKey(
                        name: "FK_historialprecios_usuarios_idusuario",
                        column: x => x.idusuario,
                        principalTable: "usuarios",
                        principalColumn: "idusuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "comprobanteselectronicos",
                columns: table => new
                {
                    idcomprobante = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    idfactura = table.Column<int>(type: "integer", nullable: false),
                    claveacceso = table.Column<string>(type: "char(49)", nullable: true),
                    xmlgenerado = table.Column<string>(type: "text", nullable: true),
                    xmlfirmado = table.Column<string>(type: "text", nullable: true),
                    estadoenvio = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "NO_ENVIADO"),
                    mensajerespuesta = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    fechaenvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fechaautorizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    numeroautorizacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comprobanteselectronicos", x => x.idcomprobante);
                    table.ForeignKey(
                        name: "FK_comprobanteselectronicos_facturas_idfactura",
                        column: x => x.idfactura,
                        principalTable: "facturas",
                        principalColumn: "idfactura",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detallefactura",
                columns: table => new
                {
                    iddetalle = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    idfactura = table.Column<int>(type: "integer", nullable: false),
                    idproducto = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<short>(type: "smallint", nullable: false),
                    preciounitario = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    totallinea = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    FacturaIdFactura = table.Column<int>(type: "integer", nullable: true),
                    ProductoIdProducto = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detallefactura", x => x.iddetalle);
                    table.ForeignKey(
                        name: "FK_detallefactura_facturas_FacturaIdFactura",
                        column: x => x.FacturaIdFactura,
                        principalTable: "facturas",
                        principalColumn: "idfactura");
                    table.ForeignKey(
                        name: "FK_detallefactura_facturas_idfactura",
                        column: x => x.idfactura,
                        principalTable: "facturas",
                        principalColumn: "idfactura",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detallefactura_productos_ProductoIdProducto",
                        column: x => x.ProductoIdProducto,
                        principalTable: "productos",
                        principalColumn: "idproducto");
                    table.ForeignKey(
                        name: "FK_detallefactura_productos_idproducto",
                        column: x => x.idproducto,
                        principalTable: "productos",
                        principalColumn: "idproducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_clientes_identificacion",
                table: "clientes",
                column: "identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_comprobantes_clave",
                table: "comprobanteselectronicos",
                column: "claveacceso");

            migrationBuilder.CreateIndex(
                name: "IX_comprobanteselectronicos_idfactura",
                table: "comprobanteselectronicos",
                column: "idfactura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_configuracionempresa_ruc",
                table: "configuracionempresa",
                column: "ruc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_detallefactura_FacturaIdFactura",
                table: "detallefactura",
                column: "FacturaIdFactura");

            migrationBuilder.CreateIndex(
                name: "IX_detallefactura_idfactura",
                table: "detallefactura",
                column: "idfactura");

            migrationBuilder.CreateIndex(
                name: "IX_detallefactura_idproducto",
                table: "detallefactura",
                column: "idproducto");

            migrationBuilder.CreateIndex(
                name: "IX_detallefactura_ProductoIdProducto",
                table: "detallefactura",
                column: "ProductoIdProducto");

            migrationBuilder.CreateIndex(
                name: "idx_facturas_numero",
                table: "facturas",
                column: "numerofactura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_facturas_ClienteIdCliente",
                table: "facturas",
                column: "ClienteIdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_idcliente",
                table: "facturas",
                column: "idcliente");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_idusuario",
                table: "facturas",
                column: "idusuario");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_UsuarioIdUsuario",
                table: "facturas",
                column: "UsuarioIdUsuario");

            migrationBuilder.CreateIndex(
                name: "idx_historial_productos",
                table: "historialprecios",
                column: "idproducto");

            migrationBuilder.CreateIndex(
                name: "IX_historialprecios_idusuario",
                table: "historialprecios",
                column: "idusuario");

            migrationBuilder.CreateIndex(
                name: "IX_historialprecios_ProductoIdProducto",
                table: "historialprecios",
                column: "ProductoIdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_historialprecios_UsuarioIdUsuario",
                table: "historialprecios",
                column: "UsuarioIdUsuario");

            migrationBuilder.CreateIndex(
                name: "idx_productos_codigo",
                table: "productos",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comprobanteselectronicos");

            migrationBuilder.DropTable(
                name: "configuracionempresa");

            migrationBuilder.DropTable(
                name: "detallefactura");

            migrationBuilder.DropTable(
                name: "historialprecios");

            migrationBuilder.DropTable(
                name: "facturas");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
