using LilasBoutique.Components;
using LilasBoutique.Data;
using LilasBoutique.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registro del DbContext
builder.Services.AddDbContext<LilaBoutiqueContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de la capa lógica (Servicios)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IBodegaService, BodegaService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// 1. Registramos tu proveedor de autenticación personalizado
builder.Services.AddScoped<CustomAuthenticationStateProvider>();

// 2. Le indicamos a Blazor que el AuthenticationStateProvider global ES tu proveedor personalizado
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// 3. Activamos la autorización nativa para componentes de Blazor
builder.Services.AddAuthorizationCore();

// 4. Configuraciones de Blazor Server
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
});

// 5. Antiforgery configurado en la fase de servicios (ANTES del builder.Build)
builder.Services.AddAntiforgery();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode().DisableAntiforgery();

// ==========================================
// SEMBRADO DE DATOS (Data Seeding)
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LilaBoutiqueContext>();

    // 1. Crear los roles si no existen
    if (!context.Rol.Any())
    {
        context.Rol.AddRange(
            new Rol { NombreRol = "Administrador", NivelPrivilegio = 1 },
            new Rol { NombreRol = "Vendedor", NivelPrivilegio = 2 },
            new Rol { NombreRol = "Bodeguero", NivelPrivilegio = 3 }
        );
        context.SaveChanges();
    }

    // 2. Comprobar y sembrar los roles individuales de manera segura
    var rolAdmin = context.Rol.FirstOrDefault(r => r.NombreRol == "Administrador");
    var rolVendedor = context.Rol.FirstOrDefault(r => r.NombreRol == "Vendedor");
    var rolBodeguero = context.Rol.FirstOrDefault(r => r.NombreRol == "Bodeguero");

    var nuevosUsuarios = new List<Usuario>();

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

    if (nuevosUsuarios.Any())
    {
        context.Usuario.AddRange(nuevosUsuarios);
        context.SaveChanges();
    }
}
// ==========================================

app.Run();