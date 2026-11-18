using DES_COGNITIVO_FUN_PROG.Models;
using DES_COGNITIVO_FUN_PROG.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Diagnostics.CodeAnalysis;

namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventoController : Controller
    {
        private readonly GestionEventosContext _context; //esta es una clase que es el puente de la inyeccion de dependencias 

        public EventoController(GestionEventosContext context)
        {
            _context = context;
        }

        //vamos a crear un evento controller, el cual va a pertenecer a un cliente, ahora bien, podemos;
        //crear eventos que pertenezcan a un cliente
        //editar el evento que pertenezca a un cliente
        //eliminar el evento que pertenezca a un cliente
        //barra de busqueda de un evento 
        public async Task<IActionResult> Index(string buscar)
        {
            // Usamos Include para poder realizar una union con la tabla clientes y estados
            // Filtramos para que solo muestre los eventos activos (borrado lógico)
            
            //agregamos esta variable sin el envio de la informacion aun, enviaremos la peticion luego de realizar el resto de consultas
            var eventos =  _context.Eventos
                .Include(b => b.IdClienteNavigation)
                .Where(e => e.Activo == true);


                //var clientesActivos = await _context.Clientes // Origen (EF Core)
                //.Where(c => c.EstaActivo)                 // Prepara el filtro SQL (LINQ)
                //.OrderBy(c => c.Nombre)                   // Prepara el ordenamiento (LINQ)
                //.ToListAsync();                           // Ejecuta el SQL y trae la lista (EF Core Async)




            //validamos que el dato que nos retorna no sea nulo osea que si tenga algun dato y que este sea mayor a 0
            //if (buscar.HasValue && buscar.Value > 0 ){

            //    // a los eventos, le realizamos una comparacion total, si es que estan en la bd y de ahi enviamos la coincidencia unica
            //    eventos = eventos.Where(e=> e.IdEvento == buscar.Value);

            //}

            if (!string.IsNullOrEmpty(buscar))
            {
                //buscamos el codigo del evento en la bd como string
                eventos = eventos.Where(e => e.CodigoEvento.Contains(buscar));


            }
            ViewData["BusquedaEventos"] = buscar;

            //el metodo tolistasync pide en una lista los datos que se pide a la bd, se pide al final luego de empaquetar todo lo que deceamos pedir
            var BuscarEvetos = await eventos.ToListAsync();

            return View(BuscarEvetos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Cargamos las listas desplegables (dropdowns) para la vista Create
            ViewData["IdCliente"] = new SelectList(_context.Clientes.Where(c => c.Activo == true), "IdCliente", "Nombre");
            ViewData["IdEstado"] = new SelectList(_context.EstadoEventos.Where(e => e.Activo == true), "IdEstado", "EstadoEvento1");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventoViewModel model)
        {
            //-----------------------------------------------------------------------
            // este bloque sirve, para generar los codigos de los eventos a partir de un parametro grande de numeros
            // esto con la finalidad de mejorar la busqueda de eventos, a traves de numeros directamete

            long nuevoCodigoNum = 10000000; // Valor inicial por defecto

            // Buscamos el evento más reciente en la base de datos
            var ultimoEvento = await _context.Eventos
                .OrderByDescending(e => e.IdEvento)
                .FirstOrDefaultAsync();

            // Si ya existen eventos, intentamos convertir su código a número y sumarle 1
            if (ultimoEvento != null && long.TryParse(ultimoEvento.CodigoEvento, out long ultimoCodigoDB))
            {
                nuevoCodigoNum = ultimoCodigoDB + 1;
            }

            // Le asignamos el nuevo código al modelo como string
            model.CodigoEvento = nuevoCodigoNum.ToString();

            // Como el código ya no lo escribe el usuario, quitamos la validación obligatoria del ModelState
            ModelState.Remove("CodigoEvento");
            // fin del bloque 
            // -------------------------------------------------------------------------

            if (ModelState.IsValid)
            {
                var evento = new Evento()
                {

                    IdCliente = model.IdCliente.Value, // Agregado .Value
                    IdEstado = model.IdEstado.Value,
                    Nombre = model.Nombre,
                    Presupuesto = model.Presupuesto.Value,
                    Fecha = model.Fecha.Value,
                    Lugar = model.Lugar,
                    CodigoEvento = model.CodigoEvento,
                    Descripcion = model.Descripcion,
                    Activo = true
                    
                };

                //agregamoss al evento los datos optenidos del formulario entregado al cliete
                _context.Add(evento);

                //guardamos los datos igualados del formulario
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Si falla la validación, recargamos los SelectList
            // 1. En Cliente validamos Activo == true
            ViewData["IdCliente"] = new SelectList(_context.Clientes.Where(c => c.Activo == true), "IdCliente", "Nombre", model.IdCliente);
            // 2. En EstadoEvento validamos Activo == true, y usamos "EstadoEvento1" en lugar de "EstadoEvento"
            ViewData["IdEstado"] = new SelectList(_context.EstadoEventos.Where(e => e.Activo == true), "IdEstado", "EstadoEvento1", model.IdEstado);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null || evento.Activo != true) return NotFound();

            var model = new EventoViewModel()
            {
                IdEvento = evento.IdEvento,
                IdCliente = evento.IdCliente,
                IdEstado = evento.IdEstado,
                Nombre = evento.Nombre,
                Presupuesto = evento.Presupuesto,
                Fecha = evento.Fecha,
                Lugar = evento.Lugar,
                CodigoEvento = evento.CodigoEvento
            };

            // Cargamos las listas desplegables con el valor actual seleccionado
            ViewData["IdCliente"] = new SelectList(_context.Clientes.Where(c => c.Activo == true), "IdCliente", "Nombre", model.IdCliente);
            ViewData["IdEstado"] = new SelectList(_context.EstadoEventos.Where(e => e.Activo == true), "IdEstado", "EstadoEvento1", model.IdEstado);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventoViewModel model)
        {
            if (id != model.IdEvento) return NotFound();

            // Quitamos la validación del CodigoEvento en la edición si está en el modelo
            ModelState.Remove("CodigoEvento");

            if (ModelState.IsValid)
            {
                try
                {
                    var evento = await _context.Eventos.FindAsync(id);
                    if (evento == null) return NotFound();

                    evento.IdCliente = model.IdCliente.Value; 
                    evento.IdEstado = model.IdEstado.Value;   // Agregado .Value
                    evento.Nombre = model.Nombre;
                    evento.Presupuesto = model.Presupuesto.Value; 
                    evento.Fecha = model.Fecha.Value;             
                    evento.Lugar = model.Lugar;

                    _context.Update(evento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventoExists(model.IdEvento)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Si hay error, recargamos los SelectList
            ViewData["IdCliente"] = new SelectList(_context.Clientes.Where(c => c.Activo == true), "IdCliente", "Nombre", model.IdCliente);
            ViewData["IdEstado"] = new SelectList(_context.EstadoEventos.Where(e => e.Activo == true), "IdEstado", "EstadoEvento1", model.IdEstado);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // Buscamos el evento por su ID
            var evento = await _context.Eventos.FindAsync(id);

            // Validamos que exista y que esté activo (que no haya sido borrado lógicamente antes)
            if (evento == null || evento.Activo != true) return NotFound();

            // Mapeamos los datos reales del Evento hacia el EventoViewModel
            var model = new EventoViewModel()
            {
                IdEvento = evento.IdEvento,
                IdCliente = evento.IdCliente,
                IdEstado = evento.IdEstado,
                Nombre = evento.Nombre,
                Descripcion = evento.Descripcion,
                Presupuesto = evento.Presupuesto,
                Fecha = evento.Fecha,
                Lugar = evento.Lugar,
                CodigoEvento = evento.CodigoEvento
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, EventoViewModel model)
        {
            try
            {
                var evento = await _context.Eventos.FindAsync(id);
                if (evento == null) return NotFound();

                // En vez de usar _context.Remove(evento), lo pasamos a false para el borrado lógico
                evento.Activo = false;

                // Actualizamos la tabla
                _context.Eventos.Update(evento);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Usamos el método EventoExists corregido
                if (!EventoExists(model.IdEvento)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventoExists(int id)
        {
            // Corregido: Ahora busca en _context.Eventos evaluando el IdEvento
            return _context.Eventos.Any(e => e.IdEvento == id);
        }
    }
}

