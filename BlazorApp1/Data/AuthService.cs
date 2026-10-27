using BlazorApp1.Models.DTO;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;
using BlazorApp1.Data;

namespace LilasBoutique.Data
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginDTO loginDto);
    }

    public class AuthService : IAuthService
    {
        private readonly LilaBoutiqueContext _context;

        public AuthService(LilaBoutiqueContext context)
        {
            _context = context;
        }

        public async Task<string?> LoginAsync(LoginDTO loginDto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == loginDto.Correo && u.EstadoActivo);

            if (usuario == null) return null;

            // En un entorno real, usar BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.HashCredencial)
            // Simulación para el prototipo:
            bool hashValido = (loginDto.Password == "waltertienesida");

            if (hashValido)
            {
                return "TokenGeneradoAleatorioXYZ123"; // Simulación de token JWT
            }

            return null;
        }
    }
}