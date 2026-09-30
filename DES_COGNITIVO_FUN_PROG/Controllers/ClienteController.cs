using DES_COGNITIVO_FUN_PROG.Models;
using DES_COGNITIVO_FUN_PROG.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    public class ClienteController : Controller
    {
        private readonly GestionEventosContext _context; // aqui se define la conexion entre la inyeccion de dependencias y este constructor

        public ClienteController(GestionEventosContext Context) 
        {
            _context = Context;
        }
        public async Task<IActionResult> Index() // una funcion publica que envia a traves de IActionsResult via GET por defecto a Index de la vista
        {

            return View( await _context.Clientes.ToListAsync()); // aqui se manda los clientes de la base de datos a la vista index de este controlador
        }

        public IActionResult Create() // este metodo no envia modelos  y se comunica por GET
        {
            ViewData["VerClientes"] = new SelectList(_context.Clientes, "IdCliente", "Nombre");
            return View(); // la vista esta vacia, pero que pasa si nuestro modelo manda error
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClienteViewModel model) // este metodo no envia modelos  y se comunica por POST
        {
            if (ModelState.IsValid) //valisamos que las validaciones [ValidateAntiForgeryToken] esten bien si no, regresa la vista
            {
                var cliente = new Cliente() // creamos un obgeto de Cliente() (el espejo de la tabla en la BD)
                {
                    // Son todos los datos que se muestran en el formulario y que pasaron por las ViewModels
                    //IdCliente = model.IdCliente, no mandamos el ID y dejamos que la BD lo asigne
                    NumeroIdentidad = model.NumeroIdentidad,
                    Nombre = model.Nombre,
                    Correo = model.Correo,
                    Telefono = model.Telefono

                };
                _context.Add(cliente); //aqui guardamos los datos en entity framework

                var t = _context.SaveChangesAsync();
                await t;// aqui guardamos los clientes en la BD que anteriormente cargados en entity framework
                //tengamos en cuenta que podemos realizar otro tipo de acciones antes de enviar los datos a la BD
                // ya que el metodo es asincrono 

                return RedirectToAction(nameof(Index)); //una vez finalizado me retorna al index que esta al principio
            
            }
            ViewData["VerClientes"] = new SelectList(_context.Clientes, "IdCliente", "Nombre",model.IdCliente);
            return View(); // la vista esta vacia, pero que pasa si nuestro modelo manda error
        }
    }
}
