using LilasBoutique.Components;
using LilasBoutique.Data;
using LilasBoutique.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
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

// 1. Registrar Autenticación por Cookies (Requisito de .NET 8)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/"; // Si no está logueado, lo manda aquí
        options.AccessDeniedPath = "/";
    });

// 2. Registrar Autorización global
builder.Services.AddAuthorization();

// Registramos el CustomAuthenticationStateProvider
builder.Services.AddScoped<CustomAuthenticationStateProvider>();

// Le indicamos a Blazor que use nuestro Custom provider cuando pida un AuthenticationStateProvider genérico
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode().DisableAntiforgery();

builder.Services.AddAntiforgery();

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

    // 2. Crear el usuario Administrador por defecto si no hay usuarios
    if (!context.Usuario.Any())
    {
        var rolAdmin = context.Rol.FirstOrDefault(r => r.NombreRol == "Administrador");

        if (rolAdmin != null)
        {
            context.Usuario.Add(new Usuario
            {
                Correo = "admin@lilasboutique.com",
                // Encriptamos la contraseña "admin123" usando BCrypt directamente
                HashCredencial = BCrypt.Net.BCrypt.HashPassword("admin123"),
                EstadoActivo = true,
                RolId = rolAdmin.IdRol
            });
            context.SaveChanges();
        }
    }
}
// ==========================================

app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.Run();
