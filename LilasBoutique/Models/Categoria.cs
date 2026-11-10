using System.Collections.Generic;

namespace LilasBoutique.Models
{
    /// <summary>
    /// Entidad que representa las categorías principales de las prendas (Ej. Camisas, Pantalones).
    /// </summary>
    public class Categoria
    {
        #region Propiedades Escalares
        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; } = null!;
        #endregion

        #region Propiedades de Navegación
        public ICollection<Prenda> Prendas { get; set; } = new List<Prenda>();
        #endregion
    }
}