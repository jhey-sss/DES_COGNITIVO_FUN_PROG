using DES_COGNITIVO_FUN_PROG.Models;
using DES_COGNITIVO_FUN_PROG.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    [Authorize(Roles = "Admin")]
    [Authorize]
    public class AdministradorController : Controller
    {
        
        private readonly GestionEventosContext _Context;
        
        public AdministradorController(GestionEventosContext context)
        {
            _Context = context;
        }
        
        public async Task<IActionResult> Index()
        {

            return RedirectToAction("Index", "Home"); // la vista vacia me envia directamente al home principal
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            // Si ya está logueado como Admin, lo mandamos directo al Index
            if (User.IsInRole("Admin")) return RedirectToAction("Index");

            return View(); // Esto busca y renderiza la vista Login.cshtml
        }
        [HttpPost]
        [AllowAnonymous] //cualquiera de fuera puede acceder a este recurso
        public async Task<IActionResult> Login(AdministradorViewModel model)
        {
            var admin = await _Context.Administradors. //pide los datos de la bd
                FirstOrDefaultAsync(l => l.Usuario == model.Usuario && l.ContraseñaHash == model.ContraseñaHash); //compara los datos

            if (admin != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,admin.Usuario),
                    new Claim(ClaimTypes.Role,"Admin")
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos");
            return View(model);
        }

        public async Task<IActionResult> Create_Admin(AdministradorViewModel model)
        { 
            if (ModelState.IsValid) 
            {
                var Admin = new Administrador()
                {
                    Usuario = model.Usuario,
                    ContraseñaHash = model.ContraseñaHash
                };

                _Context.Add(Admin);

                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");                
            }
            
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");

        }



    }
}
