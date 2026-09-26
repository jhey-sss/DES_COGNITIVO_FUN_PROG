using System;
using System.Collections.Generic;

namespace DES_COGNITIVO_FUN_PROG.Models;

public partial class Administrador
{
    public int IdAdmin { get; set; }

    public string Usuario { get; set; } = null!;

    public string ContraseñaHash { get; set; } = null!;
}
