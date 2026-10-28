using BlazorApp1.Models;
using BlazorApp1.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Data
{
    public interface InterInventarioService
    {
        Task<List<VarianteDTO>> ObtenerVariantesAsync(string terminoBusqueda);
        Task<List<VarianteDTO>> ObtenerAlertasStockAsync();
        Task<OperacionResultadoDTO> RegistrarMovimientoBodegaAsync(MovimientoBodegaDTO movimiento);
        Task<OperacionResultadoDTO> RegistrarNuevaPrendaAsync(NuevaPrendaDTO nuevaPrendaDto);
        Task<bool> EliminarPrendaAsync(string codigoIdentificador);
    }

    public class InventarioService : InterInventarioService
    {
        private readonly LilaBoutiqueContext _context;

        public InventarioService(LilaBoutiqueContext context)
        {
            _context = context;
        }

        public async Task<List<VarianteDTO>> ObtenerVariantesAsync(string terminoBusqueda)
        {
            var query = _context.VariantesPrenda.Include(v => v.Prenda).AsQueryable();

            if (!string.IsNullOrWhiteSpace(terminoBusqueda))
            {
                terminoBusqueda = terminoBusqueda.ToLower();
                query = query.Where(v => v.Prenda.Marca.ToLower().Contains(terminoBusqueda) ||
                                         v.CodigoPrenda.ToLower().Contains(terminoBusqueda));
            }

            return await query.Select(v => new VarianteDTO
            {
                IdVariante = v.IdVariante,
                CodigoPrenda = v.CodigoPrenda,
                NombrePrenda = v.Prenda.Marca,
                Talla = v.Talla,
                Color = v.Color,
                StockFisicoActual = v.StockFisicoActual,
                LimiteAlerta = v.LimiteAlerta
            }).ToListAsync();
        }

        public async Task<List<VarianteDTO>> ObtenerAlertasStockAsync()
        {
            return await _context.VariantesPrenda
                .Include(v => v.Prenda)
                .Where(v => v.StockFisicoActual <= v.LimiteAlerta)
                .Select(v => new VarianteDTO
                {
                    IdVariante = v.IdVariante,
                    CodigoPrenda = v.CodigoPrenda,
                    NombrePrenda = v.Prenda.Marca,
                    Talla = v.Talla,
                    Color = v.Color,
                    StockFisicoActual = v.StockFisicoActual,
                    LimiteAlerta = v.LimiteAlerta
                }).ToListAsync();
        }

        public async Task<OperacionResultadoDTO> RegistrarNuevaPrendaAsync(NuevaPrendaDTO dto)
        {
            // Validar que el código identificador no exista previamente para evitar choques
            bool existe = await _context.Prendas.AnyAsync(p => p.CodigoIdentificador == dto.CodigoIdentificador);
            if (existe)
            {
                return new OperacionResultadoDTO { Exito = false, MensajeError = "Ya existe una prenda registrada con ese código." };
            }

            // Crear la entidad maestra (Prenda)
            var nuevaPrenda = new Prenda
            {
                CodigoIdentificador = dto.CodigoIdentificador,
                Marca = dto.Marca,
                DescripcionBreve = dto.DescripcionBreve,
                Categoria = dto.Categoria,
                Variantes = new List<VariantePrenda>() // Iniciar la colección
            };

            // Mapear y adjuntar las variantes (Composición fuerte descrita en el diagrama de clases)
            foreach (var v in dto.Variantes)
            {
                nuevaPrenda.Variantes.Add(new VariantePrenda
                {
                    Talla = v.Talla,
                    Color = v.Color,
                    StockFisicoActual = v.StockInicial,
                    LimiteAlerta = v.LimiteAlerta
                });
            }

            // Entity Framework Core es inteligente: al agregar la prenda maestra, 
            // automáticamente insertará sus variantes vinculadas en la base de datos.
            _context.Prendas.Add(nuevaPrenda);
            await _context.SaveChangesAsync();

            return new OperacionResultadoDTO { Exito = true };
        }

        public async Task<OperacionResultadoDTO> RegistrarMovimientoBodegaAsync(MovimientoBodegaDTO dto)
        {
            var variante = await _context.VariantesPrenda.FindAsync(dto.IdVariante);
            if (variante == null) return new OperacionResultadoDTO { Exito = false, MensajeError = "Variante no encontrada." };

            // Evaluar reglas de negocio centralizadamente (Cumplimiento REQ-04)
            if (dto.TipoOperacion == "MERMA_DAÑO" || dto.TipoOperacion == "VENTA_CLIENTE")
            {
                if (variante.StockFisicoActual < dto.Cantidad)
                {
                    return new OperacionResultadoDTO { Exito = false, MensajeError = "Stock insuficiente para realizar la salida." };
                }
                variante.StockFisicoActual -= dto.Cantidad;
            }
            else if (dto.TipoOperacion == "RECEPCION_PROVEEDOR")
            {
                variante.StockFisicoActual += dto.Cantidad;
            }
            else
            {
                return new OperacionResultadoDTO { Exito = false, MensajeError = "Operación no válida." };
            }

            // Registrar en tabla de auditoría
            var auditoria = new MovimientoAuditoria
            {
                IdVariante = dto.IdVariante,
                IdUsuario = 1, // Nota: En producción, extraer ID del token de sesión actual
                TipoOperacion = dto.TipoOperacion,
                CantidadAfectada = dto.Cantidad,
                Justificacion = dto.Justificacion,
                FechaHora = DateTime.Now
            };

            _context.MovimientosAuditoria.Add(auditoria);
            await _context.SaveChangesAsync();

            return new OperacionResultadoDTO { Exito = true };
        }

        public async Task<bool> EliminarPrendaAsync(string codigoIdentificador)
        {
            var prenda = await _context.Prendas.FindAsync(codigoIdentificador);

            if (prenda != null)
            {
                _context.Prendas.Remove(prenda);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}