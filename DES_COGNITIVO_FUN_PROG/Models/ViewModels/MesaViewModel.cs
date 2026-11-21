using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    public class MesaViewModel
    {
        [Key]
        public int IdMesa { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un evento.")]
        [Display(Name = "Evento")]
        public int IdEvento { get; set; }

        [Required(ErrorMessage = "El número de mesa es obligatorio.")]
        [Range(1, 999, ErrorMessage = "El número de la mesa debe ser mayor a 0.")]
        [Display(Name = "Número de Mesa")]
        public int NumeroMesa { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria.")]
        [Range(1, 100, ErrorMessage = "La capacidad debe ser entre 1 y 100 personas.")]
        public int Capacidad { get; set; }

        [StringLength(100, ErrorMessage = "El nombre de la mesa no puede superar los 100 caracteres.")]
        [Display(Name = "Nombre de la Mesa (Opcional)")]
        public string? NombreMesa { get; set; }

        public bool? Activo { get; set; }
    }
}