using Microsoft.EntityFrameworkCore;
using LilasBoutique.Models;

namespace LilasBoutique.Data
{
    #region Interface
    public interface IAuthService
    {
        Task<Usuario?> ObtenerUsuarioPorCorreo(string correo);
    }
    #endregion

    #region Implementation
    public class AuthService : IAuthService
    {
        private readonly LilaBoutiqueContext _context;

        public AuthService(LilaBoutiqueContext context) => _context = context;

        /// <summary>
        /// Busca un usuario en la base de datos por su correo electrónico 
        /// e incluye su Rol asociado para la validación de privilegios (RBAC).
        /// </summary>
        /// <param name="correo">Correo ingresado en el login.</param>
        /// <returns>Objeto Usuario o null si no existe.</returns>
        public async Task<Usuario?> ObtenerUsuarioPorCorreo(string correo)
        {
            return await _context.Usuario
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);
        }
    }
    #endregion
}