using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using LilasBoutique.Models;
using LilasBoutique.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace LilasBoutique.Data
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        #region Inyección y Variables Privadas
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceScopeFactory _scopeFactory;

        public CustomAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor, IServiceScopeFactory scopeFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _scopeFactory = scopeFactory;
        }
        #endregion

        #region Métodos de Estado
        /// <summary>
        /// Método interceptor de Blazor. Verifica cada vez que un componente lo solicita
        /// si existe una Cookie válida del navegador y si el usuario sigue activo en Base de Datos.
        /// </summary>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsPrincipal principal;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.Request.Cookies.TryGetValue("lila_user", out var correo) == true && !string.IsNullOrEmpty(correo))
                {
                    // Scope independiente porque GetAuthenticationStateAsync puede chocar con transacciones DB activas
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<LilaBoutiqueContext>();

                    var usuario = await context.Usuario
                        .Include(u => u.Rol)
                        .FirstOrDefaultAsync(u => u.Correo == correo && u.EstadoActivo);

                    if (usuario != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                            new Claim(ClaimTypes.Name, usuario.Correo),
                            new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "")
                        };

                        var identity = new ClaimsIdentity(claims, "CustomAuth");
                        principal = new ClaimsPrincipal(identity);
                        return new AuthenticationState(principal);
                    }
                }
            }
            catch
            {
                // Si ocurre un error de base de datos o de contexto HTTP, el sistema por seguridad asume usuario Anónimo
            }

            principal = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(principal);
        }

        /// <summary>
        /// Informa inmediatamente a Blazor que un usuario ha iniciado o cerrado sesión 
        /// para repintar la UI (Mostrar/Ocultar Menús laterales).
        /// </summary>
        public Task ActualizarEstadoAutenticacion(Usuario? usuario)
        {
            ClaimsPrincipal principal;

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "")
                };

                var identity = new ClaimsIdentity(claims, "CustomAuth");
                principal = new ClaimsPrincipal(identity);
            }
            else
            {
                principal = new ClaimsPrincipal(new ClaimsIdentity());
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
            return Task.CompletedTask;
        }
        #endregion
    }
}