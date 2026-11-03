using Microsoft.EntityFrameworkCore;
using LilasBoutique.Models;

namespace LilasBoutique.Data
{
    public interface IAuthService
    {
        Task<Usuario?> ObtenerUsuarioPorCorreo(string correo);
    }

    public class AuthService : IAuthService
    {
        private readonly LilaBoutiqueContext _context;

        public AuthService(LilaBoutiqueContext context) => _context = context;

        public async Task<Usuario?> ObtenerUsuarioPorCorreo(string correo)
        {
            return await _context.Usuario
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);
        }
    }
}