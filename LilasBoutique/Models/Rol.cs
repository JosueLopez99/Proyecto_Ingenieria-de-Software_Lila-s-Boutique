using System.Collections.Generic;

namespace LilasBoutique.Models
{
    /// <summary>
    /// Entidad catálogo que define los niveles de acceso al sistema (RBAC).
    /// </summary>
    public class Rol
    {
        #region Propiedades Escalares
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = null!;
        public int NivelPrivilegio { get; set; }
        #endregion

        #region Propiedades de Navegación
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        #endregion
    }
}