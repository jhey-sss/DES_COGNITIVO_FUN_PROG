using DES_COGNITIVO_FUN_PROG.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<GestionEventosContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("EventosContext"));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";  //ruta para el login del usuario comun
        options.AccessDeniedPath = "/Administrador/Login";// Si un usuario común intenta entrar al admin, lo rebota aquí

    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); //valida quien es el usuario
app.UseAuthorization(); //valida el rol

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Administrador}/{action=Login}/{id?}") //define la ruta a donde apunta por defecto
    .WithStaticAssets();


app.Run();

