namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    // VIEWMODELS PARA LA VISTA DE RESUMEN VISUAL DEL EVENTO (mesas, invitados e informacion general)

    // datos completos de un evento para la pantalla de resumen
    public class EventoResumenViewModel
    {
        // informacion general del evento
        public int IdEvento { get; set; }
        public string CodigoEvento { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public DateOnly Fecha { get; set; }
        public string Lugar { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal Presupuesto { get; set; }
        public string NombreEstado { get; set; } = null!;

        // datos del cliente dueño del evento
        public string NombreCliente { get; set; } = null!;
        public string? CorreoCliente { get; set; }
        public string? TelefonoCliente { get; set; }

        // contadores de invitados
        public int TotalInvitados { get; set; }
        public int TotalConfirmados { get; set; }
        public int TotalPendientes { get; set; }
        public int PersonasConfirmadas { get; set; } // invitados confirmados + sus acompañantes
        public int PersonasEnTotal { get; set; }     // todos los invitados + todos sus acompañantes

        // contadores de mesas
        public int TotalMesas { get; set; }
        public int CapacidadTotal { get; set; }
        public int LugaresOcupados { get; set; }     // lugares usados en mesas (invitado + acompañantes)
        public int InvitadosSinMesa { get; set; }

        // porcentajes para las barras de progreso
        public int PorcentajeConfirmacion => TotalInvitados == 0 ? 0 : (int)Math.Round(TotalConfirmados * 100.0 / TotalInvitados);
        public int PorcentajeOcupacion => CapacidadTotal == 0 ? 0 : (int)Math.Min(100, Math.Round(LugaresOcupados * 100.0 / CapacidadTotal));

        public List<ResumenMesaViewModel> Mesas { get; set; } = new List<ResumenMesaViewModel>();

        // invitados que todavia no tienen mesa (se muestran como una "mesa" aparte)
        public ResumenMesaViewModel? SinMesa { get; set; }

        // todos los invitados del evento en una sola lista (pestaña de invitados)
        public List<ResumenInvitadoViewModel> Invitados { get; set; } = new List<ResumenInvitadoViewModel>();

        // restricciones alimentarias agrupadas (pestaña de informacion)
        public List<ResumenRestriccionViewModel> Restricciones { get; set; } = new List<ResumenRestriccionViewModel>();
    }

    // una mesa del evento con sus invitados
    public class ResumenMesaViewModel
    {
        public int IdMesa { get; set; }          // 0 = invitados sin mesa
        public int NumeroMesa { get; set; }
        public string? NombreMesa { get; set; }
        public int Capacidad { get; set; }
        public int LugaresOcupados { get; set; } // cada invitado ocupa 1 lugar mas los de sus acompañantes
        public int Confirmados { get; set; }
        public int Pendientes { get; set; }
        public List<ResumenInvitadoViewModel> Invitados { get; set; } = new List<ResumenInvitadoViewModel>();

        public int LugaresLibres => Math.Max(0, Capacidad - LugaresOcupados);
        public int Sobrecupo => Math.Max(0, LugaresOcupados - Capacidad);
        public int PorcentajeOcupacion => Capacidad == 0 ? 0 : (int)Math.Min(100, Math.Round(LugaresOcupados * 100.0 / Capacidad));

        // texto para mostrar, por ejemplo "Mesa 3" o "Mesa 3 · Familia Perez"
        public string Titulo => IdMesa == 0
            ? "Sin mesa asignada"
            : string.IsNullOrWhiteSpace(NombreMesa) ? $"Mesa {NumeroMesa}" : $"Mesa {NumeroMesa} · {NombreMesa}";
    }

    // un invitado dentro del resumen
    public class ResumenInvitadoViewModel
    {
        public int IdInvitado { get; set; }
        public int? NumeroMesa { get; set; }
        public string Nombres { get; set; } = null!;
        public string Dni { get; set; } = null!;
        public bool Confirmado { get; set; }
        public int NumAcompañantes { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? RestriccionesAlimentarias { get; set; }

        public string TextoMesa => NumeroMesa.HasValue ? $"Mesa {NumeroMesa}" : "Sin mesa";
    }

    // una restriccion alimentaria y los invitados que la tienen
    public class ResumenRestriccionViewModel
    {
        public string Restriccion { get; set; } = null!;
        public List<string> Invitados { get; set; } = new List<string>();
    }
}