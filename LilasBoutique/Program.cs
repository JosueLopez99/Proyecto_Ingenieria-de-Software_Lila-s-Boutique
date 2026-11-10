using LilasBoutique.Components;
using LilasBoutique.Data;
using LilasBoutique.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region 1. Configuración de Componentes y Base de Datos
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registro del DbContext (EF Core)
builder.Services.AddDbContext<LilaBoutiqueContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

#region 2. Inyección de Dependencias (Servicios Scoped)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IBodegaService, BodegaService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
#endregion

#region 3. Autenticación, Autorización y Estado (RBAC)
// 3.1. Contexto HTTP necesario para leer/escribir cookies de sesión al recargar página (F5)
builder.Services.AddHttpContextAccessor();

// 3.2. Configuración de autenticación por Cookies nativa de ASP.NET Core
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// 3.3. Proveedor de autenticación personalizado (Conecta Blazor SignalR con las Cookies)
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// 3.4. Autorización y cascada de estado
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
#endregion

#region 4. Configuraciones Adicionales
// Detalles de errores para Blazor Server 
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
});

// Antiforgery configurado en la fase de servicios
builder.Services.AddAntiforgery();
#endregion

var app = builder.Build();

#region 5. Pipeline de Solicitudes HTTP (Middlewares)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Importante!: Orden estricto para procesar la seguridad
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode().DisableAntiforgery();
#endregion

#region 6. Sembrado de Datos (Data Seeding)
// Crea un Scope para insertar Roles y Usuarios básicos si la base de datos está vacía.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LilaBoutiqueContext>();

    // Crear Roles Iniciales
    if (!context.Rol.Any())
    {
        context.Rol.AddRange(
            new Rol { NombreRol = "Administrador", NivelPrivilegio = 1 },
            new Rol { NombreRol = "Vendedor", NivelPrivilegio = 2 },
            new Rol { NombreRol = "Bodeguero", NivelPrivilegio = 3 }
        );
        context.SaveChanges();
    }

    var rolAdmin = context.Rol.FirstOrDefault(r => r.NombreRol == "Administrador");
    var rolVendedor = context.Rol.FirstOrDefault(r => r.NombreRol == "Vendedor");
    var rolBodeguero = context.Rol.FirstOrDefault(r => r.NombreRol == "Bodeguero");

    var nuevosUsuarios = new List<Usuario>();

    // Sembrar Administrador Default
    if (rolAdmin != null && !context.Usuario.Any(u => u.Correo == "admin@lilasboutique.com"))
    {
        nuevosUsuarios.Add(new Usuario
        {
            Correo = "admin@lilasboutique.com",
            HashCredencial = BCrypt.Net.BCrypt.HashPassword("admin123"),
            EstadoActivo = true,
            RolId = rolAdmin.IdRol
        });
    }

    // Sembrar Vendedor Default
    if (rolVendedor != null && !context.Usuario.Any(u => u.Correo == "vendedor@lilasboutique.com"))
    {
        nuevosUsuarios.Add(new Usuario
        {
            Correo = "vendedor@lilasboutique.com",
            HashCredencial = BCrypt.Net.BCrypt.HashPassword("vendedor123"),
            EstadoActivo = true,
            RolId = rolVendedor.IdRol
        });
    }

    // Sembrar Bodeguero Default
    if (rolBodeguero != null && !context.Usuario.Any(u => u.Correo == "bodeguero@lilasboutique.com"))
    {
        nuevosUsuarios.Add(new Usuario
        {
            Correo = "bodeguero@lilasboutique.com",
            HashCredencial = BCrypt.Net.BCrypt.HashPassword("bodeguero123"),
            EstadoActivo = true,
            RolId = rolBodeguero.IdRol
        });
    }

    // Insertar en Base de Datos
    if (nuevosUsuarios.Any())
    {
        context.Usuario.AddRange(nuevosUsuarios);
        context.SaveChanges();
    }
}
#endregion

app.Run();