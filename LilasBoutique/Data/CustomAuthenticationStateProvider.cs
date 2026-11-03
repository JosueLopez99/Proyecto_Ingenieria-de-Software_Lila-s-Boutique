using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using LilasBoutique.Models;

namespace LilasBoutique.Data
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _usuarioActual = new(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_usuarioActual));
        }

        public Task ActualizarEstadoAutenticacion(Usuario? usuario)
        {
            if (usuario != null)
            {
                // Aquí definimos los Claims. El NameIdentifier es el que lee Configuracion.razor
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.Rol.NombreRol)
                };

                var identity = new ClaimsIdentity(claims, "CustomAuth");
                _usuarioActual = new ClaimsPrincipal(identity);
            }
            else
            {
                // Lógica para cerrar sesión
                _usuarioActual = new ClaimsPrincipal(new ClaimsIdentity());
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_usuarioActual)));
            return Task.CompletedTask;
        }
    }
}