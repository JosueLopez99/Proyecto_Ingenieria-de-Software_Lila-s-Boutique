using System.Collections.Generic;

namespace LilasBoutique.Models
{
    /// <summary>
    /// Entidad transaccional principal (SKU). 
    /// Combina la Ficha Maestra (Prenda) con Atributos (Talla, Color) para gestionar 
    /// unificados el inventario físico y los precios.
    /// </summary>
    public class VariantePrenda
    {
        #region Propiedades Escalares
        public int IdVariante { get; set; }
        public int PrendaId { get; set; }
        public int TallaId { get; set; }
        public int ColorId { get; set; }
        public int StockFisicoActual { get; set; }
        public decimal Precio { get; set; }
        public int LimiteAlerta { get; set; }
        #endregion

        #region Propiedades de Navegación
        public Prenda Prenda { get; set; } = null!;
        public Talla Talla { get; set; } = null!;
        public Color Color { get; set; } = null!;
        public ICollection<MovimientoAuditoria> Movimientos { get; set; } = new List<MovimientoAuditoria>();
        #endregion
    }
}