/* src/SistemaFacturacion.Application/DTOs/ReportesDtos.cs */
using System;

namespace SistemaFacturacion.Application.DTOs
{
    public class ReporteVentasDto
    {
        public string Periodo { get; set; } = string.Empty; // "2025-01-01", "2025-01", "2025"
        public int CantidadFacturas { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
    }

    public class ReporteProductoDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalGenerado { get; set; }
        public decimal PrecioPromedio { get; set; } // Nuevo
    }

    public class ReporteAuditoriaVentaDto
    {
        public DateTime Fecha { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public decimal PrecioLista { get; set; }    // Precio oficial (Producto.PrecioVenta)
        public decimal PrecioVendido { get; set; }  // Precio en factura
        public decimal Descuento { get; set; }      // Diferencia
        public string Usuario { get; set; } = string.Empty;
    }
    
    // Mantenemos el anterior por si acaso, renombrado o tal cual si no molesta
    public class ReporteAuditoriaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
    }
}