using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    public class EventoViewModel
    {
        [Key]
        public int IdEvento { get; set; }

        [Required(ErrorMessage = "Debe asignar un cliente al evento.")]
        [Display(Name = "Cliente")]
        public int? IdCliente { get; set; } // Agregado ?

        [Required(ErrorMessage = "Debe asignar un estado inicial.")]
        [Display(Name = "Estado del Evento")]
        public int? IdEstado { get; set; } // Agregado ?

        [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El presupuesto es obligatorio.")]
        [Range(0.01, 10000000.00, ErrorMessage = "Ingrese un presupuesto válido mayor a 0.")]
        [DataType(DataType.Currency)]
        public decimal? Presupuesto { get; set; } // Agregado ?

        [Required(ErrorMessage = "La fecha del evento es obligatoria.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly? Fecha { get; set; } // Agregado ?

        [Required(ErrorMessage = "El lugar es obligatorio.")]
        [StringLength(255, ErrorMessage = "El lugar no puede exceder los 255 caracteres.")]
        public string Lugar { get; set; } = null!;

        [StringLength(50, ErrorMessage = "El código no puede exceder los 50 caracteres.")]
        [Display(Name = "Código Único")]
        public string? CodigoEvento { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        public bool? Activo { get; set; }
    }
}