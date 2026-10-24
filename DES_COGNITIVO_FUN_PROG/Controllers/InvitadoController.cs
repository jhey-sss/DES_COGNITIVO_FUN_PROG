using Microsoft.AspNetCore.Mvc;
using DES_COGNITIVO_FUN_PROG.Models;
using DES_COGNITIVO_FUN_PROG.Models.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    public class InvitadoController : Controller
    {
        // Datos de prueba compartidos entre las solicitudes web.

        private readonly GestionEventosContext _context; // aqui se define la conexion entre la inyeccion de dependencias y este constructor

        public InvitadoController(GestionEventosContext Context)
        {
            _context = Context;
        }


        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }

        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public IActionResult Index(string codigo)
        //        {
        //            ViewBag.CodigoEvento = CodigoEvento;
        //            ViewBag.NombreEvento = NombreEvento;

        //            if (string.IsNullOrWhiteSpace(codigo))
        //            {
        //                ViewBag.Mensaje = "Ingresa tu código de invitado.";
        //                return View();
        //            }

        //            Invitado? invitado = registro.BuscarInvitado(codigo);

        //            if (invitado == null)
        //            {
        //                ViewBag.Mensaje =
        //                    "El código ingresado no es válido.";
        //                return View();
        //            }

        //            ViewBag.Invitado = invitado;
        //            return View();
        //        }

        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public IActionResult Responder(
        //            string codigo, string respuesta, int acompanantes = 0)
        //        {
        //            ViewBag.CodigoEvento = CodigoEvento;
        //            ViewBag.NombreEvento = NombreEvento;

        //            Invitado? invitado = registro.BuscarInvitado(codigo);

        //            if (invitado == null)
        //            {
        //                ViewBag.Mensaje = "El código de invitado no es válido.";
        //                return View("Index");
        //            }

        //            if (respuesta != "S" && respuesta != "N")
        //            {
        //                ViewBag.Mensaje = "Selecciona si asistirás o no.";
        //                ViewBag.Invitado = invitado;
        //                return View("Index");
        //            }

        //            bool asistira = respuesta == "S";

        //            bool guardado = registro.RegistrarRespuesta(
        //                invitado, asistira, acompanantes, out string mensaje);

        //            ViewBag.Mensaje = mensaje;
        //            ViewBag.Invitado = invitado;
        //            ViewBag.Guardado = guardado;

        //            return View("Index");
        //        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                ViewBag.Mensaje = "Ingresa tu código de invitado (DNI).";
                return View();
            }

            // Buscamos el invitado en la base de datos usando el DNI como código
            var invitadoDb = await _context.Invitados
                .FirstOrDefaultAsync(i => i.Dni == codigo);

            if (invitadoDb == null)
            {
                ViewBag.Mensaje = "El código ingresado no es válido.";
                return View();
            }

            // Mapeamos el modelo de la Base de Datos al ViewModel
            var viewModel = new InvitadoViewModel
            {
                IdInvitado = invitadoDb.IdInvitado,
                IdEvento = invitadoDb.IdEvento,
                IdMesa = invitadoDb.IdMesa,
                Nombres = invitadoDb.Nombres,
                Dni = invitadoDb.Dni,
                Confirmado = invitadoDb.Confirmado,
                NumAcompañantes = invitadoDb.NumAcompañantes,
                FechaConfirmacion = invitadoDb.FechaConfirmacion
            };

            // Opcional: Si necesitas cargar datos del evento relacionado para tus ViewBags
            var evento = await _context.Eventos.FindAsync(viewModel.IdEvento);
            if (evento != null)
            {
                ViewBag.CodigoEvento = evento.IdEvento; // o el campo código que uses
                ViewBag.NombreEvento = evento.Nombre;  // asumiendo que tiene la propiedad Nombre
            }

            ViewBag.Invitado = viewModel;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Responder(string codigo, string respuesta, int acompanantes = 0)
        {
            if (respuesta != "S" && respuesta != "N")
            {
                ViewBag.Mensaje = "Selecciona si asistirás o no.";
                return RedirectToAction(nameof(Index));
            }

            // Buscamos al invitado en la base de datos
            var invitadoDb = await _context.Invitados
                .FirstOrDefaultAsync(i => i.Dni == codigo);

            if (invitadoDb == null)
            {
                ViewBag.Mensaje = "El código de invitado no es válido.";
                return View("Index");
            }

            try
            {
                // Actualizamos las propiedades del registro de la base de datos
                invitadoDb.Confirmado = (respuesta == "S");
                invitadoDb.NumAcompañantes = acompanantes;
                invitadoDb.FechaConfirmacion = DateOnly.FromDateTime(DateTime.Now);

                // Guardamos los cambios de manera asíncrona en la base de datos
                _context.Update(invitadoDb);
                await _context.SaveChangesAsync();

                ViewBag.Mensaje = respuesta == "S"
                    ? "¡Confirmaste tu asistencia con éxito!"
                    : "Has declinado la invitación.";
                ViewBag.Guardado = true;
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Ocurrió un error al guardar tu respuesta de confirmación.";
                ViewBag.Guardado = false;
            }

            // Volvemos a mapear al ViewModel para mantener la información en la vista externa
            var viewModel = new InvitadoViewModel
            {
                IdInvitado = invitadoDb.IdInvitado,
                IdEvento = invitadoDb.IdEvento,
                IdMesa = invitadoDb.IdMesa,
                Nombres = invitadoDb.Nombres,
                Dni = invitadoDb.Dni,
                Confirmado = invitadoDb.Confirmado,
                NumAcompañantes = invitadoDb.NumAcompañantes,
                FechaConfirmacion = invitadoDb.FechaConfirmacion
            };

            ViewBag.Invitado = viewModel;
            return View("Index");
        }
    }
}
