using DES_COGNITIVO_FUN_PROG.Models;
using DES_COGNITIVO_FUN_PROG.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        // 1. Inyectamos la base de datos
        private readonly GestionEventosContext _context;

        public HomeController(GestionEventosContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 2. Calculamos las métricas
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var model = new DashboardViewModel
            {
                TotalClientesActivos = await _context.Clientes.CountAsync(c => c.Activo == true),
                TotalEventosActivos = await _context.Eventos.CountAsync(e => e.Activo == true),
                TotalInvitados = await _context.Invitados.CountAsync(i => i.Activo == true),
                EventosEsteMes = await _context.Eventos.CountAsync(e => e.Activo == true && e.Fecha.Month == hoy.Month && e.Fecha.Year == hoy.Year)
            };

            // 3. Enviamos el modelo a la vista
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}