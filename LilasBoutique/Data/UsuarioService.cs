using Microsoft.EntityFrameworkCore;
using LilasBoutique.Models;

namespace LilasBoutique.Data
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> ObtenerTodos();
        Task<List<Rol>> ObtenerRoles();
        Task ActualizarPassword(int usuarioId, string nuevoHash);
        Task ActualizarRol(int usuarioId, int nuevoRolId);
        Task CrearUsuario(string correo, string contrasena, int rolId);
        Task ActualizarUsuarioCompleto(int idUsuario, string correo, string nuevaContrasena, int rolId, bool estadoActivo);

        // Nueva firma para eliminar usuarios
        Task EliminarUsuario(int idUsuario);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly LilaBoutiqueContext _context;

        public UsuarioService(LilaBoutiqueContext context) => _context = context;

        public async Task<List<Usuario>> ObtenerTodos()
        {
            return await _context.Usuario.Include(u => u.Rol).AsNoTracking().ToListAsync();
        }

        public async Task<List<Rol>> ObtenerRoles()
        {
            return await _context.Rol.AsNoTracking().ToListAsync();
        }

        public async Task ActualizarPassword(int usuarioId, string nuevoHash)
        {
            await _context.Usuario
                .Where(u => u.IdUsuario == usuarioId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.HashCredencial, nuevoHash));
        }

        public async Task ActualizarRol(int usuarioId, int nuevoRolId)
        {
            await _context.Usuario
                .Where(u => u.IdUsuario == usuarioId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.RolId, nuevoRolId));
        }

        public async Task CrearUsuario(string correo, string contrasena, int rolId)
        {
            bool existe = await _context.Usuario.AnyAsync(u => u.Correo.ToLower() == correo.ToLower());
            if (existe) throw new Exception("El correo electrónico ya está registrado en el sistema.");

            var nuevoUsuario = new Usuario
            {
                Correo = correo,
                HashCredencial = BCrypt.Net.BCrypt.HashPassword(contrasena),
                EstadoActivo = true,
                RolId = rolId
            };

            _context.Usuario.Add(nuevoUsuario);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarUsuarioCompleto(int idUsuario, string correo, string nuevaContrasena, int rolId, bool estadoActivo)
        {
            bool existe = await _context.Usuario.AnyAsync(u => u.Correo.ToLower() == correo.ToLower() && u.IdUsuario != idUsuario);
            if (existe) throw new Exception("El correo ingresado ya pertenece a otro usuario.");

            if (!string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);
                await _context.Usuario.Where(u => u.IdUsuario == idUsuario)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(u => u.Correo, correo)
                        .SetProperty(u => u.RolId, rolId)
                        .SetProperty(u => u.EstadoActivo, estadoActivo)
                        .SetProperty(u => u.HashCredencial, hash));
            }
            else
            {
                await _context.Usuario.Where(u => u.IdUsuario == idUsuario)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(u => u.Correo, correo)
                        .SetProperty(u => u.RolId, rolId)
                        .SetProperty(u => u.EstadoActivo, estadoActivo));
            }
        }

        public async Task EliminarUsuario(int idUsuario)
        {
            // Ejecuta el borrado directo en SQL
            await _context.Usuario
                .Where(u => u.IdUsuario == idUsuario)
                .ExecuteDeleteAsync();
        }
    }
}