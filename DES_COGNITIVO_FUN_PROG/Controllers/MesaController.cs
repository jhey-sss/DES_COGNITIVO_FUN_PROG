using DES_COGNITIVO_FUN_PROG.Models;
using Microsoft.AspNetCore.Mvc;

namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    public class MesaController : Controller
    {
        private readonly GestionEventosContext _context; // aqui se define la conexion entre la inyeccion de dependencias y este constructor

        public MesaController(GestionEventosContext Context)
        {
            _context = Context;
        }
        public IActionResult Index()
        {
            return View(_context.Mesas);

        }
    }
}

