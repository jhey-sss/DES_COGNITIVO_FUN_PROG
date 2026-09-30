using DES_COGNITIVO_FUN_PROG.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        //Creamos un nuevo metodo el cual lo usaremos para crear datos
        //public IActionResult Create() //agregamos una nueva vista con anticlick, agregar vista y crear vista en blanco
        //{
        //    // SelectList recibe 4  parametros (fuente de informacion, de que parte de la fuente lo optiene, que muestra de esa fuente)
        //    //  creamos un nuevo metodo a partir del constructor de SelecList
        //    ViewData["GestionEventos"] = new SelectList(_context.Eventos, "IdEvento", "Nombre");
        //    return View();
        //}

        //[HttpPost]
        //public IActionResult Create() //agregamos una nueva vista con anticlick, agregar vista y crear vista en blanco
        //{
        //    // SelectList recibe 4  parametros (fuente de informacion, de que parte de la fuente lo optiene, que muestra de esa fuente)
        //    //  creamos un nuevo metodo a partir del constructor de SelecList
        //    ViewData["GestionEventos"] = new SelectList(_context.Eventos, "IdEvento", "Nombre");
        //    return View();
        //}
    }
}
