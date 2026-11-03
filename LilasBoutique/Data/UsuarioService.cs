using Microsoft.EntityFrameworkCore;
using LilasBoutique.Models;

namespace LilasBoutique.Data
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> ObtenerTodos();
        Task<List<Rol>> ObtenerRoles(); // Necesario para el combobox de la UI
        Task ActualizarPassword(int usuarioId, string nuevoHash);
        Task ActualizarRol(int usuarioId, int nuevoRolId);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly LilaBoutiqueContext _context;

        public UsuarioService(LilaBoutiqueContext context) => _context = context;

        public async Task<List<Usuario>> ObtenerTodos()
        {
            // Se utiliza el DbSet "Usuarios" declarado en LilaBoutiqueContext
            return await _context.Usuario.Include(u => u.Rol).ToListAsync();
        }

        public async Task<List<Rol>> ObtenerRoles()
        {
            // Retorna los roles de la base de datos para la gestión administrativa[cite: 1, 2]
            return await _context.Rol.ToListAsync();
        }

        public async Task ActualizarPassword(int usuarioId, string nuevoHash)
        {
            var usuario = await _context.Usuario.FindAsync(usuarioId);
            if (usuario != null)
            {
                usuario.HashCredencial = nuevoHash;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ActualizarRol(int usuarioId, int nuevoRolId)
        {
            var usuario = await _context.Usuario.FindAsync(usuarioId);
            if (usuario != null)
            {
                usuario.RolId = nuevoRolId;
                await _context.SaveChangesAsync();
            }
        }
    }
}