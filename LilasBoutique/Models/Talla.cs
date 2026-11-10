using System.Collections.Generic;

namespace LilasBoutique.Models
{
    /// <summary>
    /// Entidad catálogo de dimensiones físicas (Ej. S, M, L, 32, 34).
    /// Depende directamente de una Categoría para evitar cruces ilógicos (Ej. Talla 32 en Camisas).
    /// </summary>
    public class Talla
    {
        #region Propiedades Escalares
        public int IdTalla { get; set; }
        public string NombreTalla { get; set; } = null!;
        public int CategoriaId { get; set; }
        #endregion

        #region Propiedades de Navegación
        public Categoria Categoria { get; set; } = null!; // Corregido: añadido null! para silenciar advertencias
        public ICollection<VariantePrenda> Variantes { get; set; } = new List<VariantePrenda>();
        #endregion
    }
}