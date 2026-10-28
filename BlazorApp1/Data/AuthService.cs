using BlazorApp1.Models.DTO;
using Microsoft.EntityFrameworkCore;
// using BCrypt.Net; (Puedes agregarlo arriba o usar la ruta completa como en el código)

namespace BlazorApp1.Data
{
    public interface InterAuthService
    {
        Task<string?> LoginAsync(LoginDTO loginDto);
    }

    public class AuthService : InterAuthService
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

            // Implementación real del algoritmo Hash (Cumplimiento Ley 787)
            // Compara la contraseña que digitó el usuario con el Hash encriptado de la base de datos
            bool hashValido = BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.HashCredencial);

            if (hashValido)
            {
                return "TokenGeneradoAleatorioXYZ123"; // Simulación de token JWT que ya tenías
            }

            return null;
        }
    }
}