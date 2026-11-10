using System.Collections.Generic;

namespace LilasBoutique.Models
{
    /// <summary>
    /// Entidad Maestra que define los datos generales (Ficha Maestra) de un artículo.
    /// Delega el control de stock, talla y color a sus 'Variantes'.
    /// </summary>
    public class Prenda
    {
        #region Propiedades Escalares
        public int IdPrenda { get; set; }
        public string CodigoIdentificador { get; set; } = null!;
        public string Marca { get; set; } = null!;
        public string? DescripcionBreve { get; set; }
        public int CategoriaId { get; set; }
        #endregion

        #region Propiedades de Navegación
        public Categoria Categoria { get; set; } = null!;
        public ICollection<VariantePrenda> Variantes { get; set; } = new List<VariantePrenda>();
        #endregion
    }
}