using DES_COGNITIVO_FUN_PROG.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    public class EventoController : Controller
    {
        private readonly GestionEventosContext _context; //esta es una clase que es el puente de la inyeccion de dependencias 


        public EventoController(GestionEventosContext context)
        {
            _context = context;
        }
        public async Task< IActionResult> Index()
        {
            var evento = _context.Eventos.Include(b=>b.IdClienteNavigation); //Usamos Include para pode realizar una union con la tabla clientes
            return View( await evento.ToListAsync());
        }
    }
}
