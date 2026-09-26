using System;
using System.Collections.Generic;

namespace DES_COGNITIVO_FUN_PROG.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string NumeroIdentidad { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Telefono { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();
}
