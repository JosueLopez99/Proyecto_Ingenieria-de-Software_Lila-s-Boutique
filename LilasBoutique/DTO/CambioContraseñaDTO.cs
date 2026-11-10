using System.ComponentModel.DataAnnotations;

namespace LilasBoutique.DTOs
{
    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) utilizado para validar 
    /// los formularios de actualización de contraseñas de los usuarios.
    /// </summary>
    public class CambioContraseñaDTO
    {
        #region Propiedades de Formulario

        [Required(ErrorMessage = "Ingrese su contraseña actual")]
        public string Actual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la nueva contraseña")]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        public string Nueva { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la contraseña")]
        [Compare("Nueva", ErrorMessage = "Las contraseñas no coinciden")]
        public string Confirmacion { get; set; } = string.Empty;

        #endregion
    }
}