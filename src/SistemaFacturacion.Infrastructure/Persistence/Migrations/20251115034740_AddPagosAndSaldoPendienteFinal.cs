using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaFacturacion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPagosAndSaldoPendienteFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "saldopendiente",
                table: "facturas",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    idpago = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    idfactura = table.Column<int>(type: "integer", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    metodopago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fechapago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    idusuario = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "REGISTRADO")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.idpago);
                    table.ForeignKey(
                        name: "FK_pagos_facturas_idfactura",
                        column: x => x.idfactura,
                        principalTable: "facturas",
                        principalColumn: "idfactura",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pagos_usuarios_idusuario",
                        column: x => x.idusuario,
                        principalTable: "usuarios",
                        principalColumn: "idusuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_pagos_factura",
                table: "pagos",
                column: "idfactura");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_idusuario",
                table: "pagos",
                column: "idusuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropColumn(
                name: "saldopendiente",
                table: "facturas");
        }
    }
}
