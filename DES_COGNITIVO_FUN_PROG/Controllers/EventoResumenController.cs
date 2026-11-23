using ClosedXML.Excel;
using DES_COGNITIVO_FUN_PROG.Models;
using DES_COGNITIVO_FUN_PROG.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DES_COGNITIVO_FUN_PROG.Controllers
{
    // vista visual de un evento: mesas, invitados e informacion general, con exportacion a excel por mesa
    [Authorize(Roles = "Admin")]
    public class EventoResumenController : Controller
    {
        private const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly GestionEventosContext _context;

        public EventoResumenController(GestionEventosContext context)
        {
            _context = context;
        }

        // GET: EventoResumen/Index/5
        public async Task<IActionResult> Index(int? idEvento)
        {
            if (idEvento == null) return NotFound();

            var evento = await _context.Eventos
                .Include(e => e.IdClienteNavigation)
                .Include(e => e.IdEstadoNavigation)
                .FirstOrDefaultAsync(e => e.IdEvento == idEvento && e.Activo == true);
            if (evento == null) return NotFound();

            var (mesas, invitados) = await CargarMesasEInvitados(evento.IdEvento);

            // cada invitado se convierte en un dato simple para la vista
            ResumenInvitadoViewModel AViewModel(Invitado i) => new ResumenInvitadoViewModel
            {
                IdInvitado = i.IdInvitado,
                NumeroMesa = i.IdMesaNavigation?.NumeroMesa,
                Nombres = i.Nombres,
                Dni = i.Dni,
                Confirmado = i.Confirmado,
                NumAcompañantes = i.NumAcompañantes ?? 0,
                Correo = i.Correo,
                Telefono = i.Telefono,
                RestriccionesAlimentarias = i.RestriccionesAlimentarias
            };

            // armamos cada mesa con los invitados que tiene asignados
            var mesasVista = mesas.Select(m =>
            {
                var delaMesa = invitados.Where(i => i.IdMesa == m.IdMesa).ToList();
                return new ResumenMesaViewModel
                {
                    IdMesa = m.IdMesa,
                    NumeroMesa = m.NumeroMesa,
                    NombreMesa = m.NombreMesa,
                    Capacidad = m.Capacidad,
                    LugaresOcupados = delaMesa.Sum(i => 1 + (i.NumAcompañantes ?? 0)),
                    Confirmados = delaMesa.Count(i => i.Confirmado),
                    Pendientes = delaMesa.Count(i => !i.Confirmado),
                    Invitados = delaMesa.Select(AViewModel).ToList()
                };
            }).ToList();

            // invitados sin mesa (o con una mesa que ya fue eliminada)
            var idsMesasActivas = mesas.Select(m => m.IdMesa).ToHashSet();
            var sinMesa = invitados.Where(i => i.IdMesa == null || !idsMesasActivas.Contains(i.IdMesa.Value)).ToList();

            var confirmados = invitados.Where(i => i.Confirmado).ToList();

            var model = new EventoResumenViewModel
            {
                IdEvento = evento.IdEvento,
                CodigoEvento = evento.CodigoEvento,
                Nombre = evento.Nombre,
                Fecha = evento.Fecha,
                Lugar = evento.Lugar,
                Descripcion = evento.Descripcion,
                Presupuesto = evento.Presupuesto,
                NombreEstado = evento.IdEstadoNavigation.EstadoEvento1,
                NombreCliente = evento.IdClienteNavigation.Nombre,
                CorreoCliente = evento.IdClienteNavigation.Correo,
                TelefonoCliente = evento.IdClienteNavigation.Telefono,

                TotalInvitados = invitados.Count,
                TotalConfirmados = confirmados.Count,
                TotalPendientes = invitados.Count - confirmados.Count,
                PersonasConfirmadas = confirmados.Count + confirmados.Sum(i => i.NumAcompañantes ?? 0),
                PersonasEnTotal = invitados.Count + invitados.Sum(i => i.NumAcompañantes ?? 0),

                TotalMesas = mesas.Count,
                CapacidadTotal = mesas.Sum(m => m.Capacidad),
                LugaresOcupados = mesasVista.Sum(m => m.LugaresOcupados),
                InvitadosSinMesa = sinMesa.Count,

                Mesas = mesasVista,
                Invitados = invitados.Select(AViewModel).ToList()
            };

            if (sinMesa.Any())
            {
                model.SinMesa = new ResumenMesaViewModel
                {
                    IdMesa = 0,
                    Capacidad = 0,
                    LugaresOcupados = sinMesa.Sum(i => 1 + (i.NumAcompañantes ?? 0)),
                    Confirmados = sinMesa.Count(i => i.Confirmado),
                    Pendientes = sinMesa.Count(i => !i.Confirmado),
                    Invitados = sinMesa.Select(AViewModel).ToList()
                };
            }

            // agrupamos las restricciones alimentarias iguales (sin importar mayusculas) para ver rapido que pedir al catering
            model.Restricciones = invitados
                .Where(i => !string.IsNullOrWhiteSpace(i.RestriccionesAlimentarias))
                .GroupBy(i => i.RestriccionesAlimentarias!.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Select(g => new ResumenRestriccionViewModel
                {
                    Restriccion = g.Key,
                    Invitados = g.Select(i => i.Nombres).ToList()
                })
                .ToList();

            return View(model);
        }

        // Exportar en exel la lista de invitados de UNA mesa (idMesa = 0 exporta los invitados sin mesa)
        public async Task<IActionResult> ExportarMesa(int? idEvento, int? idMesa)
        {
            if (idEvento == null || idMesa == null) return NotFound();

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.IdEvento == idEvento && e.Activo == true);
            if (evento == null) return NotFound();

            var (mesas, invitados) = await CargarMesasEInvitados(evento.IdEvento);

            string etiqueta;
            string nombreHoja;
            Mesa? mesa = null;
            List<Invitado> lista;

            if (idMesa == 0)
            {
                var idsMesasActivas = mesas.Select(m => m.IdMesa).ToHashSet();
                lista = invitados.Where(i => i.IdMesa == null || !idsMesasActivas.Contains(i.IdMesa.Value)).ToList();
                etiqueta = "Sin mesa asignada";
                nombreHoja = "Sin mesa";
            }
            else
            {
                mesa = mesas.FirstOrDefault(m => m.IdMesa == idMesa);
                if (mesa == null) return NotFound();
                lista = invitados.Where(i => i.IdMesa == mesa.IdMesa).ToList();
                etiqueta = string.IsNullOrWhiteSpace(mesa.NombreMesa) ? $"Mesa {mesa.NumeroMesa}" : $"Mesa {mesa.NumeroMesa} - {mesa.NombreMesa}";
                nombreHoja = $"Mesa {mesa.NumeroMesa}";
            }

            using (var wb = new XLWorkbook())
            {
                EscribirHoja(wb.Worksheets.Add(nombreHoja), evento, etiqueta, mesa, lista);

                string sufijo = mesa != null ? $"Mesa{mesa.NumeroMesa}" : "SinMesa";
                return DescargarExcel(wb, $"Invitados_{evento.CodigoEvento}_{sufijo}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
            }
        }

        // Exportar en exel todas las mesas del evento, una hoja por cada mesa
        public async Task<IActionResult> ExportarTodasLasMesas(int? idEvento)
        {
            if (idEvento == null) return NotFound();

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.IdEvento == idEvento && e.Activo == true);
            if (evento == null) return NotFound();

            var (mesas, invitados) = await CargarMesasEInvitados(evento.IdEvento);

            using (var wb = new XLWorkbook())
            {
                // hoja de resumen con una fila por mesa
                var resumen = wb.Worksheets.Add("Resumen");
                resumen.Cell(1, 1).Value = "Mesa";
                resumen.Cell(1, 2).Value = "Nombre";
                resumen.Cell(1, 3).Value = "Capacidad";
                resumen.Cell(1, 4).Value = "Lugares ocupados";
                resumen.Cell(1, 5).Value = "Lugares libres";
                resumen.Cell(1, 6).Value = "Invitados";
                resumen.Cell(1, 7).Value = "Confirmados";
                var cabeceraResumen = resumen.Range("A1:G1");
                cabeceraResumen.Style.Font.Bold = true;
                cabeceraResumen.Style.Fill.BackgroundColor = XLColor.LightGray;

                int filaResumen = 2;
                foreach (var mesa in mesas)
                {
                    var lista = invitados.Where(i => i.IdMesa == mesa.IdMesa).ToList();
                    int ocupados = lista.Sum(i => 1 + (i.NumAcompañantes ?? 0));

                    resumen.Cell(filaResumen, 1).Value = mesa.NumeroMesa;
                    resumen.Cell(filaResumen, 2).Value = mesa.NombreMesa ?? "";
                    resumen.Cell(filaResumen, 3).Value = mesa.Capacidad;
                    resumen.Cell(filaResumen, 4).Value = ocupados;
                    resumen.Cell(filaResumen, 5).Value = Math.Max(0, mesa.Capacidad - ocupados);
                    resumen.Cell(filaResumen, 6).Value = lista.Count;
                    resumen.Cell(filaResumen, 7).Value = lista.Count(i => i.Confirmado);
                    filaResumen++;

                    // una hoja por mesa
                    string etiqueta = string.IsNullOrWhiteSpace(mesa.NombreMesa) ? $"Mesa {mesa.NumeroMesa}" : $"Mesa {mesa.NumeroMesa} - {mesa.NombreMesa}";
                    EscribirHoja(wb.Worksheets.Add($"Mesa {mesa.NumeroMesa}"), evento, etiqueta, mesa, lista);
                }

                // hoja de invitados sin mesa, solo si existen
                var idsMesasActivas = mesas.Select(m => m.IdMesa).ToHashSet();
                var sinMesa = invitados.Where(i => i.IdMesa == null || !idsMesasActivas.Contains(i.IdMesa.Value)).ToList();
                if (sinMesa.Any())
                {
                    resumen.Cell(filaResumen, 1).Value = "Sin mesa";
                    resumen.Cell(filaResumen, 6).Value = sinMesa.Count;
                    resumen.Cell(filaResumen, 7).Value = sinMesa.Count(i => i.Confirmado);
                    EscribirHoja(wb.Worksheets.Add("Sin mesa"), evento, "Sin mesa asignada", null, sinMesa);
                }

                resumen.Columns().AdjustToContents();

                return DescargarExcel(wb, $"Mesas_{evento.CodigoEvento}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
            }
        }

        // trae las mesas activas del evento y sus invitados activos (ordenados), se usa en la vista y en los excel
        private async Task<(List<Mesa> Mesas, List<Invitado> Invitados)> CargarMesasEInvitados(int idEvento)
        {
            var mesas = await _context.Mesas
                .Where(m => m.IdEvento == idEvento && m.Activo == true)
                .OrderBy(m => m.NumeroMesa)
                .ToListAsync();

            var invitados = await _context.Invitados
                .Include(i => i.IdMesaNavigation)
                .Where(i => i.IdEvento == idEvento && i.Activo == true)
                .OrderBy(i => i.Nombres)
                .ToListAsync();

            return (mesas, invitados);
        }

        // escribe en una hoja la lista de invitados de una mesa, con un titulo y los totales al final
        private static void EscribirHoja(IXLWorksheet hoja, Evento evento, string etiquetaMesa, Mesa? mesa, List<Invitado> invitados)
        {
            // titulo e informacion de la mesa
            hoja.Cell(1, 1).Value = $"{evento.Nombre} - {etiquetaMesa}";
            hoja.Cell(1, 1).Style.Font.Bold = true;
            hoja.Cell(1, 1).Style.Font.FontSize = 14;

            int ocupados = invitados.Sum(i => 1 + (i.NumAcompañantes ?? 0));
            if (mesa != null)
            {
                hoja.Cell(2, 1).Value = $"Capacidad: {mesa.Capacidad}   |   Lugares ocupados: {ocupados}   |   Lugares libres: {Math.Max(0, mesa.Capacidad - ocupados)}";
            }
            else
            {
                hoja.Cell(2, 1).Value = $"Lugares ocupados: {ocupados}";
            }

            // los encabezados
            const int filaEncabezado = 4;
            string[] encabezados = { "N°", "Nombre", "DNI", "Confirmacion", "Numero acompañantes", "Lugares ocupados", "Correo", "Telefono", "Restricciones alimentarias" };
            for (int c = 0; c < encabezados.Length; c++)
            {
                hoja.Cell(filaEncabezado, c + 1).Value = encabezados[c];
            }

            var cabecera = hoja.Range(filaEncabezado, 1, filaEncabezado, encabezados.Length);
            cabecera.Style.Font.Bold = true;
            cabecera.Style.Fill.BackgroundColor = XLColor.LightGray;

            // llenado de los datos
            int fila = filaEncabezado + 1;
            int numero = 1;
            foreach (var invitado in invitados)
            {
                int acompañantes = invitado.NumAcompañantes ?? 0;

                hoja.Cell(fila, 1).Value = numero++;
                hoja.Cell(fila, 2).Value = invitado.Nombres;
                hoja.Cell(fila, 3).Value = invitado.Dni;
                hoja.Cell(fila, 4).Value = invitado.Confirmado ? "Confirmado" : "Pendiente";
                hoja.Cell(fila, 5).Value = acompañantes;
                hoja.Cell(fila, 6).Value = 1 + acompañantes;
                hoja.Cell(fila, 7).Value = invitado.Correo ?? "";
                hoja.Cell(fila, 8).Value = invitado.Telefono ?? "";
                hoja.Cell(fila, 9).Value = invitado.RestriccionesAlimentarias ?? "";
                fila++;
            }

            // fila de totales
            fila++;
            hoja.Cell(fila, 2).Value = "TOTAL";
            hoja.Cell(fila, 4).Value = $"{invitados.Count(i => i.Confirmado)} de {invitados.Count} confirmados";
            hoja.Cell(fila, 5).Value = invitados.Sum(i => i.NumAcompañantes ?? 0);
            hoja.Cell(fila, 6).Value = ocupados;
            hoja.Range(fila, 1, fila, encabezados.Length).Style.Font.Bold = true;

            // el titulo no debe ensanchar la primera columna
            hoja.Columns().AdjustToContents(filaEncabezado, fila);
        }

        // convierte el libro de excel en un archivo para descargar
        private FileContentResult DescargarExcel(XLWorkbook wb, string nombreArchivo)
        {
            using (var stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(stream.ToArray(), TipoExcel, nombreArchivo);
            }
        }
    }
}