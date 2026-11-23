using ClosedXML.Excel;
using DES_COGNITIVO_FUN_PROG.Helpers;
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
            var eventos = _context.Eventos
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

            //buscamos el codigo del evento en la bd como string
            {
                buscar = buscar.Trim();
                eventos = eventos.Where(e => e.CodigoEvento.Contains(buscar) || e.Nombre.Contains(buscar));
            }
            ViewData["BusquedaEventos"] = buscar;

            //el metodo tolistasync pide en una lista los datos que se pide a la bd, se pide al final luego de empaquetar todo lo que deceamos pedir
            var BuscarEvetos = await eventos.ToListAsync();

            return View(BuscarEvetos);
        }





        //Exportar en exel un reporte de los clientes de los eventos
        public async Task<IActionResult> ExportarExel(string? buscar)
        {
            //aplicamos el filtro del index
            var consulta = _context.Eventos.Where(a => a.Activo);
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();
                consulta = consulta.Where(c => c.CodigoEvento.Contains(buscar) || c.Nombre.Contains(buscar));
            }

            //gaurdamos en un viewdata, el texto buscado para que no se pierda en la vista
            ViewData["BusquedaActual"] = buscar;

            //enviamos la peticion a la base de datos a traves del metodo asincronico implementado
            var EventosFiltrados = await consulta.ToListAsync();


            //aqui creamos el archivo

            using (var wb = new XLWorkbook())
            {
                var worksheet = wb.Worksheets.Add("Invitado");
                // ls encabezados

                worksheet.Cell(1, 1).Value = "Numero Evento";
                worksheet.Cell(1, 2).Value = "Estado evento";
                worksheet.Cell(1, 3).Value = "Nombre";
                worksheet.Cell(1, 4).Value = "Presupuesto";
                worksheet.Cell(1, 5).Value = "Fecha";
                worksheet.Cell(1, 6).Value = "Lugar";
                worksheet.Cell(1, 7).Value = "CodigoEvento";
                worksheet.Cell(1, 8).Value = "Descripcion";



                //definimos el rango a estalizar
                var headerRow = worksheet.Range("A1:H1");
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                // llenado de los datos causa
                int fila = 2; //en la primera fila esta el encabezado entonces desde el segundo se van agregando los datos
                foreach (var evento in EventosFiltrados)
                {
                    worksheet.Cell(fila, 1).Value = evento.IdEvento;
                    worksheet.Cell(fila, 2).Value = evento.IdEstado;
                    worksheet.Cell(fila, 3).Value = evento.Nombre;
                    worksheet.Cell(fila, 4).Value = evento.Presupuesto;

                    var celdaFecha = worksheet.Cell(fila, 5);
                    celdaFecha.Value = evento.Fecha.ToDateTime(TimeOnly.MinValue);
                    celdaFecha.Style.DateFormat.Format = "dd/MM/yyyy";
                    worksheet.Cell(fila, 6).Value = evento.Lugar;
                    worksheet.Cell(fila, 7).Value = evento.CodigoEvento;
                    worksheet.Cell(fila, 8).Value = evento.Descripcion;


                    fila++;

                }
                worksheet.Columns().AdjustToContents();
                //Descarga del archivo 
                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    var content = stream.ToArray();
                    string nombreArchivo = $"Reporte_Eventos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nombreArchivo
                    );
                }
            }
        }


        // Cambia el estado de un evento desde el tablero del dashboard (arrastrando la tarjeta o con la lista desplegable)
        // responde en json porque se llama con fetch desde la vista, sin recargar la pagina
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int idEvento, int idEstado)
        {
            try
            {
                // el evento tiene que existir y estar activo (borrado logico)
                var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.IdEvento == idEvento && e.Activo);
                if (evento == null) return NotFound(new { ok = false, mensaje = "El evento no existe o fue desactivado." });

                // el estado destino tiene que existir en la tabla de estados y estar activo
                var estadoValido = await _context.EstadoEventos.AnyAsync(s => s.IdEstado == idEstado && s.Activo == true);
                if (!estadoValido) return BadRequest(new { ok = false, mensaje = "El estado seleccionado no es valido." });

                evento.IdEstado = idEstado;
                await _context.SaveChangesAsync();

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                // si algo falla al guardar devolvemos el mensaje real para poder verlo en el tablero del dashboard
                return StatusCode(500, new { ok = false, mensaje = ex.GetBaseException().Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new EventoViewModel();
            CargarListas(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // archivoInvitados es opcional: un excel con los invitados iniciales del evento
        public async Task<IActionResult> Create(EventoViewModel model, IFormFile? archivoInvitados)
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

            // si se subio un excel de invitados, validamos que sea un archivo valido antes de crear el evento
            bool hayArchivoInvitados = archivoInvitados != null && archivoInvitados.Length > 0;
            if (hayArchivoInvitados)
            {
                var errorArchivo = ImportadorInvitadosExcel.ValidarArchivo(archivoInvitados);
                if (errorArchivo != null) ModelState.AddModelError("archivoInvitados", errorArchivo);
            }

            // ---- MESAS DEL EVENTO ----
            // las mesas llegan como bloques editables del formulario, si no hay ningun bloque pero se indico
            // la cantidad de mesas y los invitados por mesa, las generamos aqui
            model.Mesas ??= new List<MesaEventoViewModel>();
            model.Mesas.RemoveAll(m => m.Eliminar);
            foreach (var m in model.Mesas) m.IdMesa = 0; // en Create todas las mesas son nuevas
            if (model.Mesas.Count == 0 && model.CantidadMesas > 0)
            {
                if (model.CapacidadPorMesa == null)
                {
                    ModelState.AddModelError(nameof(model.CapacidadPorMesa), "Indique cuántos invitados caben en cada mesa.");
                }
                else if (model.CantidadMesas <= MesasEventoHelper.MaximoMesasPorEvento)
                {
                    model.Mesas = MesasEventoHelper.Generar(model.CantidadMesas!.Value, model.CapacidadPorMesa.Value);
                }
            }
            NormalizarClaves(model.Mesas);

            // validaciones del evento (cliente, estado, fecha, nombre repetido) y de las mesas
            await ValidarDatosEventoAsync(model, null);
            foreach (var error in MesasEventoHelper.ValidarMesas(model.Mesas))
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (ModelState.IsValid)
            {
                var evento = new Evento()
                {

                    IdCliente = model.IdCliente!.Value,
                    IdEstado = model.IdEstado!.Value,
                    Nombre = model.Nombre,
                    Presupuesto = model.Presupuesto!.Value,
                    Fecha = model.Fecha!.Value,
                    Lugar = model.Lugar,
                    CodigoEvento = model.CodigoEvento,
                    Descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim(),
                    Activo = true

                };

                // las mesas se guardan junto con el evento en la misma operacion (si algo falla no queda nada a medias)
                foreach (var m in model.Mesas)
                {
                    evento.Mesas.Add(new Mesa
                    {
                        NumeroMesa = m.NumeroMesa!.Value,
                        Capacidad = m.Capacidad!.Value,
                        NombreMesa = string.IsNullOrWhiteSpace(m.NombreMesa) ? null : m.NombreMesa.Trim(),
                        Activo = true
                    });
                }

                //agregamoss al evento los datos optenidos del formulario entregado al cliete
                _context.Add(evento);

                //guardamos los datos igualados del formulario
                await _context.SaveChangesAsync();

                // si se subio un excel, registramos los invitados iniciales en el evento recien creado
                // si algo falla con el excel el evento ya queda creado, y se puede importar de nuevo desde la lista de invitados del evento
                if (hayArchivoInvitados)
                {
                    var resultado = await ImportadorInvitadosExcel.ImportarAsync(_context, evento.IdEvento, archivoInvitados!);

                    // los invitados que el excel dejo sin mesa se reparten en las mesas con lugares libres
                    if (model.DistribuirInvitados && resultado.Importados > 0)
                    {
                        var (asignados, sinLugar) = await MesasEventoHelper.DistribuirSinMesaAsync(_context, evento.IdEvento);
                        if (asignados > 0)
                            resultado.Detalles.Add($"Se distribuyeron automaticamente {asignados} invitado(s) en las mesas con lugares libres.");
                        if (sinLugar > 0)
                            resultado.Detalles.Add($"{sinLugar} invitado(s) quedaron sin mesa porque no hay lugares libres. Agrega mesas desde Editar evento.");
                    }

                    resultado.GuardarEn(TempData);

                    // llevamos al administrador a la lista de invitados del evento para que vea el resultado de la importacion
                    return RedirectToAction("PorEvento", "Invitado", new { idEvento = evento.IdEvento });
                }

                return RedirectToAction(nameof(Index));
            }

            // Si falla la validación, recargamos los SelectList y mostramos los bloques de mesas como estaban
            model.Mesas = model.Mesas.OrderBy(m => m.NumeroMesa ?? int.MaxValue).ToList();
            CargarListas(model);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.IdEvento == id && e.Activo);
            if (evento == null) return NotFound();

            var model = new EventoViewModel()
            {
                IdEvento = evento.IdEvento,
                IdCliente = evento.IdCliente,
                IdEstado = evento.IdEstado,
                Nombre = evento.Nombre,
                Presupuesto = evento.Presupuesto,
                Fecha = evento.Fecha,
                Lugar = evento.Lugar,
                CodigoEvento = evento.CodigoEvento,
                Descripcion = evento.Descripcion
            };

            // bloques de mesas del evento, con los invitados sentados en cada una
            var mesas = await _context.Mesas
                .Where(m => m.IdEvento == evento.IdEvento && m.Activo == true)
                .OrderBy(m => m.NumeroMesa)
                .ToListAsync();
            var invitados = await _context.Invitados
                .Where(i => i.IdEvento == evento.IdEvento && i.Activo)
                .OrderBy(i => i.Nombres)
                .ToListAsync();

            model.Mesas = mesas.Select(m => new MesaEventoViewModel
            {
                IdMesa = m.IdMesa,
                Clave = m.IdMesa.ToString(),
                NumeroMesa = m.NumeroMesa,
                NombreMesa = m.NombreMesa,
                Capacidad = m.Capacidad
            }).ToList();

            model.Invitados = invitados.Select(i => new InvitadoMesaViewModel
            {
                IdInvitado = i.IdInvitado,
                Nombres = i.Nombres,
                Dni = i.Dni,
                Confirmado = i.Confirmado,
                Acompanantes = i.NumAcompañantes ?? 0,
                // si la mesa del invitado ya no esta activa, lo mostramos sin mesa
                MesaClave = i.IdMesa != null && mesas.Any(m => m.IdMesa == i.IdMesa) ? i.IdMesa.ToString() : ""
            }).ToList();

            CargarListas(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventoViewModel model)
        {
            if (id != model.IdEvento) return NotFound();

            // Quitamos la validación del CodigoEvento en la edición, el código nunca se modifica
            ModelState.Remove("CodigoEvento");

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.IdEvento == id && e.Activo);
            if (evento == null) return NotFound();
            model.CodigoEvento = evento.CodigoEvento;

            var mesasDb = await _context.Mesas
                .Where(m => m.IdEvento == id && m.Activo == true)
                .ToListAsync();
            var invitadosDb = await _context.Invitados
                .Where(i => i.IdEvento == id && i.Activo)
                .ToListAsync();

            model.Mesas ??= new List<MesaEventoViewModel>();
            model.Invitados ??= new List<InvitadoMesaViewModel>();

            // las mesas existentes que llegan del formulario tienen que ser del evento (y no repetirse)
            // si alguien mas las cambio mientras se editaba, recargamos la pagina
            model.Mesas.RemoveAll(m => m.IdMesa == 0 && m.Eliminar);
            var idsEnviados = model.Mesas.Where(m => m.IdMesa != 0).Select(m => m.IdMesa).ToList();
            if (idsEnviados.Distinct().Count() != idsEnviados.Count || idsEnviados.Any(x => mesasDb.All(d => d.IdMesa != x)))
            {
                TempData["ErrorEdicion"] = "Las mesas del evento cambiaron mientras editabas. Se recargó la página con los datos actuales, vuelve a hacer tus cambios.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            NormalizarClaves(model.Mesas);

            // las mesas de la bd que no llegaron en el formulario se mantienen tal cual
            foreach (var d in mesasDb.Where(d => model.Mesas.All(m => m.IdMesa != d.IdMesa)).ToList())
            {
                model.Mesas.Add(new MesaEventoViewModel
                {
                    IdMesa = d.IdMesa,
                    Clave = d.IdMesa.ToString(),
                    NumeroMesa = d.NumeroMesa,
                    NombreMesa = d.NombreMesa,
                    Capacidad = d.Capacidad
                });
            }

            var mesasVigentes = model.Mesas.Where(m => !m.Eliminar).ToList();
            var clavesVigentes = new HashSet<string>(mesasVigentes.Select(m => m.Clave));

            // invitados: solo los del evento, y los que no llegaron en el formulario conservan su mesa
            var invitadosPorId = invitadosDb.ToDictionary(d => d.IdInvitado);
            var invitadosEnviados = model.Invitados
                .Where(g => invitadosPorId.ContainsKey(g.IdInvitado))
                .GroupBy(g => g.IdInvitado)
                .Select(g => g.First())
                .ToList();
            foreach (var d in invitadosDb.Where(d => invitadosEnviados.All(g => g.IdInvitado != d.IdInvitado)))
            {
                invitadosEnviados.Add(new InvitadoMesaViewModel { IdInvitado = d.IdInvitado, MesaClave = d.IdMesa?.ToString() ?? "" });
            }
            model.Invitados = invitadosEnviados;

            foreach (var g in model.Invitados)
            {
                var d = invitadosPorId[g.IdInvitado];
                g.Nombres = d.Nombres;
                g.Dni = d.Dni;
                g.Confirmado = d.Confirmado;
                g.Acompanantes = d.NumAcompañantes ?? 0;

                // si la mesa elegida no existe o fue eliminada en este formulario, el invitado queda sin mesa
                if (string.IsNullOrEmpty(g.MesaClave) || !clavesVigentes.Contains(g.MesaClave)) g.MesaClave = "";
            }

            // validaciones del evento y de las mesas
            await ValidarDatosEventoAsync(model, id);
            var erroresMesas = MesasEventoHelper.ValidarMesas(mesasVigentes);
            foreach (var error in erroresMesas) ModelState.AddModelError(string.Empty, error);

            // ninguna mesa puede quedar con mas invitados (y acompañantes) que su capacidad
            if (erroresMesas.Count == 0)
            {
                foreach (var m in mesasVigentes.OrderBy(m => m.NumeroMesa))
                {
                    int ocupados = model.Invitados.Where(g => g.MesaClave == m.Clave).Sum(g => 1 + g.Acompanantes);
                    if (ocupados > m.Capacidad)
                    {
                        ModelState.AddModelError(string.Empty,
                            $"La mesa {m.NumeroMesa} tiene {ocupados} lugares ocupados y su capacidad es {m.Capacidad}. Mueve invitados a otra mesa o aumenta la capacidad.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    evento.IdCliente = model.IdCliente!.Value;
                    evento.IdEstado = model.IdEstado!.Value;
                    evento.Nombre = model.Nombre;
                    evento.Presupuesto = model.Presupuesto!.Value;
                    evento.Fecha = model.Fecha!.Value;
                    evento.Lugar = model.Lugar;
                    evento.Descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim();

                    // mesas: actualizamos las existentes, creamos las nuevas y desactivamos las eliminadas (borrado logico)
                    var mesasPorClave = new Dictionary<string, Mesa>();
                    foreach (var m in model.Mesas)
                    {
                        if (m.IdMesa != 0)
                        {
                            var mesa = mesasDb.First(d => d.IdMesa == m.IdMesa);
                            if (m.Eliminar)
                            {
                                mesa.Activo = false;
                                continue;
                            }

                            mesa.NumeroMesa = m.NumeroMesa!.Value;
                            mesa.Capacidad = m.Capacidad!.Value;
                            mesa.NombreMesa = string.IsNullOrWhiteSpace(m.NombreMesa) ? null : m.NombreMesa.Trim();
                            mesasPorClave[m.Clave] = mesa;
                        }
                        else if (!m.Eliminar)
                        {
                            var nueva = new Mesa
                            {
                                IdEvento = id,
                                NumeroMesa = m.NumeroMesa!.Value,
                                Capacidad = m.Capacidad!.Value,
                                NombreMesa = string.IsNullOrWhiteSpace(m.NombreMesa) ? null : m.NombreMesa.Trim(),
                                Activo = true
                            };
                            _context.Mesas.Add(nueva);
                            mesasPorClave[m.Clave] = nueva;
                        }
                    }

                    // invitados: cada uno queda en la mesa que se eligio en su bloque (o sin mesa)
                    foreach (var g in model.Invitados)
                    {
                        var invitado = invitadosPorId[g.IdInvitado];
                        if (g.MesaClave != null && mesasPorClave.TryGetValue(g.MesaClave, out var mesaDestino))
                        {
                            if (mesaDestino.IdMesa != 0) invitado.IdMesa = mesaDestino.IdMesa;
                            else invitado.IdMesaNavigation = mesaDestino; // mesa nueva: EF la guarda primero y completa el IdMesa
                        }
                        else
                        {
                            invitado.IdMesa = null;
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventoExists(model.IdEvento)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Si hay error, recargamos los SelectList y devolvemos los bloques tal como estaban
            model.Mesas = model.Mesas.OrderBy(m => m.NumeroMesa ?? int.MaxValue).ToList();
            CargarListas(model);
            return View(model);
        }

        // ---- apoyo para los formularios de crear y editar ----

        // lista desplegable de clientes y estados activos
        private void CargarListas(EventoViewModel model)
        {
            ViewData["IdCliente"] = new SelectList(_context.Clientes.Where(c => c.Activo == true), "IdCliente", "Nombre", model.IdCliente);
            ViewData["IdEstado"] = new SelectList(_context.EstadoEventos.Where(e => e.Activo == true), "IdEstado", "EstadoEvento1", model.IdEstado);
        }

        // validaciones del evento que necesitan consultar la bd (idEventoActual = null cuando se esta creando)
        private async Task ValidarDatosEventoAsync(EventoViewModel model, int? idEventoActual)
        {
            model.Nombre = model.Nombre?.Trim()!;
            model.Lugar = model.Lugar?.Trim()!;

            // un evento nuevo no puede tener una fecha que ya paso (al editar si se permite, por ejemplo para cerrar un evento viejo)
            if (idEventoActual == null && model.Fecha != null && model.Fecha < DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError(nameof(model.Fecha), "La fecha del evento no puede ser anterior a hoy.");
            }

            // el cliente y el estado tienen que existir y estar activos
            if (model.IdCliente != null && !await _context.Clientes.AnyAsync(c => c.IdCliente == model.IdCliente && c.Activo == true))
            {
                ModelState.AddModelError(nameof(model.IdCliente), "El cliente seleccionado no es válido.");
            }
            if (model.IdEstado != null && !await _context.EstadoEventos.AnyAsync(e => e.IdEstado == model.IdEstado && e.Activo == true))
            {
                ModelState.AddModelError(nameof(model.IdEstado), "El estado seleccionado no es válido.");
            }

            // no puede haber dos eventos activos del mismo cliente con el mismo nombre y fecha
            if (model.IdCliente != null && model.Fecha != null && !string.IsNullOrEmpty(model.Nombre)
                && await _context.Eventos.AnyAsync(e => e.Activo && e.IdCliente == model.IdCliente && e.Fecha == model.Fecha.Value
                                                      && e.Nombre == model.Nombre && e.IdEvento != (idEventoActual ?? 0)))
            {
                ModelState.AddModelError(nameof(model.Nombre), "Este cliente ya tiene un evento con el mismo nombre en esa fecha.");
            }
        }

        // deja cada mesa con una clave valida y unica: las existentes usan su IdMesa, las nuevas "n1", "n2"...
        // los invitados se asignan a las mesas por esta clave
        private static void NormalizarClaves(List<MesaEventoViewModel> mesas)
        {
            var usadas = new HashSet<string>();
            int contador = 100000;
            foreach (var m in mesas)
            {
                if (m.IdMesa != 0)
                {
                    m.Clave = m.IdMesa.ToString();
                }
                else if (string.IsNullOrWhiteSpace(m.Clave) || !System.Text.RegularExpressions.Regex.IsMatch(m.Clave, @"^n\d{1,9}$") || usadas.Contains(m.Clave))
                {
                    do { m.Clave = "n" + contador++; } while (usadas.Contains(m.Clave));
                }
                usadas.Add(m.Clave);
            }
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