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
        public async Task<IActionResult> Index(string? buscar)
        {
            var invitados = _context.Invitados
                .Include(i => i.IdEventoNavigation)
                .Include(i => i.IdMesaNavigation)
                .Where(i => i.Activo == true);

            if (!string.IsNullOrEmpty(buscar))
            {
                invitados = invitados.Where(i => i.Dni.Contains(buscar));
            }

            ViewData["BusquedaInvitado"] = buscar;

            var BuscarInvitado = await invitados.ToListAsync();
            return View(BuscarInvitado);
        }

        // GET: Invitado/Create
        public IActionResult Create()
        {
            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Activo == true), "IdEvento", "Nombre");
            ViewData["IdMesa"] = new SelectList(_context.Mesas.Where(m => m.Activo == true), "IdMesa", "NumeroMesa");
            var model = new InvitadoAdminViewModel();
            return View(model);
        }

        // POST: Invitado/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvitadoAdminViewModel model)
        {
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
                    NumAcompañantes = model.NumAcompañantes,
                    Activo = true
                };

                _context.Add(invitado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Activo == true), "IdEvento", "Nombre", model.IdEvento);
            ViewData["IdMesa"] = new SelectList(_context.Mesas.Where(m => m.Activo == true), "IdMesa", "NumeroMesa", model.IdMesa);
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
                Confirmado = invitado.Confirmado ?? false,
                NumAcompañantes = invitado.NumAcompañantes ?? 0
            };

            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Activo == true), "IdEvento", "Nombre", model.IdEvento);
            ViewData["IdMesa"] = new SelectList(_context.Mesas.Where(m => m.Activo == true), "IdMesa", "NumeroMesa", model.IdMesa);
            return View(model);
        }

        // POST: Invitado/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InvitadoAdminViewModel model)
        {
            if (id != model.IdInvitado) return NotFound();

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
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Activo == true), "IdEvento", "Nombre", model.IdEvento);
            ViewData["IdMesa"] = new SelectList(_context.Mesas.Where(m => m.Activo == true), "IdMesa", "NumeroMesa", model.IdMesa);
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
            try
            {
                var invitado = await _context.Invitados.FindAsync(id);
                if (invitado == null) return NotFound();

                invitado.Activo = false; // Borrado lógico
                _context.Invitados.Update(invitado);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvitadoExists(model.IdInvitado)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
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
                Confirmado = invitado.Confirmado ?? false,
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