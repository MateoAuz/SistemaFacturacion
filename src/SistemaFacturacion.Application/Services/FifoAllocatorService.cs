// AGREGAR: Services/FifoAllocatorService.cs
using SistemaFacturacion.Domain.Entities;

namespace SistemaFacturacion.Application.Services
{
    public class FifoAllocatorService
    {
        public class LoteConsumo
        {
            public int LoteId { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioCompra { get; set; }
        }

        public static List<LoteConsumo> AsignarConsumoFifo(ICollection<Lote> lotes, int cantidadRequerida)
        {
            var lotesDisponibles = lotes
                .Where(l => l.CantidadActual > 0)
                .OrderBy(l => l.FechaIngreso)
                .ToList();

            var asignacion = new List<LoteConsumo>();
            var cantidadPendiente = cantidadRequerida;

            foreach (var lote in lotesDisponibles)
            {
                if (cantidadPendiente <= 0) break;

                var cantidadConsumir = Math.Min(lote.CantidadActual, cantidadPendiente);
                asignacion.Add(new LoteConsumo 
                { 
                    LoteId = lote.IdLote, 
                    Cantidad = cantidadConsumir,
                    PrecioCompra = lote.PrecioCompra
                });

                cantidadPendiente -= cantidadConsumir;
            }

            if (cantidadPendiente > 0)
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente. Se requieren {cantidadRequerida}, disponibles {cantidadRequerida - cantidadPendiente}");
            }

            return asignacion;
        }
    }
}