using LilasBoutique.Models;
using Microsoft.EntityFrameworkCore;

namespace LilasBoutique.Data
{
    #region Interface
    public interface IBodegaService
    {
        Task ProcesarMovimiento(MovimientoAuditoria movimiento);
    }
    #endregion

    #region Implementation
    public class BodegaService : IBodegaService
    {
        private readonly LilaBoutiqueContext _context;

        public BodegaService(LilaBoutiqueContext context) => _context = context;

        /// <summary>
        /// Ejecuta una entrada o salida de inventario sumando o restando el stock,
        /// validando que el inventario nunca sea negativo, y guardando el historial de auditoría.
        /// </summary>
        public async Task ProcesarMovimiento(MovimientoAuditoria movimiento)
        {
            // Consultamos la variante sin rastreo para evitar conflictos de adjunción en EF Core
            var variante = await _context.VariantePrenda
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdVariante == movimiento.VarianteId);

            if (variante == null)
                throw new Exception("Variante de prenda no encontrada");

            if (movimiento.TipoOperacion == "RECEPCION_PROVEEDOR")
            {
                variante.StockFisicoActual += movimiento.CantidadAfectada;
            }
            else if (movimiento.TipoOperacion == "MERMA_DAÑO")
            {
                if (variante.StockFisicoActual < movimiento.CantidadAfectada)
                    throw new Exception("No se puede descontar más stock del existente (Violación CHK_Stock_Fisico).");

                variante.StockFisicoActual -= movimiento.CantidadAfectada;
            }
            else
            {
                throw new Exception("Tipo de operación no válida.");
            }

            // Marcamos la variante como modificada para que EF Core actualice únicamente el stock
            _context.Entry(variante).State = EntityState.Modified;

            // Asignamos la fecha actual de forma forzada para seguridad y registramos
            movimiento.FechaHora = DateTime.Now;
            _context.MovimientoAuditoria.Add(movimiento);

            await _context.SaveChangesAsync();
        }
    }
    #endregion
}