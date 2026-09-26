using System;
using System.Collections.Generic;

namespace DES_COGNITIVO_FUN_PROG.Models;

public partial class Evento
{
    public int IdEvento { get; set; }

    public int IdCliente { get; set; }

    public int IdEstado { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Presupuesto { get; set; }

    public DateOnly Fecha { get; set; }

    public string Lugar { get; set; } = null!;

    public string CodigoEvento { get; set; } = null!;

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual EstadoEvento IdEstadoNavigation { get; set; } = null!;

    public virtual ICollection<Invitado> Invitados { get; set; } = new List<Invitado>();

    public virtual ICollection<Mesa> Mesas { get; set; } = new List<Mesa>();
}
