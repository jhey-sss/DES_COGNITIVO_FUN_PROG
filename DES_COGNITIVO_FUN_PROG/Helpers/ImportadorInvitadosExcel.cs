using ClosedXML.Excel;
using DES_COGNITIVO_FUN_PROG.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DES_COGNITIVO_FUN_PROG.Helpers
{
    // resultado de una importacion de invitados desde excel, se usa para mostrarle al administrador que paso con cada fila
    public class ResultadoImportacionInvitados
    {
        public int Importados { get; set; }
        public int Omitidos { get; set; }

        // si es distinto de null, el archivo completo no se pudo procesar (columnas faltantes, archivo corrupto, etc)
        public string? ErrorGeneral { get; set; }

        // mensajes por fila (errores y advertencias)
        public List<string> Detalles { get; set; } = new List<string>();

        // texto corto que se muestra en negrita en la alerta
        public string Resumen()
        {
            if (ErrorGeneral != null) return ErrorGeneral;
            if (Importados == 0 && Omitidos == 0) return "El archivo no tiene invitados para importar.";
            return $"Importacion finalizada: {Importados} invitado(s) registrado(s) y {Omitidos} fila(s) omitida(s).";
        }

        // color de la alerta de bootstrap segun el resultado
        public string TipoAlerta()
        {
            if (ErrorGeneral != null) return "danger";
            if (Importados > 0 && Omitidos == 0) return "success";
            return "warning";
        }

        // guarda el resultado en TempData para mostrarlo en la lista de invitados luego de redirigir
        public void GuardarEn(ITempDataDictionary tempData)
        {
            const int maximoMensajes = 15; // limitamos los mensajes para no saturar el TempData (cookie)

            var mensajes = Detalles.Take(maximoMensajes).ToList();
            if (Detalles.Count > maximoMensajes)
            {
                mensajes.Add($"... y {Detalles.Count - maximoMensajes} mensaje(s) mas.");
            }

            tempData["ImportacionResumen"] = Resumen();
            tempData["ImportacionTipo"] = TipoAlerta();
            tempData["ImportacionDetalles"] = mensajes.ToArray();
        }
    }

    // lee un archivo de excel (.xlsx) y registra los invitados en un evento
    // se usa desde Evento/Create (invitados iniciales) y desde Invitado/PorEvento (importar mas invitados)
    public static class ImportadorInvitadosExcel
    {
        private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB
        private const int MaximoFilas = 1000;
        private const int CapacidadMinimaMesaNueva = 10; // capacidad con la que se crean las mesas que no existian

        // nombres de columna aceptados para cada campo (ya normalizados: sin tildes, sin espacios, en minuscula)
        // esto permite tambien importar el excel que se descarga desde la lista de invitados del evento
        private static readonly Dictionary<string, string> MapaEncabezados = new Dictionary<string, string>
        {
            { "nombre", "nombres" }, { "nombres", "nombres" }, { "nombrecompleto", "nombres" }, { "invitado", "nombres" },
            { "dni", "dni" }, { "documento", "dni" }, { "numerodedocumento", "dni" }, { "numerodocumento", "dni" },
            { "correo", "correo" }, { "email", "correo" }, { "correoelectronico", "correo" },
            { "telefono", "telefono" }, { "celular", "telefono" }, { "movil", "telefono" },
            { "restricciones", "restricciones" }, { "restriccionesalimentarias", "restricciones" }, { "comentario", "restricciones" }, { "alergias", "restricciones" },
            { "mesa", "mesa" }, { "numerodemesa", "mesa" }, { "nummesa", "mesa" }, { "numeromesa", "mesa" }
        };

        // validamos el archivo antes de procesarlo, devuelve el mensaje de error o null si todo esta bien
        public static string? ValidarArchivo(IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0) return "Selecciona un archivo de Excel (.xlsx).";

            if (!Path.GetExtension(archivo.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return "El archivo debe ser un Excel con formato .xlsx.";

            if (archivo.Length > TamanoMaximoBytes)
                return "El archivo no puede pesar mas de 5 MB.";

            return null;
        }

        // quita tildes, espacios y signos de un encabezado para poder compararlo
        private static string Normalizar(string texto)
        {
            var descompuesto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && char.IsLetter(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        // importa los invitados del archivo al evento indicado
        public static async Task<ResultadoImportacionInvitados> ImportarAsync(GestionEventosContext context, int idEvento, IFormFile archivo)
        {
            var resultado = new ResultadoImportacionInvitados();

            // abrimos el excel, si esta corrupto o tiene contraseña avisamos
            XLWorkbook libro;
            try
            {
                using var stream = archivo.OpenReadStream();
                libro = new XLWorkbook(stream);
            }
            catch (Exception)
            {
                resultado.ErrorGeneral = "No se pudo leer el archivo. Verifica que sea un Excel .xlsx valido y que no tenga contraseña.";
                return resultado;
            }

            using (libro)
            {
                // usamos la hoja "Invitados" si existe, si no la primera hoja del archivo
                var hoja = libro.Worksheets.FirstOrDefault(h => h.Name.Equals("Invitados", StringComparison.OrdinalIgnoreCase))
                           ?? libro.Worksheets.FirstOrDefault();
                if (hoja == null)
                {
                    resultado.ErrorGeneral = "El archivo no tiene hojas.";
                    return resultado;
                }

                // buscamos en la primera fila en que columna esta cada campo
                var columnas = new Dictionary<string, int>();
                var ultimaColumna = hoja.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
                for (int c = 1; c <= ultimaColumna; c++)
                {
                    var clave = Normalizar(hoja.Cell(1, c).GetString());
                    if (MapaEncabezados.TryGetValue(clave, out var campo) && !columnas.ContainsKey(campo))
                    {
                        columnas[campo] = c;
                    }
                }

                if (!columnas.ContainsKey("nombres") || !columnas.ContainsKey("dni"))
                {
                    resultado.ErrorGeneral = "No se encontraron las columnas obligatorias 'Nombre' y 'DNI' en la primera fila. Descarga la plantilla y usala como base.";
                    return resultado;
                }

                var ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 1;
                if (ultimaFila - 1 > MaximoFilas)
                {
                    resultado.ErrorGeneral = $"El archivo tiene mas de {MaximoFilas} filas. Divide la lista en varios archivos.";
                    return resultado;
                }

                // dnis que ya estan registrados (activos) en el evento, tambien sirve para detectar repetidos dentro del mismo archivo
                var dnisRegistrados = new HashSet<string>(
                    await context.Invitados
                        .Where(i => i.IdEvento == idEvento && i.Activo == true)
                        .Select(i => i.Dni)
                        .ToListAsync(),
                    StringComparer.OrdinalIgnoreCase);

                // mesas del evento, para poder asignar la mesa por su numero
                // si el excel trae una mesa que aun no existe en el evento, se crea automaticamente
                var mesasEvento = (await context.Mesas
                        .Where(m => m.IdEvento == idEvento && m.Activo == true)
                        .ToListAsync())
                    .GroupBy(m => m.NumeroMesa)
                    .ToDictionary(g => g.Key, g => g.First());
                var mesasCreadas = new Dictionary<int, int>(); // numero de mesa creada -> cantidad de invitados importados en ella

                var emailValidador = new EmailAddressAttribute();
                var nuevosInvitados = new List<Invitado>();
                var mesasNoValidas = new SortedSet<string>();

                // lee el texto de una celda segun el campo, devuelve "" si la columna no existe
                string Leer(string campo, int fila) =>
                    columnas.TryGetValue(campo, out var col) ? hoja.Cell(fila, col).GetString().Trim() : "";

                for (int fila = 2; fila <= ultimaFila; fila++)
                {
                    var nombres = Leer("nombres", fila);
                    var dni = Leer("dni", fila);
                    var correo = Leer("correo", fila);
                    var telefono = Leer("telefono", fila);
                    var restricciones = Leer("restricciones", fila);
                    var mesaTexto = Leer("mesa", fila);

                    // las filas totalmente vacias se ignoran sin avisar
                    if (nombres == "" && dni == "" && correo == "" && telefono == "" && restricciones == "") continue;

                    // mismas reglas que el formulario de registrar invitado
                    var errores = new List<string>();
                    if (nombres.Length < 3 || nombres.Length > 100) errores.Add("el nombre debe tener entre 3 y 100 caracteres");
                    if (dni.Length < 8 || dni.Length > 20 || !Regex.IsMatch(dni, "^[0-9A-Za-z]+$")) errores.Add("el DNI debe tener entre 8 y 20 letras o numeros");
                    if (correo != "" && (correo.Length > 100 || !emailValidador.IsValid(correo))) errores.Add("el correo no es valido");
                    if (telefono != "" && (telefono.Length > 20 || !Regex.IsMatch(telefono, @"^[\d\s\+\-]*$"))) errores.Add("el telefono no es valido");
                    if (restricciones.Length > 255) errores.Add("las restricciones superan los 255 caracteres");

                    // el dni no se puede repetir en el evento (ni en la bd ni dentro del mismo archivo)
                    if (errores.Count == 0 && !dnisRegistrados.Add(dni))
                    {
                        errores.Add($"el DNI {dni} ya esta registrado en este evento o esta repetido en el archivo");
                    }

                    if (errores.Count > 0)
                    {
                        resultado.Omitidos++;
                        resultado.Detalles.Add($"Fila {fila}: {string.Join("; ", errores)}.");
                        continue;
                    }

                    // asignamos la mesa por su numero, si la mesa no existe en el evento se crea automaticamente
                    Mesa? mesaInvitado = null;
                    if (mesaTexto != "")
                    {
                        // aceptamos "3" y tambien "3.0" (excel a veces guarda los numeros como decimales)
                        if (decimal.TryParse(mesaTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out var numeroDecimal)
                            && numeroDecimal == Math.Truncate(numeroDecimal) && numeroDecimal >= 1 && numeroDecimal <= 999)
                        {
                            int numeroMesa = (int)numeroDecimal;

                            if (!mesasEvento.TryGetValue(numeroMesa, out mesaInvitado))
                            {
                                mesaInvitado = new Mesa
                                {
                                    IdEvento = idEvento,
                                    NumeroMesa = numeroMesa,
                                    Capacidad = CapacidadMinimaMesaNueva,
                                    Activo = true
                                };
                                mesasEvento[numeroMesa] = mesaInvitado;
                                mesasCreadas[numeroMesa] = 0;
                                context.Mesas.Add(mesaInvitado);
                            }

                            // contamos cuantos invitados caen en cada mesa creada, para ajustar su capacidad
                            if (mesasCreadas.ContainsKey(numeroMesa)) mesasCreadas[numeroMesa]++;
                        }
                        else
                        {
                            mesasNoValidas.Add(mesaTexto);
                        }
                    }

                    nuevosInvitados.Add(new Invitado
                    {
                        IdEvento = idEvento,
                        IdMesaNavigation = mesaInvitado, // si la mesa es nueva, EF la guarda primero y completa el IdMesa solo
                        Nombres = nombres,
                        Dni = dni,
                        Correo = correo == "" ? null : correo,
                        Telefono = telefono == "" ? null : telefono,
                        RestriccionesAlimentarias = restricciones == "" ? null : restricciones,
                        Confirmado = false,
                        NumAcompañantes = 0,
                        Activo = true
                    });
                }

                // la capacidad de las mesas nuevas es 10 como minimo, o la cantidad de invitados que se asignaron (maximo 100, como en el formulario de mesas)
                foreach (var par in mesasCreadas)
                {
                    mesasEvento[par.Key].Capacidad = Math.Min(100, Math.Max(CapacidadMinimaMesaNueva, par.Value));
                }

                if (mesasCreadas.Count > 0)
                {
                    resultado.Detalles.Add($"Se crearon automaticamente las mesas {string.Join(", ", mesasCreadas.Keys.OrderBy(n => n))} porque no existian en el evento. Revisa su capacidad en el apartado Mesas.");
                }

                if (mesasNoValidas.Count > 0)
                {
                    resultado.Detalles.Add($"Numeros de mesa no validos ({string.Join(", ", mesasNoValidas)}), esos invitados se registraron sin mesa. Usa numeros del 1 al 999.");
                }

                if (nuevosInvitados.Count > 0)
                {
                    context.Invitados.AddRange(nuevosInvitados);
                    await context.SaveChangesAsync();
                    resultado.Importados = nuevosInvitados.Count;
                }
            }

            return resultado;
        }

        // genera la plantilla de excel que el administrador descarga para llenar la lista de invitados
        public static byte[] GenerarPlantilla()
        {
            using var wb = new XLWorkbook();

            var hoja = wb.Worksheets.Add("Invitados");
            var encabezados = new[] { "Nombre", "DNI", "Correo", "Telefono", "Restricciones", "Numero de mesa" };
            for (int i = 0; i < encabezados.Length; i++)
            {
                hoja.Cell(1, i + 1).Value = encabezados[i];
            }

            // el dni y el telefono van como texto para que excel no les quite los ceros del inicio
            hoja.Column(2).Style.NumberFormat.Format = "@";
            hoja.Column(4).Style.NumberFormat.Format = "@";

            var headerRow = hoja.Range("A1:F1");
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            hoja.Column(1).Width = 32;
            hoja.Column(2).Width = 16;
            hoja.Column(3).Width = 32;
            hoja.Column(4).Width = 16;
            hoja.Column(5).Width = 32;
            hoja.Column(6).Width = 16;

            // segunda hoja con las instrucciones (no se importa, solo se lee la hoja "Invitados")
            var ayuda = wb.Worksheets.Add("Instrucciones");
            var lineas = new[]
            {
                "INSTRUCCIONES PARA IMPORTAR INVITADOS",
                "",
                "1. Llena la hoja \"Invitados\" desde la fila 2, una fila por invitado. No cambies los encabezados de la fila 1.",
                "2. Obligatorios: Nombre (3 a 100 caracteres) y DNI (8 a 20 letras o numeros, sin espacios ni guiones).",
                "3. Opcionales: Correo, Telefono (solo numeros, espacios, + y -), Restricciones (maximo 255 caracteres).",
                "4. Numero de mesa: opcional (del 1 al 999). Si la mesa no existe en el evento, se crea automaticamente.",
                "5. Si un DNI ya esta registrado en el evento, o se repite en el archivo, esa fila se omite.",
                "6. Maximo 1000 filas por archivo y 5 MB. El formato debe ser .xlsx.",
                "",
                "Ejemplo:  Maria Lopez Perez | 12345678 | maria@correo.com | 987654321 | Vegetariana | 3"
            };
            for (int i = 0; i < lineas.Length; i++)
            {
                ayuda.Cell(i + 1, 1).Value = lineas[i];
            }
            ayuda.Cell(1, 1).Style.Font.Bold = true;
            ayuda.Column(1).Width = 110;

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }
    }
}