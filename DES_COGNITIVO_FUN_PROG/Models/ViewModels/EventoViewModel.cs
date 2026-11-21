using System;
using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    public class EventoViewModel
    {
        [Key]
        public int IdEvento { get; set; }

        [Required(ErrorMessage = "Debe asignar un cliente al evento.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un cliente válido.")]
        [Display(Name = "Cliente")]
        public int? IdCliente { get; set; }

        [Required(ErrorMessage = "Debe asignar un estado inicial.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un estado válido.")]
        [Display(Name = "Estado del Evento")]
        public int? IdEstado { get; set; }

        [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres."
        )]
        [RegularExpression(
            @"^[\p{L}\p{N}]+(?: [\p{L}\p{N}]+)*$",
            ErrorMessage = "El nombre solo puede contener letras, números y espacios entre palabras. No se permiten caracteres especiales ni espacios innecesarios."
        )]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El presupuesto es obligatorio.")]
        [Range(
            typeof(decimal),
            "0.01",
            "10000000.00",
            ErrorMessage = "El presupuesto debe ser mayor a 0 y no puede superar 10,000,000.00."
        )]
        [DataType(DataType.Currency)]
        public decimal? Presupuesto { get; set; }

        [Required(ErrorMessage = "La fecha del evento es obligatoria.")]
        [DataType(DataType.Date)]
        [DisplayFormat(
            DataFormatString = "{0:yyyy-MM-dd}",
            ApplyFormatInEditMode = true
        )]
        public DateOnly? Fecha { get; set; }

        [Required(ErrorMessage = "El lugar es obligatorio.")]
        [StringLength(
            255,
            MinimumLength = 3,
            ErrorMessage = "El lugar debe tener entre 3 y 255 caracteres."
        )]
        [RegularExpression(
            @"^[\p{L}\p{N}]+(?:[ ,.\/#-][\p{L}\p{N}]+)*$",
            ErrorMessage = "El lugar contiene caracteres no permitidos o espacios innecesarios."
        )]
        public string Lugar { get; set; } = null!;

        [StringLength(
            50,
            MinimumLength = 3,
            ErrorMessage = "El código debe tener entre 3 y 50 caracteres."
        )]
        [RegularExpression(
            @"^[a-zA-Z0-9]+$",
            ErrorMessage = "El código solo puede contener letras y números, sin espacios ni caracteres especiales."
        )]
        [Display(Name = "Código Único")]
        public string? CodigoEvento { get; set; }

        [StringLength(
            500,
            ErrorMessage = "La descripción no puede exceder los 500 caracteres."
        )]
        [RegularExpression(
            @"^[\p{L}\p{N}](?:[\p{L}\p{N} .,;:¿?¡!()/-]*[\p{L}\p{N}.!?])?$",
            ErrorMessage = "La descripción contiene caracteres no permitidos o espacios innecesarios."
        )]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        public bool? Activo { get; set; }
    }
}