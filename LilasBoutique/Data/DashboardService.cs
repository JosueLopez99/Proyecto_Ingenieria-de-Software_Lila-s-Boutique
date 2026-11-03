using Microsoft.EntityFrameworkCore;
using LilasBoutique.Models;

namespace LilasBoutique.Data
{
    public interface IDashboardService
    {
        Task<decimal> CalcularValorInventario();
        Task<List<VariantePrenda>> ObtenerAlertasStock();
    }

    public class DashboardService : IDashboardService
    {
        private readonly LilaBoutiqueContext _context;

        public DashboardService(LilaBoutiqueContext context) => _context = context;

        public async Task<decimal> CalcularValorInventario()
        {
            // Sumatoria aritmética: (StockFisico * PrecioUnitario) de todo el catálogo
            return await _context.VariantePrenda
                .SumAsync(v => v.StockFisicoActual * v.Precio);
        }

        public async Task<List<VariantePrenda>> ObtenerAlertasStock()
        {
            // Recupera todas las variantes cuyo stock es menor o igual al umbral de alerta definido (Default: 10)
            return await _context.VariantePrenda
                .Include(v => v.Prenda)
                    .ThenInclude(p => p.Categoria) // <-- Esta instrucción resuelve el error nulo
                .Include(v => v.Talla)
                .Include(v => v.Color)
        .Where(v => v.StockFisicoActual <= v.LimiteAlerta)
        .ToListAsync();
        }
    }
}