using System;
using System.Collections.Generic;

namespace DES_COGNITIVO_FUN_PROG.Models;

public partial class Invitado
{
    public int IdInvitado { get; set; }

    public int IdEvento { get; set; }

    public int? IdMesa { get; set; }

    public string Nombres { get; set; } = null!;

    public string Dni { get; set; } = null!;

    public bool? Confirmado { get; set; }

    public int? NumAcompañantes { get; set; }

    public DateOnly? FechaConfirmacion { get; set; }

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual Mesa? IdMesaNavigation { get; set; }
}
