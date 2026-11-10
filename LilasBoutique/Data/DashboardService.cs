using Microsoft.EntityFrameworkCore;
using LilasBoutique.Models;

namespace LilasBoutique.Data
{
    #region Interface
    public interface IDashboardService
    {
        Task<decimal> CalcularValorInventario();
        Task<List<VariantePrenda>> ObtenerAlertasStock();
        Task<List<MovimientoAuditoria>> ObtenerAuditoriaReciente(int limite = 100);
    }
    #endregion

    #region Implementation
    public class DashboardService : IDashboardService
    {
        private readonly LilaBoutiqueContext _context;

        public DashboardService(LilaBoutiqueContext context) => _context = context;

        /// <summary>
        /// Ejecuta una sumatoria aritmética (StockFisico * PrecioUnitario) 
        /// delegando el cálculo al motor SQL Server.
        /// </summary>
        public async Task<decimal> CalcularValorInventario()
        {
            return await _context.VariantePrenda
                .SumAsync(v => v.StockFisicoActual * v.Precio);
        }

        /// <summary>
        /// Retorna las variantes de prenda cuyo stock actual está igual o por debajo de su propio límite definido.
        /// </summary>
        public async Task<List<VariantePrenda>> ObtenerAlertasStock()
        {
            return await _context.VariantePrenda
                .Include(v => v.Prenda)
                    .ThenInclude(p => p.Categoria)
                .Include(v => v.Talla)
                .Include(v => v.Color)
                .Where(v => v.StockFisicoActual <= v.LimiteAlerta)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna el historial de la tabla de Auditoría cargando las sub-propiedades.
        /// Retorna máximo 100 registros para evitar latencia de carga masiva en el Dashboard.
        /// </summary>
        public async Task<List<MovimientoAuditoria>> ObtenerAuditoriaReciente(int limite = 100)
        {
            return await _context.MovimientoAuditoria
                .Include(m => m.Usuario)
                .Include(m => m.Variante!)
                    .ThenInclude(v => v.Prenda)
                .Include(m => m.Variante!)
                    .ThenInclude(v => v.Talla)
                .Include(m => m.Variante!)
                    .ThenInclude(v => v.Color)
                .OrderByDescending(m => m.FechaHora)
                .Take(limite)
                .AsNoTracking()
                .ToListAsync();
        }
    }
    #endregion
}