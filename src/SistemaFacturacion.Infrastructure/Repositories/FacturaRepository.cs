using Microsoft.EntityFrameworkCore;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Infrastructure.Persistence;

namespace SistemaFacturacion.Infrastructure.Repositories;

public class FacturaRepository : IFacturaRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IStockService _stockService;

    public FacturaRepository(ApplicationDbContext db, IStockService stockService)
    {
        _db = db;
        _stockService = stockService;
    }

    public async Task<Factura> CrearFacturaAsync(Factura factura, CancellationToken ct = default)
    {
        // Iniciar una transacción
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Validar Stock (doble chequeo)
            var (esValido, error) = await _stockService.ValidarStockAsync(factura.Detalles, ct);
            if (!esValido)
            {
                throw new InvalidOperationException($"Error de stock: {error}");
            }

            // 2. Guardar la cabecera de la factura
            _db.Facturas.Add(factura);
            await _db.SaveChangesAsync(ct); 
            // EF automáticamente actualiza el IdFactura en el objeto 'factura'

            // 3. Asignar el IdFactura a los detalles (ya deberían venir asignados por el controlador)
            // (EF maneja esto si la relación está bien configurada en la entidad)

            // 4. Descontar el Stock
            await _stockService.DescontarStockAsync(factura.Detalles, ct);
            
            // 5. Confirmar la transacción
            await tx.CommitAsync(ct);

            return factura;
        }
        catch (Exception)
        {
            // Si algo falla (guardar factura o descontar stock), revierte todo.
            await tx.RollbackAsync(ct);
            throw; // Relanza la excepción
        }
    }
}