using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    public class EventoViewModel
    {
        [Key]
        public int IdEvento { get; set; }

        [Required(ErrorMessage = "Debe asignar un cliente al evento.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido.")]
        [Display(Name = "Cliente")]
        public int? IdCliente { get; set; } // Agregado ?

        [Required(ErrorMessage = "Debe asignar un estado inicial.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un estado válido.")]
        [Display(Name = "Estado del Evento")]
        public int? IdEstado { get; set; } // Agregado ?

        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre del evento es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        [RegularExpression(@"^[\p{L}\p{N}]+(?: [\p{L}\p{N}]+)*$", ErrorMessage = "El nombre solo puede contener letras, números y un espacio entre palabras. No se permiten caracteres especiales ni espacios innecesarios.")]
        public string Nombre { get; set; } = null!;

        // los limites se leen siempre con punto decimal, sin importar la cultura del servidor
        [Required(ErrorMessage = "El presupuesto es obligatorio.")]
        [Range(typeof(decimal), "0.01", "10000000.00",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true,
            ErrorMessage = "El presupuesto debe ser mayor a 0 y no puede superar 10,000,000.00.")]
        [DataType(DataType.Currency)]
        public decimal? Presupuesto { get; set; } // Agregado ?

        [Required(ErrorMessage = "La fecha del evento es obligatoria.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly? Fecha { get; set; } // Agregado ?

        // letras y numeros separados por un espacio o por , . / # - (permite "Av. Larco 123, Miraflores" o "Jr. Lima #45 - Mz B")
        [Required(AllowEmptyStrings = false, ErrorMessage = "El lugar es obligatorio.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "El lugar debe tener entre 3 y 255 caracteres.")]
        [RegularExpression(@"^[\p{L}\p{N}]+(?:(?: ?[,.\/#-] ?| )[\p{L}\p{N}]+)*\.?$", ErrorMessage = "El lugar contiene caracteres no permitidos o espacios innecesarios.")]
        public string Lugar { get; set; } = null!;

        // el codigo lo genera el controlador, el usuario no lo escribe
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El código debe tener entre 3 y 50 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "El código solo puede contener letras y números, sin espacios ni caracteres especiales.")]
        [Display(Name = "Código Único")]
        public string? CodigoEvento { get; set; }

        // opcional, acepta saltos de linea porque se escribe en un cuadro de texto grande
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        [RegularExpression(@"^[\p{L}\p{N}¿¡(](?:[\p{L}\p{N} .,;:¿?¡!()/\r\n-]*[\p{L}\p{N}.!?)])?$", ErrorMessage = "La descripción contiene caracteres no permitidos o espacios innecesarios.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        public bool? Activo { get; set; }

        // ---------------- MESAS DEL EVENTO ----------------

        // atajo para generar bloques de mesas: cuantas mesas y cuantos invitados caben en cada una
        [Range(0, 300, ErrorMessage = "La cantidad de mesas debe estar entre 0 y 300.")]
        [Display(Name = "Cantidad de mesas")]
        public int? CantidadMesas { get; set; }

        [Range(1, 100, ErrorMessage = "Los invitados por mesa deben estar entre 1 y 100.")]
        [Display(Name = "Invitados por mesa")]
        public int? CapacidadPorMesa { get; set; }

        // solo en Create: reparte los invitados del excel que quedaron sin mesa en las mesas con lugares libres
        [Display(Name = "Distribuir automáticamente los invitados sin mesa")]
        public bool DistribuirInvitados { get; set; } = true;

        // bloques de mesas del evento (se editan y se envian desde el formulario)
        public List<MesaEventoViewModel> Mesas { get; set; } = new List<MesaEventoViewModel>();

        // invitados del evento con la mesa donde estan sentados (solo en Edit)
        public List<InvitadoMesaViewModel> Invitados { get; set; } = new List<InvitadoMesaViewModel>();
    }

    // una mesa dentro del formulario del evento
    // las validaciones se hacen en el controlador (MesasEventoHelper) porque las filas eliminadas no se validan
    public class MesaEventoViewModel
    {
        public int IdMesa { get; set; } // 0 = mesa nueva que aun no existe en la bd

        // identificador de la mesa dentro del formulario: el IdMesa para las existentes, "n1", "n2"... para las nuevas
        // los invitados apuntan a la mesa con esta clave, asi se pueden asignar a mesas que aun no se guardaron
        public string Clave { get; set; } = string.Empty;

        public int? NumeroMesa { get; set; }

        public string? NombreMesa { get; set; }

        public int? Capacidad { get; set; }

        public bool Eliminar { get; set; }
    }

    // un invitado dentro del formulario de editar evento
    public class InvitadoMesaViewModel
    {
        public int IdInvitado { get; set; }

        // clave de la mesa elegida, vacio = sin mesa
        public string? MesaClave { get; set; }

        // solo para mostrar (no se envian desde el formulario, el controlador los vuelve a llenar desde la bd)
        public string Nombres { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public bool Confirmado { get; set; }
        public int Acompanantes { get; set; }
    }
}