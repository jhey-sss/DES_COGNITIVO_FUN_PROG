using ClosedXML.Excel;
using DES_COGNITIVO_FUN_PROG.Models;
using DES_COGNITIVO_FUN_PROG.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InvitadoController : Controller
    {
        private readonly GestionEventosContext _context;

        public InvitadoController(GestionEventosContext context)
        {
            _context = context;
        }

        // PARTE 1: MÓDULO DEL ADMINISTRADOR (CRUD)
        // aqui solo puede acceder el aministrador para manejar el crud completo

        // GET: Invitado
        // ahora el index no muestra todos los invitados mezclados, sino la lista de eventos con el resumen de sus invitados
        // al presionar un evento se va a la lista de invitados unicamente de ese evento (PorEvento)
        public async Task<IActionResult> Index(string? buscar)
        {
            // solo eventos activos (borrado logico), la consulta aun no se envia a la bd
            var eventos = _context.Eventos.Where(e => e.Activo == true);

            //buscamos por codigo o por nombre del evento
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();
                eventos = eventos.Where(e => e.CodigoEvento.Contains(buscar) || e.Nombre.Contains(buscar));
            }

            ViewData["BusquedaInvitado"] = buscar;

            // armamos el resumen de cada evento con el conteo de sus invitados activos y confirmados
            var resumenEventos = await eventos
                .OrderBy(e => e.Fecha)
                .Select(e => new EventoInvitadosResumenViewModel
                {
                    IdEvento = e.IdEvento,
                    CodigoEvento = e.CodigoEvento,
                    Nombre = e.Nombre,
                    Fecha = e.Fecha,
                    Lugar = e.Lugar,
                    NombreEstado = e.IdEstadoNavigation.EstadoEvento1,
                    NombreCliente = e.IdClienteNavigation.Nombre,
                    TotalInvitados = e.Invitados.Count(i => i.Activo == true),
                    TotalConfirmados = e.Invitados.Count(i => i.Activo == true && i.Confirmado == true)
                })
                .ToListAsync();

            return View(resumenEventos);
        }

        // GET: Invitado/PorEvento?idEvento=1
        // lista unicamente los invitados de un evento, desde aqui se hace el crud completo, se exporta a excel y se abre el formulario del cliente
        public async Task<IActionResult> PorEvento(int? idEvento, string? buscar)
        {
            if (idEvento == null) return NotFound();

            var evento = await _context.Eventos
                .Include(e => e.IdClienteNavigation)
                .Include(e => e.IdEstadoNavigation)
                .FirstOrDefaultAsync(e => e.IdEvento == idEvento && e.Activo == true);

            if (evento == null) return NotFound();

            // traemos todos los invitados activos del evento, de aqui salen los contadores generales
            var todosLosInvitados = await _context.Invitados
                .Include(i => i.IdMesaNavigation)
                .Where(i => i.IdEvento == idEvento && i.Activo == true)
                .OrderBy(i => i.Nombres)
                .ToListAsync();

            var confirmados = todosLosInvitados.Where(i => i.Confirmado).ToList();

            var model = new InvitadosPorEventoViewModel
            {
                IdEvento = evento.IdEvento,
                CodigoEvento = evento.CodigoEvento,
                Nombre = evento.Nombre,
                Fecha = evento.Fecha,
                Lugar = evento.Lugar,
                NombreEstado = evento.IdEstadoNavigation.EstadoEvento1,
                NombreCliente = evento.IdClienteNavigation.Nombre,
                TotalInvitados = todosLosInvitados.Count,
                TotalConfirmados = confirmados.Count,
                TotalPendientes = todosLosInvitados.Count - confirmados.Count,
                PersonasConfirmadas = confirmados.Count + confirmados.Sum(i => i.NumAcompañantes ?? 0),
                Busqueda = buscar,
                // la barra de busqueda solo filtra la tabla, los contadores de arriba se mantienen
                Invitados = AplicarBusqueda(todosLosInvitados, buscar)
            };

            return View(model);
        }

        // filtro de busqueda compartido entre la lista del evento y el excel, busca por dni o por nombre
        private static List<Invitado> AplicarBusqueda(List<Invitado> invitados, string? buscar)
        {
            if (string.IsNullOrWhiteSpace(buscar)) return invitados;

            buscar = buscar.Trim();
            return invitados
                .Where(i => i.Dni.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                         || i.Nombres.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }




        //Exportar en exel un reporte de los clientes de los eventos
        public async Task<IActionResult> ExportarExel(string? buscar)
        {
            //aplicamos el filtro del index
            var consulta = _context.Invitados.Where(a => a.Activo);
            if (!string.IsNullOrEmpty(buscar))
            {
                consulta = consulta.Where(c => c.Dni.Contains(buscar));

            }

            //gaurdamos en un viewdata, el texto buscado para que no se pierda en la vista
            ViewData["BusquedaActual"] = buscar;

            //enviamos la peticion a la base de datos a traves del metodo asincronico implementado
            var InvitadosFiltrados = await consulta.ToListAsync();


            //aqui creamos el archivo

            using (var wb = new XLWorkbook())
            {
                var worksheet = wb.Worksheets.Add("Invitado");
                // ls encabezados

                worksheet.Cell(1, 1).Value = "Numero Evento";
                worksheet.Cell(1, 2).Value = "Numero de mesa";
                worksheet.Cell(1, 3).Value = "Nombre";
                worksheet.Cell(1, 4).Value = "DNI";
                worksheet.Cell(1, 5).Value = "confirmacion";
                worksheet.Cell(1, 6).Value = "Numero acompañantes";
                worksheet.Cell(1, 7).Value = "fecha de confirmacion";
                worksheet.Cell(1, 8).Value = "correo";
                worksheet.Cell(1, 9).Value = "telefono";
                worksheet.Cell(1, 10).Value = "Comentario";


                //definimos el rango a estalizar
                var headerRow = worksheet.Range("A1:J1");
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                // llenado de los datos causa
                int fila = 2; //en la primera fila esta el encabezado entonces desde el segundo se van agregando los datos
                foreach (var invitado in InvitadosFiltrados)
                {
                    worksheet.Cell(fila, 1).Value = invitado.IdEvento;
                    worksheet.Cell(fila, 2).Value = invitado.IdMesa;
                    worksheet.Cell(fila, 3).Value = invitado.Nombres;
                    worksheet.Cell(fila, 4).Value = invitado.Dni;
                    worksheet.Cell(fila, 5).Value = invitado.Confirmado;
                    worksheet.Cell(fila, 6).Value = invitado.NumAcompañantes;

                    var celdaFecha = worksheet.Cell(fila, 7);
                    if (invitado.FechaConfirmacion.HasValue)
                    {
                        celdaFecha.Value = invitado.FechaConfirmacion.Value.ToDateTime(TimeOnly.MinValue);
                        celdaFecha.Style.DateFormat.Format = "dd/MM/yyyy";
                    }
                    else
                    {
                        celdaFecha.Value = "Pendiente";
                    }
                    worksheet.Cell(fila, 8).Value = invitado.Correo;
                    worksheet.Cell(fila, 9).Value = invitado.Telefono;
                    worksheet.Cell(fila, 10).Value = invitado.RestriccionesAlimentarias;



                    fila++;

                }
                worksheet.Columns().AdjustToContents();
                //Descarga del archivo 
                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    var content = stream.ToArray();
                    string nombreArchivo = $"Reporte_Clientes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nombreArchivo
                    );
                }
            }
        }

        //Exportar en exel la lista de invitados de un solo evento (respeta la busqueda que este aplicada en la lista)
        public async Task<IActionResult> ExportarExelPorEvento(int? idEvento, string? buscar)
        {
            if (idEvento == null) return NotFound();

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.IdEvento == idEvento && e.Activo == true);
            if (evento == null) return NotFound();

            //enviamos la peticion a la base de datos y luego aplicamos el mismo filtro de la vista
            var invitadosEvento = await _context.Invitados
                .Include(i => i.IdMesaNavigation)
                .Where(i => i.IdEvento == idEvento && i.Activo == true)
                .OrderBy(i => i.Nombres)
                .ToListAsync();

            var InvitadosFiltrados = AplicarBusqueda(invitadosEvento, buscar);

            //aqui creamos el archivo

            using (var wb = new XLWorkbook())
            {
                var worksheet = wb.Worksheets.Add("Invitados");
                // ls encabezados

                worksheet.Cell(1, 1).Value = "Evento";
                worksheet.Cell(1, 2).Value = "Numero de mesa";
                worksheet.Cell(1, 3).Value = "Nombre";
                worksheet.Cell(1, 4).Value = "DNI";
                worksheet.Cell(1, 5).Value = "confirmacion";
                worksheet.Cell(1, 6).Value = "Numero acompañantes";
                worksheet.Cell(1, 7).Value = "fecha de confirmacion";
                worksheet.Cell(1, 8).Value = "correo";
                worksheet.Cell(1, 9).Value = "telefono";
                worksheet.Cell(1, 10).Value = "Comentario";

                //definimos el rango a estalizar
                var headerRow = worksheet.Range("A1:J1");
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                // llenado de los datos
                int fila = 2; //en la primera fila esta el encabezado entonces desde el segundo se van agregando los datos
                foreach (var invitado in InvitadosFiltrados)
                {
                    worksheet.Cell(fila, 1).Value = evento.Nombre;

                    // si no tiene mesa asignada dejamos la celda con un texto claro
                    if (invitado.IdMesaNavigation != null)
                    {
                        worksheet.Cell(fila, 2).Value = invitado.IdMesaNavigation.NumeroMesa;
                    }
                    else
                    {
                        worksheet.Cell(fila, 2).Value = "Sin mesa";
                    }

                    worksheet.Cell(fila, 3).Value = invitado.Nombres;
                    worksheet.Cell(fila, 4).Value = invitado.Dni;
                    worksheet.Cell(fila, 5).Value = invitado.Confirmado ? "Confirmado" : "Pendiente";
                    worksheet.Cell(fila, 6).Value = invitado.NumAcompañantes ?? 0;

                    var celdaFecha = worksheet.Cell(fila, 7);
                    if (invitado.FechaConfirmacion.HasValue)
                    {
                        celdaFecha.Value = invitado.FechaConfirmacion.Value.ToDateTime(TimeOnly.MinValue);
                        celdaFecha.Style.DateFormat.Format = "dd/MM/yyyy";
                    }
                    else
                    {
                        celdaFecha.Value = "Pendiente";
                    }
                    worksheet.Cell(fila, 8).Value = invitado.Correo;
                    worksheet.Cell(fila, 9).Value = invitado.Telefono;
                    worksheet.Cell(fila, 10).Value = invitado.RestriccionesAlimentarias;

                    fila++;

                }
                worksheet.Columns().AdjustToContents();
                //Descarga del archivo 
                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    var content = stream.ToArray();
                    string nombreArchivo = $"Invitados_{evento.CodigoEvento}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nombreArchivo
                    );
                }
            }
        }

        // GET: Invitado/Create
        // si se llega desde la lista de un evento (Invitado/Create?idEvento=1) el evento ya viene seleccionado y solo se muestran las mesas de ese evento
        public IActionResult Create(int? idEvento)
        {
            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Activo == true), "IdEvento", "Nombre", idEvento);
            ViewData["IdMesa"] = new SelectList(_context.Mesas.Where(m => m.Activo == true && (idEvento == null || m.IdEvento == idEvento)), "IdMesa", "NumeroMesa");
            var model = new InvitadoAdminViewModel();
            if (idEvento != null) model.IdEvento = idEvento.Value;
            return View(model);
        }

        // POST: Invitado/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvitadoAdminViewModel model)
        {
            // la mesa elegida tiene que pertenecer al mismo evento del invitado
            if (model.IdMesa != null && !await _context.Mesas.AnyAsync(m => m.IdMesa == model.IdMesa && m.IdEvento == model.IdEvento))
            {
                ModelState.AddModelError("IdMesa", "La mesa seleccionada no pertenece al evento elegido.");
            }

            // no permitimos dos invitados activos con el mismo dni en un evento, porque el acceso del invitado busca por evento + dni
            if (await _context.Invitados.AnyAsync(i => i.IdEvento == model.IdEvento && i.Dni == model.DNI && i.Activo == true))
            {
                ModelState.AddModelError("DNI", "Ya existe un invitado con este DNI en el evento seleccionado.");
            }

            if (ModelState.IsValid)
            {
                var invitado = new Invitado()
                {
                    IdEvento = model.IdEvento,
                    IdMesa = model.IdMesa,
                    Nombres = model.Nombres,
                    Dni = model.DNI,
                    Correo = model.Correo,
                    Telefono = model.Telefono,
                    RestriccionesAlimentarias = model.RestriccionesAlimentarias,
                    Confirmado = model.Confirmado,
                    FechaConfirmacion = model.Confirmado ? DateOnly.FromDateTime(DateTime.Now) : null,
                    NumAcompañantes = model.NumAcompañantes,
                    Activo = true
                };

                _context.Add(invitado);
                await _context.SaveChangesAsync();
                // volvemos a la lista de invitados del evento en el que se registro
                return RedirectToAction(nameof(PorEvento), new { idEvento = model.IdEvento });
            }

            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Activo == true), "IdEvento", "Nombre", model.IdEvento);
            ViewData["IdMesa"] = new SelectList(_context.Mesas.Where(m => m.Activo == true && m.IdEvento == model.IdEvento), "IdMesa", "NumeroMesa", model.IdMesa);
            return View(model);
        }

        // GET: Invitado/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var invitado = await _context.Invitados.FindAsync(id);
            if (invitado == null || invitado.Activo == false) return NotFound();

            var model = new InvitadoAdminViewModel()
            {
                IdInvitado = invitado.IdInvitado,
                IdEvento = invitado.IdEvento,
                IdMesa = invitado.IdMesa,
                Nombres = invitado.Nombres,
                DNI = invitado.Dni,
                Correo = invitado.Correo,
                Telefono = invitado.Telefono,
                RestriccionesAlimentarias = invitado.RestriccionesAlimentarias,
                Confirmado = invitado.Confirmado,
                NumAcompañantes = invitado.NumAcompañantes ?? 0
            };

            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Activo == true), "IdEvento", "Nombre", model.IdEvento);
            ViewData["IdMesa"] = new SelectList(_context.Mesas.Where(m => m.Activo == true && m.IdEvento == model.IdEvento), "IdMesa", "NumeroMesa", model.IdMesa);
            return View(model);
        }

        // POST: Invitado/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InvitadoAdminViewModel model)
        {
            if (id != model.IdInvitado) return NotFound();

            // la mesa elegida tiene que pertenecer al mismo evento del invitado
            if (model.IdMesa != null && !await _context.Mesas.AnyAsync(m => m.IdMesa == model.IdMesa && m.IdEvento == model.IdEvento))
            {
                ModelState.AddModelError("IdMesa", "La mesa seleccionada no pertenece al evento elegido.");
            }

            // el dni no se puede repetir en el mismo evento (ignoramos al propio invitado que estamos editando)
            if (await _context.Invitados.AnyAsync(i => i.IdEvento == model.IdEvento && i.Dni == model.DNI && i.Activo == true && i.IdInvitado != model.IdInvitado))
            {
                ModelState.AddModelError("DNI", "Ya existe un invitado con este DNI en el evento seleccionado.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var invitado = await _context.Invitados.FindAsync(id);
                    if (invitado == null) return NotFound();

                    invitado.IdEvento = model.IdEvento;
                    invitado.IdMesa = model.IdMesa;
                    invitado.Nombres = model.Nombres;
                    invitado.Dni = model.DNI;
                    invitado.Correo = model.Correo;
                    invitado.Telefono = model.Telefono;
                    invitado.RestriccionesAlimentarias = model.RestriccionesAlimentarias;
                    // si el administrador marca la asistencia como confirmada y antes no lo estaba, registramos la fecha de confirmacion
                    if (model.Confirmado && !invitado.Confirmado)
                    {
                        invitado.FechaConfirmacion = DateOnly.FromDateTime(DateTime.Now);
                    }

                    invitado.Confirmado = model.Confirmado;
                    invitado.NumAcompañantes = model.NumAcompañantes;

                    _context.Update(invitado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvitadoExists(model.IdInvitado)) return NotFound();
                    else throw;
                }
                // volvemos a la lista de invitados del evento
                return RedirectToAction(nameof(PorEvento), new { idEvento = model.IdEvento });
            }

            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Activo == true), "IdEvento", "Nombre", model.IdEvento);
            ViewData["IdMesa"] = new SelectList(_context.Mesas.Where(m => m.Activo == true && m.IdEvento == model.IdEvento), "IdMesa", "NumeroMesa", model.IdMesa);
            return View(model);
        }

        // GET: Invitado/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var invitado = await _context.Invitados
                .Include(i => i.IdEventoNavigation)
                .Include(i => i.IdMesaNavigation)
                .FirstOrDefaultAsync(m => m.IdInvitado == id);

            if (invitado == null || invitado.Activo == false) return NotFound();

            var model = new InvitadoAdminViewModel()
            {
                IdInvitado = invitado.IdInvitado,
                IdEvento = invitado.IdEvento, // lo enviamos para poder volver a la lista del evento al cancelar
                Nombres = invitado.Nombres,
                DNI = invitado.Dni,
                Correo = invitado.Correo
            };

            ViewBag.NombreEvento = invitado.IdEventoNavigation?.Nombre;
            ViewBag.NumeroMesa = invitado.IdMesaNavigation?.NumeroMesa.ToString() ?? "Sin mesa asignada";

            return View(model);
        }

        // POST: Invitado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, InvitadoAdminViewModel model)
        {
            int idEventoInvitado = 0; // guardamos el evento del invitado para volver a su lista luego de borrar
            try
            {
                var invitado = await _context.Invitados.FindAsync(id);
                if (invitado == null) return NotFound();

                idEventoInvitado = invitado.IdEvento;
                invitado.Activo = false; // Borrado lógico
                _context.Invitados.Update(invitado);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvitadoExists(model.IdInvitado)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(PorEvento), new { idEvento = idEventoInvitado });
        }

        private bool InvitadoExists(int id)
        {
            return _context.Invitados.Any(e => e.IdInvitado == id);
        }

        // PARTE 2: MÓDULO DEL INVITADO PÚBLICO (Acceso y Confirmación)
        //aqui accede el usuario para tener acceso a registrar su aceptacion y declinacion de la invitacion

        // falta implementrr el escrapeo a la web del dni.com, para realizar un cruce de informacion de los dnis no registrados

        [AllowAnonymous]
        // GET: /Invitado/Acceso/1
        [HttpGet]
        [Route("Invitado/Acceso/{idEvento}")]
        public IActionResult Acceso(int idEvento)
        {
            var model = new BuscarInvitacionViewModel { IdEvento = idEvento };
            return View(model);
        }

        // POST: /Invitado/Acceso/1
        [AllowAnonymous]
        [HttpPost]
        [Route("Invitado/Acceso/{idEvento}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Acceso(BuscarInvitacionViewModel login)
        {
            if (!ModelState.IsValid) return View(login);

            // Buscar invitado activo por DNI en ese evento específico
            var invitado = await _context.Invitados
                .FirstOrDefaultAsync(i => i.IdEvento == login.IdEvento && i.Dni == login.DNI && i.Activo == true);

            if (invitado == null)
            {
                ModelState.AddModelError(string.Empty, "No se encontró una invitación con este DNI para este evento.");
                return View(login);
            }

            // Buscar compañeros de mesa (misma mesa, que no sea él mismo y que estén activos)
            var companeros = new List<string>();
            if (invitado.IdMesa != null)
            {
                companeros = await _context.Invitados
                    .Where(i => i.IdMesa == invitado.IdMesa && i.IdInvitado != invitado.IdInvitado && i.Activo == true)
                    .Select(i => i.Nombres)
                    .ToListAsync();
            }

            // Cargar el modelo de confirmación
            var modelConfirmacion = new InvitadoConfirmacionViewModel
            {
                IdInvitado = invitado.IdInvitado,
                Nombres = invitado.Nombres,
                Confirmado = invitado.Confirmado,
                NumAcompañantes = invitado.NumAcompañantes ?? 0,
                RestriccionesAlimentarias = invitado.RestriccionesAlimentarias,
                CompañerosDeMesa = companeros
            };

            // Renderizar la vista MiInvitacion (la URL se mantiene en /Invitado/Acceso/Id para seguridad)
            return View("MiInvitacion", modelConfirmacion);
        }

        // POST: /Invitado/ConfirmarAsistencia
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarAsistencia(InvitadoConfirmacionViewModel model)
        {
            var invitado = await _context.Invitados.FindAsync(model.IdInvitado);
            if (invitado == null || invitado.Activo == false) return NotFound();

            // Solo permitimos modificar estos campos al invitado
            invitado.Confirmado = model.Confirmado;
            invitado.NumAcompañantes = model.NumAcompañantes;
            invitado.RestriccionesAlimentarias = model.RestriccionesAlimentarias;
            invitado.FechaConfirmacion = DateOnly.FromDateTime(DateTime.Now);

            _context.Update(invitado);
            await _context.SaveChangesAsync();

            // Redirigir a una página de agradecimiento
            return View("ConfirmacionExitosa");
        }
    }
}