using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    public class ClienteViewModel
    {
        [Key]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El número de identidad es obligatorio.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "El documento debe tener entre 8 y 20 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Solo se permiten números y letras sin espacios.")]
        [Display(Name = "DNI o RUC")]
        public string NumeroIdentidad { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres.")]
        [EmailAddress(ErrorMessage = "Ingrese un formato de correo electrónico válido.")]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
        [RegularExpression(@"^[\d\s\+\-]*$", ErrorMessage = "Ingrese un número de teléfono válido.")]
        [DataType(DataType.PhoneNumber)]
        public string Telefono { get; set; } = null!;

        public bool Activo { get; set; }
    }
}