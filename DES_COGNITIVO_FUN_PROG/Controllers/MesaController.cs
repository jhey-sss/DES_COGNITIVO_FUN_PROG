using DES_COGNITIVO_FUN_PROG.Models;
using DES_COGNITIVO_FUN_PROG.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MesaController : Controller
    {
        private readonly GestionEventosContext _context;

        public MesaController(GestionEventosContext context)
        {
            _context = context;
        }

        // GET: Mesa
        // idEvento es opcional: si viene, solo se muestran las mesas de ese evento
        public async Task<IActionResult> Index(int? idEvento)
        {
            // Filtramos solo las mesas activas e incluimos la navegación del Evento para mostrar su nombre
            // tambien incluimos los invitados de cada mesa para mostrar cuantos lugares estan ocupados
            var consulta = _context.Mesas
                .Include(m => m.IdEventoNavigation)
                .Include(m => m.Invitados)
                .Where(m => m.Activo == true);

            // filtro opcional por evento
            if (idEvento != null)
            {
                consulta = consulta.Where(m => m.IdEvento == idEvento);
            }

            var mesasActivas = await consulta
                .OrderBy(m => m.IdEventoNavigation.Nombre)
                .ThenBy(m => m.NumeroMesa)
                .ToListAsync();

            // lista de eventos para el filtro de la vista
            var eventosFiltro = _context.Eventos.Where(e => e.Activo == true).ToList();
            ViewData["IdEvento"] = new SelectList(eventosFiltro, "IdEvento", "Nombre", idEvento);
            ViewData["IdEventoFiltro"] = idEvento;

            return View(mesasActivas);
        }

        // GET: Mesa/Create
        // si se llega desde la lista de mesas filtrada por un evento (Mesa/Create?idEvento=1) el evento ya viene seleccionado
        public IActionResult Create(int? idEvento)
        {
            // Cargamos los eventos activos para el DropDownList
            var eventosActivos = _context.Eventos.Where(e => e.Activo == true).ToList();
            ViewData["IdEvento"] = new SelectList(eventosActivos, "IdEvento", "Nombre", idEvento);

            var model = new MesaViewModel();
            if (idEvento != null) model.IdEvento = idEvento.Value;
            return View(model);
        }

        // POST: Mesa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MesaViewModel model)
        {
            // el numero de mesa no se puede repetir dentro del mismo evento, porque los invitados se asignan por mesa
            if (await _context.Mesas.AnyAsync(m => m.IdEvento == model.IdEvento && m.NumeroMesa == model.NumeroMesa && m.Activo == true))
            {
                ModelState.AddModelError("NumeroMesa", "Ya existe una mesa con este número en el evento seleccionado.");
            }

            if (ModelState.IsValid)
            {
                var mesa = new Mesa()
                {
                    IdEvento = model.IdEvento,
                    NumeroMesa = model.NumeroMesa,
                    Capacidad = model.Capacidad,
                    NombreMesa = model.NombreMesa,
                    Activo = true // Por defecto al crear
                };

                _context.Add(mesa);
                await _context.SaveChangesAsync();
                // volvemos a la lista de mesas del evento en el que se creo
                return RedirectToAction(nameof(Index), new { idEvento = model.IdEvento });
            }

            var eventosActivos = _context.Eventos.Where(e => e.Activo == true).ToList();
            ViewData["IdEvento"] = new SelectList(eventosActivos, "IdEvento", "Nombre", model.IdEvento);
            return View(model);
        }

        // GET: Mesa/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null || mesa.Activo == false) return NotFound();

            var model = new MesaViewModel()
            {
                IdMesa = mesa.IdMesa,
                IdEvento = mesa.IdEvento,
                NumeroMesa = mesa.NumeroMesa,
                Capacidad = mesa.Capacidad,
                NombreMesa = mesa.NombreMesa
            };

            var eventosActivos = _context.Eventos.Where(e => e.Activo == true).ToList();
            ViewData["IdEvento"] = new SelectList(eventosActivos, "IdEvento", "Nombre", model.IdEvento);

            return View(model);
        }

        // POST: Mesa/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MesaViewModel model)
        {
            if (id != model.IdMesa) return NotFound();

            // el numero de mesa no se puede repetir dentro del mismo evento (ignoramos la propia mesa que estamos editando)
            if (await _context.Mesas.AnyAsync(m => m.IdEvento == model.IdEvento && m.NumeroMesa == model.NumeroMesa && m.Activo == true && m.IdMesa != model.IdMesa))
            {
                ModelState.AddModelError("NumeroMesa", "Ya existe una mesa con este número en el evento seleccionado.");
            }

            // si la mesa ya tiene invitados sentados no se puede cambiar de evento, porque los invitados quedarian en una mesa de otro evento
            var mesaActual = await _context.Mesas.AsNoTracking().FirstOrDefaultAsync(m => m.IdMesa == id);
            if (mesaActual != null && mesaActual.IdEvento != model.IdEvento
                && await _context.Invitados.AnyAsync(i => i.IdMesa == id && i.Activo == true))
            {
                ModelState.AddModelError("IdEvento", "No puedes cambiar el evento de una mesa que ya tiene invitados asignados.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var mesa = await _context.Mesas.FindAsync(id);
                    if (mesa == null) return NotFound();

                    mesa.IdEvento = model.IdEvento;
                    mesa.NumeroMesa = model.NumeroMesa;
                    mesa.Capacidad = model.Capacidad;
                    mesa.NombreMesa = model.NombreMesa;

                    _context.Update(mesa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MesaExists(model.IdMesa)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var eventosActivos = _context.Eventos.Where(e => e.Activo == true).ToList();
            ViewData["IdEvento"] = new SelectList(eventosActivos, "IdEvento", "Nombre", model.IdEvento);
            return View(model);
        }

        // GET: Mesa/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var mesa = await _context.Mesas
                .Include(m => m.IdEventoNavigation) // Incluimos el evento para mostrar el nombre en la vista
                .FirstOrDefaultAsync(m => m.IdMesa == id);

            if (mesa == null || mesa.Activo == false) return NotFound();

            var model = new MesaViewModel()
            {
                IdMesa = mesa.IdMesa,
                IdEvento = mesa.IdEvento,
                NumeroMesa = mesa.NumeroMesa,
                Capacidad = mesa.Capacidad,
                NombreMesa = mesa.NombreMesa
            };

            // Pasamos el nombre del evento a la vista mediante ViewBag para mostrarlo al eliminar
            ViewBag.NombreEvento = mesa.IdEventoNavigation.Nombre;

            return View(model);
        }

        // POST: Mesa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, MesaViewModel model)
        {
            try
            {
                var mesa = await _context.Mesas.FindAsync(id);
                if (mesa == null) return NotFound();

                // Borrado lógico
                mesa.Activo = false;

                // los invitados que estaban sentados en esta mesa quedan sin mesa asignada
                var invitadosDeLaMesa = await _context.Invitados.Where(i => i.IdMesa == id).ToListAsync();
                foreach (var invitado in invitadosDeLaMesa)
                {
                    invitado.IdMesa = null;
                }

                _context.Mesas.Update(mesa);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MesaExists(model.IdMesa)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MesaExists(int id)
        {
            return _context.Mesas.Any(e => e.IdMesa == id);
        }
    }
}