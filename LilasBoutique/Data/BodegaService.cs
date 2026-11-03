
using LilasBoutique.Models;

namespace LilasBoutique.Data
{
    public interface IBodegaService
    {
        Task ProcesarMovimiento(MovimientoAuditoria movimiento);
    }

    public class BodegaService : IBodegaService
    {
        private readonly LilaBoutiqueContext _context;

        public BodegaService(LilaBoutiqueContext context) => _context = context;

        public async Task ProcesarMovimiento(MovimientoAuditoria movimiento)
        {
            var variante = await _context.VariantePrenda.FindAsync(movimiento.VarianteId);
            if (variante == null) throw new Exception("Variante de prenda no encontrada");

            if (movimiento.TipoOperacion == "RECEPCION_PROVEEDOR") //[cite: 2]
            {
                variante.StockFisicoActual += movimiento.CantidadAfectada;
            }
            else if (movimiento.TipoOperacion == "MERMA_DAÑO") //[cite: 2]
            {
                if (variante.StockFisicoActual < movimiento.CantidadAfectada)
                    throw new Exception("No se puede descontar más stock del existente (Violación CHK_Stock_Fisico)."); //[cite: 2]

                variante.StockFisicoActual -= movimiento.CantidadAfectada;
            }
            else
            {
                throw new Exception("Tipo de operación no válida.");
            }

            movimiento.FechaHora = DateTime.Now;
            _context.MovimientoAuditoria.Add(movimiento);
            await _context.SaveChangesAsync();
        }
    }
}