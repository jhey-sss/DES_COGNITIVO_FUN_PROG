using DES_COGNITIVO_FUN_PROG.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    public class WorkspaceController : Controller
    {
        private readonly GestionEventosContext _context; //esta es una clase que es el puente de la inyeccion de dependencias 

        public WorkspaceController(GestionEventosContext Context)  // este es el constructor que guarda la conexion de Context en una variable local llamada _context
        { 
        _context = Context;
        
        }//Esta es la inyeccion de dependencia del controlador
        public async Task<IActionResult> Index() // esta es una clase asincronica que usa task para traer un bloque da informacion
            // esta clase se conecta on index el cual es una vista
        {
            //aqui se retorna los datos a la vista a traves de la conexion _context con una lista asincronica. el cual devuelve la tabla Clientes
            return View(await _context.Clientes.ToListAsync()); 
        }
    }
}
