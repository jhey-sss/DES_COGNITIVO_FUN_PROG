using System;
using System.Collections.Generic;

namespace DES_COGNITIVO_FUN_PROG.Models;

public partial class EstadoEvento
{
    public int IdEstado { get; set; }

    public string EstadoEvento1 { get; set; } = null!;

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();
}
