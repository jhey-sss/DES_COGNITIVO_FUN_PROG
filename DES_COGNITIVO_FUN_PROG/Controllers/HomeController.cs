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
            // enviar cambios 11,,1,1,1,,1
            // armamos el tablero de estados, una columna por cada estado de evento
            // traemos solo los datos necesarios de cada evento activo, junto con el conteo de sus invitados
            var eventosTablero = await _context.Eventos
                .Where(e => e.Activo == true)
                .OrderBy(e => e.Fecha)
                .Select(e => new EventoTableroViewModel
                {
                    IdEvento = e.IdEvento,
                    IdEstado = e.IdEstado,
                    CodigoEvento = e.CodigoEvento,
                    Nombre = e.Nombre,
                    Fecha = e.Fecha,
                    Lugar = e.Lugar,
                    NombreCliente = e.IdClienteNavigation.Nombre,
                    TotalInvitados = e.Invitados.Count(i => i.Activo == true),
                    TotalConfirmados = e.Invitados.Count(i => i.Activo == true && i.Confirmado == true)
                })
                .ToListAsync();

            var estados = await _context.EstadoEventos.OrderBy(s => s.IdEstado).ToListAsync();

            // mostramos los estados activos, y tambien cualquier estado inactivo que aun tenga eventos, para que ningun evento desaparezca del tablero
            model.ColumnasEstados = estados
                .Where(s => s.Activo == true || eventosTablero.Any(e => e.IdEstado == s.IdEstado))
                .Select(s => new ColumnaEstadoViewModel
                {
                    IdEstado = s.IdEstado,
                    NombreEstado = s.EstadoEvento1,
                    Eventos = eventosTablero.Where(e => e.IdEstado == s.IdEstado).ToList()
                })
                .ToList();

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