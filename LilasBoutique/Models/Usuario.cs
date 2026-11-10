using System.Collections.Generic;

namespace LilasBoutique.Models
{
    /// <summary>
    /// Entidad que representa a los empleados con acceso al sistema.
    /// Almacena credenciales hasheadas (BCrypt) y gestiona bloqueos lógicos (EstadoActivo).
    /// </summary>
    public class Usuario
    {
        #region Propiedades Escalares
        public int IdUsuario { get; set; }
        public string Correo { get; set; } = null!;
        public string HashCredencial { get; set; } = null!;
        public bool EstadoActivo { get; set; }
        public int RolId { get; set; }
        #endregion

        #region Propiedades de Navegación
        public Rol Rol { get; set; } = null!;
        public ICollection<MovimientoAuditoria> Movimientos { get; set; } = new List<MovimientoAuditoria>();
        #endregion
    }
}