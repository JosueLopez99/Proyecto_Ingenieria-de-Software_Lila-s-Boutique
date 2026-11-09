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

        // Actualizado para recibir el correo exacto del vendedor
        Task RegistrarVenta(int varianteId, int cantidadVendida, string correoUsuario);

        Task RegistrarMovimientoBodega(int idVariante, string tipoOperacion, int cantidad, string correoUsuario, string justificacion);
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

        public async Task GuardarPrendaCompleta(Prenda nuevaPrenda, List<VariantePrenda> variantes)
        {
            // 1. Verificar que el código identificador no exista ya
            bool codigoExiste = await _context.Prenda
                .AnyAsync(p => p.CodigoIdentificador.ToLower() == nuevaPrenda.CodigoIdentificador.ToLower());

            if (codigoExiste)
            {
                throw new Exception($"El código identificador '{nuevaPrenda.CodigoIdentificador}' ya está en uso por otra prenda.");
            }

            // 2. NUEVA VALIDACIÓN: Bloqueo de clones físicos exactos
            foreach (var varianteTemporal in variantes)
            {
                bool clonExiste = await _context.VariantePrenda
                    .Include(vp => vp.Prenda)
                    .AnyAsync(vp =>
                        vp.Prenda.CategoriaId == nuevaPrenda.CategoriaId &&
                        vp.Prenda.Marca.ToLower() == nuevaPrenda.Marca.ToLower() &&
                        vp.TallaId == varianteTemporal.TallaId &&
                        vp.ColorId == varianteTemporal.ColorId);

                if (clonExiste)
                {
                    // Extraemos los nombres reales de la BD para dar un error específico y entendible
                    var nombreTalla = await _context.Talla.Where(t => t.IdTalla == varianteTemporal.TallaId).Select(t => t.NombreTalla).FirstOrDefaultAsync();
                    var nombreColor = await _context.Color.Where(c => c.IdColor == varianteTemporal.ColorId).Select(c => c.NombreColor).FirstOrDefaultAsync();

                    throw new Exception($"Registro bloqueado: Ya existe una prenda '{nuevaPrenda.Marca}' en talla '{nombreTalla}' y color '{nombreColor}' en el catálogo. No se permiten registros duplicados.");
                }
            }

            // 3. Proceso original de guardado si pasa todas las pruebas
            _context.Prenda.Add(nuevaPrenda);
            await _context.SaveChangesAsync();

            foreach (var variante in variantes)
            {
                variante.PrendaId = nuevaPrenda.IdPrenda;
                _context.VariantePrenda.Add(variante);
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

        public async Task RegistrarVenta(int varianteId, int cantidadVendida, string correoUsuario)
        {
            var variante = await _context.VariantePrenda.FindAsync(varianteId);
            if (variante == null) throw new Exception("Variante no encontrada");

            if (variante.StockFisicoActual < cantidadVendida)
                throw new Exception("Stock insuficiente para realizar la venta.");

            // Buscar el ID real del usuario basado en el correo exacto de la sesión
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo.ToLower() == correoUsuario.ToLower());
            if (usuario == null) throw new Exception("No se pudo validar el usuario actual en la base de datos.");

            variante.StockFisicoActual -= cantidadVendida;

            var movimiento = new MovimientoAuditoria
            {
                VarianteId = varianteId,
                UsuarioId = usuario.IdUsuario, // ID real obtenido de la BD
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
            // 1. Obtenemos la variante original y su ficha maestra (Prenda) para analizar la situación
            var varianteOriginal = await _context.VariantePrenda
                .Include(v => v.Prenda)
                .ThenInclude(p => p.Variantes) // Cargamos las variantes hermanas
                .FirstOrDefaultAsync(v => v.IdVariante == varianteModificada.IdVariante);

            if (varianteOriginal == null) throw new Exception("La variante no fue encontrada.");

            // Detectamos si el usuario realmente cambió el código o la marca en el modal
            bool huboCambioMaestro = varianteOriginal.Prenda.CodigoIdentificador.ToLower() != varianteModificada.Prenda.CodigoIdentificador.ToLower() ||
                                     varianteOriginal.Prenda.Marca.ToLower() != varianteModificada.Prenda.Marca.ToLower();

            // 2. Validación: Que el nuevo código no esté ocupado por OTRA prenda activa distinta
            if (huboCambioMaestro)
            {
                bool codigoOcupado = await _context.Prenda
                    .Where(p => p.CodigoIdentificador.ToLower() == varianteModificada.Prenda.CodigoIdentificador.ToLower()
                             && p.IdPrenda != varianteOriginal.PrendaId)
                    .AnyAsync(p => p.Variantes.Any());

                if (codigoOcupado) throw new Exception($"El código '{varianteModificada.Prenda.CodigoIdentificador}' ya pertenece a otra prenda activa en el catálogo.");
            }

            int cantidadVariantesAsociadas = varianteOriginal.Prenda.Variantes.Count;

            // =========================================================
            // 3. ESTRATEGIA DE SEPARACIÓN (SPLIT)
            // =========================================================
            // Si hay un cambio en el código o marca, Y la prenda maestra tiene más de 1 variante (hermanas compartiendo código)...
            if (huboCambioMaestro && cantidadVariantesAsociadas > 1)
            {
                // Creamos una nueva Ficha Maestra independiente para aislar esta variante
                var nuevaPrenda = new Prenda
                {
                    CodigoIdentificador = varianteModificada.Prenda.CodigoIdentificador,
                    Marca = varianteModificada.Prenda.Marca,
                    CategoriaId = varianteOriginal.Prenda.CategoriaId,
                    DescripcionBreve = varianteOriginal.Prenda.DescripcionBreve
                };

                _context.Prenda.Add(nuevaPrenda);
                await _context.SaveChangesAsync(); // Guardamos para que SQL genere el nuevo IdPrenda

                // Movemos esta variante específica a la nueva Prenda y actualizamos su precio
                await _context.VariantePrenda
                    .Where(v => v.IdVariante == varianteModificada.IdVariante)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(v => v.PrendaId, nuevaPrenda.IdPrenda)
                        .SetProperty(v => v.Precio, varianteModificada.Precio));
            }
            else
            {
                // Si es "hija única", O si NO hubo cambio de código/marca (es decir, solo le cambiaron el precio)
                await _context.VariantePrenda
                    .Where(v => v.IdVariante == varianteModificada.IdVariante)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(v => v.Precio, varianteModificada.Precio));

                // Si fue hija única y le cambiaron el código, actualizamos a la madre directamente porque no afecta a nadie más
                if (huboCambioMaestro)
                {
                    await _context.Prenda
                        .Where(p => p.IdPrenda == varianteOriginal.PrendaId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.CodigoIdentificador, varianteModificada.Prenda.CodigoIdentificador)
                            .SetProperty(p => p.Marca, varianteModificada.Prenda.Marca));
                }
            }
        }

        public async Task RegistrarMovimientoBodega(int idVariante, string tipoOperacion, int cantidad, string correoUsuario, string justificacion)
        {
            var variante = await _context.VariantePrenda.FindAsync(idVariante);
            if (variante == null) throw new Exception("La variante seleccionada no existe.");

            // 1. Buscamos el ID real del usuario basado en el correo exacto de la sesión
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo.ToLower() == correoUsuario.ToLower());
            if (usuario == null) throw new Exception("No se pudo validar el usuario actual en la base de datos.");

            // 2. Lógica estricta de Suma o Resta
            if (tipoOperacion == "RECEPCION_PROVEEDOR")
            {
                variante.StockFisicoActual += cantidad;
            }
            else if (tipoOperacion == "MERMA_DAÑO")
            {
                if (variante.StockFisicoActual < cantidad)
                    throw new Exception($"No hay suficiente stock. Stock actual: {variante.StockFisicoActual}");

                variante.StockFisicoActual -= cantidad;
            }
            else
            {
                throw new Exception("Tipo de operación no reconocida.");
            }

            // 3. Registro exacto en la Auditoría
            var auditoria = new MovimientoAuditoria
            {
                VarianteId = idVariante,
                UsuarioId = usuario.IdUsuario, // ID real obtenido de la BD
                TipoOperacion = tipoOperacion,
                CantidadAfectada = cantidad,
                FechaHora = DateTime.Now,
                Justificacion = justificacion
            };

            _context.MovimientoAuditoria.Add(auditoria);
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