using System;

namespace LilasBoutique.Models
{
    /// <summary>
    /// Entidad transaccional (Kardex) que registra el historial inmutable de 
    /// entradas, salidas y ventas del inventario.
    /// </summary>
    public class MovimientoAuditoria
    {
        #region Propiedades Escalares
        public int IdMovimiento { get; set; }
        public DateTime FechaHora { get; set; }
        public string TipoOperacion { get; set; } = null!;
        public int CantidadAfectada { get; set; }
        public string? Justificacion { get; set; } // Puede ser nula según diseño
        public int UsuarioId { get; set; }
        public int VarianteId { get; set; }
        #endregion

        #region Propiedades de Navegación
        public Usuario Usuario { get; set; } = null!;
        public VariantePrenda Variante { get; set; } = null!;
        #endregion
    }
}