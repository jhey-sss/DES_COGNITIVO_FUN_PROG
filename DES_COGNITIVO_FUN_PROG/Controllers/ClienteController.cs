using AspNetCoreGeneratedDocument;
using DES_COGNITIVO_FUN_PROG.Models;
using DES_COGNITIVO_FUN_PROG.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Validation;

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
            //en la tabla Clientes, verifica si su propiedad Activo es verdadera y solo filtra por las que si son verdaderas
            var ClienteActivo = await _context.Clientes.Where(c =>c.Activo).ToListAsync(); 
            return View(ClienteActivo); // aqui se manda los clientes de la base de datos a la vista index de este controlador
        }

        // aqui la primera funcion retorna con get para mostrar la vista vacia al cliente
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


                await _context.SaveChangesAsync();

                // aqui guardamos los clientes en la BD que anteriormente cargados en entity framework
                //tengamos en cuenta que podemos realizar otro tipo de acciones antes de enviar los datos a la BD
                // ya que el metodo es asincrono 

                return RedirectToAction(nameof(Index)); //una vez finalizado me retorna al index que esta al principio
            
            }
            ViewData["VerClientes"] = new SelectList(_context.Clientes, "IdCliente", "Nombre",model.IdCliente);
            return View(model); // la vista esta vacia, pero que pasa si nuestro modelo manda error
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id) //El signo ? convierte un tipo de dato normal (en este caso un entero int) en un Tipo Anulable (Nullable Type).
        {
            if (id == null) return NotFound(); // validamos si el id que optenemos de la vista no es nulo
            //FildAsync se usa para buscar la clave foranea por ende el ID de toda la tabla
            var cliente = await _context.Clientes.FindAsync(id); // creamos una variable la cual guarda el context Clientes y coge el Id del cliente 
            if (cliente == null) return NotFound(); // Validamos que el cliente no sea nulo

            var model = new ClienteViewModel() //creamos una variable en base a la clase clienteviewmodel
            {
                // a estas nuevas variables les asignamos las tablas que vienen desde la base de datos
                IdCliente = cliente.IdCliente,
                NumeroIdentidad = cliente.NumeroIdentidad,
                Nombre = cliente.Nombre,
                Correo = cliente.Correo,
                Telefono = cliente.Telefono

            };
            return View(model);
            
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        // (int id, ClienteViewModel model) aqui se recoge el id de la url de la peticion del cliente, para hacer la modificacion
        // y la informacion se envia en el formato de ClienteViewModel, diamos que la peticion tiene un identificador
        // y el paquete es enviado en el formato del model
        public async Task<IActionResult> Edit(int id, ClienteViewModel model)
        {
            if (id != model.IdCliente) return NotFound();
            
            if ( ModelState.IsValid)
            {
                try
                {
                    //Cliente 
                    var cliente = await _context.Clientes.FindAsync(id); 
                    if (cliente == null) return NotFound();

                    //pasamos los datos nuevos al viewModel ala entidad de la bd
                    cliente.NumeroIdentidad = model.NumeroIdentidad;
                    cliente.Nombre = model.Nombre;
                    cliente.Correo = model.Correo;
                    cliente.Telefono = model.Telefono;

                    _context.Update(cliente);
                    

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(model.IdCliente)) return NotFound();
                    else throw;
                
                }
                return RedirectToAction (nameof(Index));

            }
            return View(model);

        }

        //para este metodo no vamos a borrar nada, unicamente hacemos un update a la tabla
        //no vamos a borrar nada, solo actualizar el esado de activo a falso
        //a traves de de update, una ves optenido los datos 
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null || !cliente.Activo) return NotFound();

            var model = new ClienteViewModel()
            {
                IdCliente = cliente.IdCliente,
                NumeroIdentidad = cliente.NumeroIdentidad,
                Nombre = cliente.Nombre,
                Correo = cliente.Correo,
                Telefono = cliente.Telefono
            };
            return View(model);

        }

        [HttpPost, ActionName("Delete") ]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Delete(int id, ClienteViewModel  model)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null) return NotFound();
                //en ves de usar _context.Remove(cliente), lo pasamos a false para que se borre logicamente
                cliente.Activo = false;

                //actualizamos la tabla
                _context.Clientes.Update(cliente);
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(model.IdCliente)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));

        }
        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.IdCliente == id);
        }

    }   
}
