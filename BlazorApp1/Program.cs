using BlazorApp1.Components;
using BlazorApp1.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registra el contexto de la base de datos
builder.Services.AddDbContext<LilaBoutiqueContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra los servicios lógicos para que Blazor pueda inyectarlos en las vistas (.razor)
builder.Services.AddScoped<InterAuthService, AuthService>();
builder.Services.AddScoped<InterInventarioService, InventarioService>();

builder.Services.AddAuthorizationCore();


// Configuración base de autenticación requerida por Blazor
builder.Services.AddAuthentication("Cookies").AddCookie(options =>
{
    // Le indicamos al sistema que nuestro login está en la raíz
    options.LoginPath = "/";
});
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

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

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
