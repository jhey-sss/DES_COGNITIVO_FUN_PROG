using DES_COGNITIVO_FUN_PROG.Models;
using DES_COGNITIVO_FUN_PROG.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DES_COGNITIVO_FUN_PROG.Helpers
{
    // reglas de las mesas que se manejan desde los formularios de evento (crear y editar)
    public static class MesasEventoHelper
    {
        public const int MaximoMesasPorEvento = 300;

        // cada invitado ocupa un lugar, mas los lugares de sus acompañantes
        public static int Ocupa(Invitado i) => 1 + (i.NumAcompañantes ?? 0);

        // genera n mesas numeradas desde 1, todas con la misma capacidad (atajo "cantidad de mesas" + "invitados por mesa")
        public static List<MesaEventoViewModel> Generar(int cantidad, int capacidad)
        {
            var lista = new List<MesaEventoViewModel>();
            for (int n = 1; n <= cantidad; n++)
            {
                lista.Add(new MesaEventoViewModel
                {
                    IdMesa = 0,
                    Clave = "n" + n,
                    NumeroMesa = n,
                    Capacidad = capacidad
                });
            }
            return lista;
        }

        // valida las mesas que se van a guardar (las marcadas como eliminadas no se envian aqui), devuelve la lista de errores
        public static List<string> ValidarMesas(IReadOnlyList<MesaEventoViewModel> mesas)
        {
            var errores = new List<string>();

            if (mesas.Count > MaximoMesasPorEvento)
                errores.Add($"Un evento no puede tener más de {MaximoMesasPorEvento} mesas.");

            foreach (var m in mesas)
            {
                if (m.NumeroMesa == null || m.NumeroMesa < 1 || m.NumeroMesa > 999)
                {
                    errores.Add("Todas las mesas deben tener un número entre 1 y 999.");
                    break;
                }
            }

            foreach (var m in mesas)
            {
                if (m.Capacidad == null || m.Capacidad < 1 || m.Capacidad > 100)
                {
                    errores.Add(m.NumeroMesa != null
                        ? $"La capacidad de la mesa {m.NumeroMesa} debe estar entre 1 y 100 invitados."
                        : "La capacidad de cada mesa debe estar entre 1 y 100 invitados.");
                }

                if (m.NombreMesa != null && m.NombreMesa.Trim().Length > 100)
                    errores.Add($"El nombre de la mesa {m.NumeroMesa} no puede superar los 100 caracteres.");
            }

            var repetidos = mesas
                .Where(m => m.NumeroMesa != null)
                .GroupBy(m => m.NumeroMesa!.Value)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .OrderBy(n => n);
            foreach (var numero in repetidos)
                errores.Add($"El número de mesa {numero} está repetido en el evento.");

            return errores.Distinct().ToList();
        }

        // reparte los invitados activos que no tienen mesa en las mesas del evento que aun tienen lugares libres
        // devuelve cuantos se asignaron y cuantos se quedaron sin mesa por falta de lugares
        public static async Task<(int Asignados, int SinLugar)> DistribuirSinMesaAsync(GestionEventosContext context, int idEvento)
        {
            var mesas = await context.Mesas
                .Where(m => m.IdEvento == idEvento && m.Activo == true)
                .OrderBy(m => m.NumeroMesa)
                .ToListAsync();

            var invitados = await context.Invitados
                .Where(i => i.IdEvento == idEvento && i.Activo == true)
                .OrderBy(i => i.IdInvitado)
                .ToListAsync();

            // lugares libres de cada mesa segun los invitados que ya tiene
            var libres = mesas.ToDictionary(
                m => m.IdMesa,
                m => m.Capacidad - invitados.Where(i => i.IdMesa == m.IdMesa).Sum(Ocupa));

            int asignados = 0, sinLugar = 0;
            foreach (var invitado in invitados.Where(i => i.IdMesa == null))
            {
                int necesita = Ocupa(invitado);
                var mesa = mesas.FirstOrDefault(m => libres[m.IdMesa] >= necesita);
                if (mesa == null)
                {
                    sinLugar++;
                    continue;
                }

                invitado.IdMesa = mesa.IdMesa;
                libres[mesa.IdMesa] -= necesita;
                asignados++;
            }

            if (asignados > 0) await context.SaveChangesAsync();
            return (asignados, sinLugar);
        }
    }
}
