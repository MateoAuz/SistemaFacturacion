using System;

namespace SistemaFacturacion.Application.DTOs
{
    public class ReporteVentasDto
    {
        public string Periodo { get; set; } = string.Empty; 
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
        public decimal PrecioPromedio { get; set; } 
    }

    public class ReporteAuditoriaVentaDto
    {
        public DateTime Fecha { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public decimal PrecioLista { get; set; }    
        public decimal PrecioVendido { get; set; }  
        public decimal Descuento { get; set; }      
        public string Usuario { get; set; } = string.Empty;
    }
    
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