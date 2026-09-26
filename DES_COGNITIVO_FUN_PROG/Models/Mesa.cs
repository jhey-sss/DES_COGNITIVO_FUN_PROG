using System;
using System.Collections.Generic;

namespace DES_COGNITIVO_FUN_PROG.Models;

public partial class Mesa
{
    public int IdMesa { get; set; }

    public int IdEvento { get; set; }

    public int NumeroMesa { get; set; }

    public int Capacidad { get; set; }

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual ICollection<Invitado> Invitados { get; set; } = new List<Invitado>();
}
