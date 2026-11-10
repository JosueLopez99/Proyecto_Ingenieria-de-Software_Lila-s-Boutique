using System.Collections.Generic;

namespace LilasBoutique.Models
{
    /// <summary>
    /// Entidad catálogo para gestionar la paleta de colores disponibles.
    /// </summary>
    public class Color
    {
        #region Propiedades Escalares
        public int IdColor { get; set; }
        public string NombreColor { get; set; } = null!;
        #endregion

        #region Propiedades de Navegación
        public ICollection<VariantePrenda> Variantes { get; set; } = new List<VariantePrenda>();
        #endregion
    }
}