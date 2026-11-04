using Microsoft.EntityFrameworkCore;
using LilasBoutique.Data;
using LilasBoutique.Models;

namespace LilasBoutique.Data
{
    public interface IInventarioService
    {
        Task<List<Categoria>> ObtenerCategorias();
        Task<List<Talla>> ObtenerTallas();
        Task<List<Color>> ObtenerColores();
        Task GuardarPrendaCompleta(Prenda prenda, List<VariantePrenda> variantes);
        Task<List<VariantePrenda>> ObtenerCatalogo(string filtroBusqueda = "");
        Task RegistrarVenta(int varianteId, int cantidadVendida, int usuarioId);
        Task RegistrarMovimientoBodega(int varianteId, string tipoOperacion, int cantidad, int usuarioId, string justificacion);
        Task EliminarVariante(int idVariante);
        Task ActualizarVariante(VariantePrenda variante);

        // Métodos de Creación
        Task AgregarCategoria(Categoria categoria);
        Task AgregarTalla(Talla talla);
        Task AgregarColor(Color color);

        // Métodos de Eliminación
        Task EliminarCategoria(int idCategoria);
        Task EliminarTalla(int idTalla);
        Task EliminarColor(int idColor);

        // Métodos de Actualización
        Task ActualizarCategoria(Categoria categoria);
        Task ActualizarTalla(Talla talla);
        Task ActualizarColor(Color color);
    }

    public class InventarioService : IInventarioService
    {
        private readonly LilaBoutiqueContext _context;

        public InventarioService(LilaBoutiqueContext context) => _context = context;

        // Se agrega AsNoTracking() para evitar el bloqueo en memoria que causaba el error de asociación al vender o editar.
        public async Task<List<Categoria>> ObtenerCategorias() => await _context.Categoria.AsNoTracking().ToListAsync();
        public async Task<List<Talla>> ObtenerTallas() => await _context.Talla.AsNoTracking().ToListAsync();
        public async Task<List<Color>> ObtenerColores() => await _context.Color.AsNoTracking().ToListAsync();

        public async Task GuardarPrendaCompleta(Prenda prenda, List<VariantePrenda> variantes)
        {
            // Limpiamos la navegación para que EF Core no intente insertar colores o tallas duplicadas
            foreach (var v in variantes)
            {
                v.Talla = null!;
                v.Color = null!;
                v.Prenda = null!;
            }

            prenda.Variantes = variantes;
            _context.Prenda.Add(prenda);
            await _context.SaveChangesAsync();

            // Generamos auditoría automática si el usuario registró stock inicial desde "Registrar Prenda"
            foreach (var v in prenda.Variantes)
            {
                if (v.StockFisicoActual > 0)
                {
                    _context.MovimientoAuditoria.Add(new MovimientoAuditoria
                    {
                        VarianteId = v.IdVariante,
                        UsuarioId = 1, // ID temporal, puedes cambiarlo por el usuario en sesión
                        CantidadAfectada = v.StockFisicoActual,
                        TipoOperacion = "RECEPCION_PROVEEDOR",
                        Justificacion = "Stock inicial ingresado al crear la prenda",
                        FechaHora = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<VariantePrenda>> ObtenerCatalogo(string filtroBusqueda = "")
        {
            var query = _context.VariantePrenda
                .AsNoTracking() // Libera los datos de la memoria para que no haya conflictos de llave foránea
                .Include(v => v.Prenda).ThenInclude(p => p.Categoria)
                .Include(v => v.Talla)
                .Include(v => v.Color)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtroBusqueda))
            {
                query = query.Where(v => v.Prenda.Marca.Contains(filtroBusqueda) ||
                                         v.Prenda.Categoria.NombreCategoria.Contains(filtroBusqueda) ||
                                         v.Talla.NombreTalla.Contains(filtroBusqueda));
            }

            return await query.ToListAsync();
        }

        public async Task RegistrarVenta(int varianteId, int cantidadVendida, int usuarioId)
        {
            var variante = await _context.VariantePrenda.FindAsync(varianteId);
            if (variante == null) throw new Exception("Variante no encontrada");

            if (variante.StockFisicoActual < cantidadVendida)
                throw new Exception("Stock insuficiente para realizar la venta.");

            variante.StockFisicoActual -= cantidadVendida;

            var movimiento = new MovimientoAuditoria
            {
                VarianteId = varianteId,
                UsuarioId = usuarioId,
                CantidadAfectada = cantidadVendida,
                TipoOperacion = "VENTA_CLIENTE",
                Justificacion = "Venta desde catálogo",
                FechaHora = DateTime.Now
            };

            _context.MovimientoAuditoria.Add(movimiento);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarVariante(int idVariante)
        {
            var variante = await _context.VariantePrenda.FindAsync(idVariante);
            if (variante != null)
            {
                // Soluciona el error de "Esta prenda ya tiene historial" eliminando primero los movimientos asociados en la BD
                var movimientosAsociados = await _context.MovimientoAuditoria.Where(m => m.VarianteId == idVariante).ToListAsync();
                if (movimientosAsociados.Any())
                {
                    _context.MovimientoAuditoria.RemoveRange(movimientosAsociados);
                }

                _context.VariantePrenda.Remove(variante);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ActualizarVariante(VariantePrenda varianteModificada)
        {
            // 1. Buscamos la variante existente incluyendo su Prenda en la base de datos
            var varianteDb = await _context.VariantePrenda
                .Include(v => v.Prenda)
                .FirstOrDefaultAsync(v => v.IdVariante == varianteModificada.IdVariante);

            if (varianteDb == null)
                throw new Exception("Variante no encontrada.");

            // 2. Actualizamos el precio de la variante
            varianteDb.Precio = varianteModificada.Precio;

            // 3. Si la prenda existe y se modificó la marca, la actualizamos también
            if (varianteDb.Prenda != null && varianteModificada.Prenda != null)
            {
                varianteDb.Prenda.Marca = varianteModificada.Prenda.Marca;
                _context.Entry(varianteDb.Prenda).State = EntityState.Modified;
            }

            _context.Entry(varianteDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task RegistrarMovimientoBodega(int varianteId, string tipoOperacion, int cantidad, int usuarioId, string justificacion)
        {
            var variante = await _context.VariantePrenda.FindAsync(varianteId);
            if (variante == null) throw new Exception("Variante no encontrada");

            bool esSalida = tipoOperacion.Contains("Salida");

            if (esSalida)
            {
                if (variante.StockFisicoActual < cantidad)
                    throw new Exception("El stock actual no puede quedar en negativo.");

                variante.StockFisicoActual -= cantidad;
            }
            else
            {
                variante.StockFisicoActual += cantidad;
            }

            // MAPEO EXACTO: Traducimos a los valores permitidos por tu restricción CHK_Tipo_Operacion
            string tipoOperacionDB = esSalida ? "MERMA_DAÑO" : "RECEPCION_PROVEEDOR";

            var movimiento = new MovimientoAuditoria
            {
                VarianteId = varianteId,
                UsuarioId = usuarioId,
                CantidadAfectada = esSalida ? -cantidad : cantidad,
                TipoOperacion = tipoOperacionDB, // Valor exacto exigido por SQL
                Justificacion = justificacion,   // Guardamos el texto escrito en la UI
                FechaHora = DateTime.Now
            };

            _context.MovimientoAuditoria.Add(movimiento);
            await _context.SaveChangesAsync();
        }

        // --- MÉTODOS DE CREACIÓN ---
        public async Task AgregarCategoria(Categoria categoria)
        {
            _context.Categoria.Add(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task AgregarTalla(Talla nuevaTalla)
        {
            // 1. Verificamos si ya existe la misma talla DENTRO de la misma categoría
            bool existeTalla = await _context.Talla.AnyAsync(t =>
                t.CategoriaId == nuevaTalla.CategoriaId &&
                t.NombreTalla.ToLower() == nuevaTalla.NombreTalla.ToLower());

            if (existeTalla)
            {
                // Si existe, lanzamos un error que será capturado por el bloque try-catch de Parametros.razor
                throw new Exception($"La talla '{nuevaTalla.NombreTalla}' ya está registrada en esta categoría.");
            }

            // 2. Si no existe, procedemos a guardarla normalmente
            _context.Talla.Add(nuevaTalla);
            await _context.SaveChangesAsync();
        }

        public async Task AgregarColor(Color color)
        {
            _context.Color.Add(color);
            await _context.SaveChangesAsync();
        }

        // --- MÉTODOS DE ELIMINACIÓN ---
        public async Task EliminarCategoria(int idCategoria)
        {
            var entidad = await _context.Categoria.FindAsync(idCategoria);
            if (entidad != null) { _context.Categoria.Remove(entidad); await _context.SaveChangesAsync(); }
        }

        public async Task EliminarTalla(int idTalla)
        {
            var entidad = await _context.Talla.FindAsync(idTalla);
            if (entidad != null) { _context.Talla.Remove(entidad); await _context.SaveChangesAsync(); }
        }

        public async Task EliminarColor(int idColor)
        {
            var entidad = await _context.Color.FindAsync(idColor);
            if (entidad != null) { _context.Color.Remove(entidad); await _context.SaveChangesAsync(); }
        }

        // --- MÉTODOS DE ACTUALIZACIÓN ---
        public async Task ActualizarCategoria(Categoria categoria)
        {
            var existente = await _context.Categoria.FindAsync(categoria.IdCategoria);
            if (existente != null)
            {
                existente.NombreCategoria = categoria.NombreCategoria;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ActualizarTalla(Talla tallaModificada)
        {
            // 1. Verificamos duplicados, asegurándonos de ignorar el ID de la talla que estamos editando
            bool existeTalla = await _context.Talla.AnyAsync(t =>
                t.CategoriaId == tallaModificada.CategoriaId &&
                t.NombreTalla.ToLower() == tallaModificada.NombreTalla.ToLower() &&
                t.IdTalla != tallaModificada.IdTalla);

            if (existeTalla)
            {
                throw new Exception($"La talla '{tallaModificada.NombreTalla}' ya está registrada en esta categoría.");
            }

            // 2. Actualizamos usando ExecuteUpdateAsync para mayor rendimiento y evitar contaminación de caché
            await _context.Talla
                .Where(t => t.IdTalla == tallaModificada.IdTalla)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.NombreTalla, tallaModificada.NombreTalla)
                    .SetProperty(t => t.CategoriaId, tallaModificada.CategoriaId));
        }

        public async Task ActualizarColor(Color color)
        {
            var existente = await _context.Color.FindAsync(color.IdColor);
            if (existente != null)
            {
                existente.NombreColor = color.NombreColor;
                await _context.SaveChangesAsync();
            }
        }
    }
}