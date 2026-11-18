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
        public async Task<IActionResult> Index()
        {
            // Filtramos solo las mesas activas e incluimos la navegación del Evento para mostrar su nombre
            var mesasActivas = await _context.Mesas
                .Include(m => m.IdEventoNavigation)
                .Where(m => m.Activo == true)
                .ToListAsync();

            return View(mesasActivas);
        }

        // GET: Mesa/Create
        public IActionResult Create()
        {
            // Cargamos los eventos activos para el DropDownList
            var eventosActivos = _context.Eventos.Where(e => e.Activo == true).ToList();
            ViewData["IdEvento"] = new SelectList(eventosActivos, "IdEvento", "Nombre");
            return View();
        }

        // POST: Mesa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MesaViewModel model)
        {
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
                return RedirectToAction(nameof(Index));
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