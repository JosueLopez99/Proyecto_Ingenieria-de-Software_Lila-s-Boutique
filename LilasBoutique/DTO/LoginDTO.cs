using System.ComponentModel.DataAnnotations;

namespace LilasBoutique.DTOs
{
    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) utilizado para estructurar 
    /// y validar las credenciales ingresadas en el inicio de sesión.
    /// </summary>
    public class LoginDTO
    {
        #region Propiedades de Formulario

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;

        #endregion
    }
}