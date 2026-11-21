using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    public class ClienteViewModel
    {
        [Key]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El número de identidad es obligatorio.")]
        [StringLength(
            20,
            MinimumLength = 8,
            ErrorMessage = "El número de identidad debe tener entre 8 y 20 dígitos."
        )]
        [RegularExpression(
            @"^\d{8,20}$",
            ErrorMessage = "El número de identidad solo puede contener números, sin letras, espacios ni caracteres especiales."
        )]
        [Display(Name = "DNI o RUC")]
        public string NumeroIdentidad { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres."
        )]
        [RegularExpression(
            @"^[\p{L}]+(?: [\p{L}]+)*$",
            ErrorMessage = "El nombre solo puede contener letras y espacios entre palabras. No se permiten números ni caracteres especiales."
        )]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [StringLength(
            100,
            ErrorMessage = "El correo no puede exceder los 100 caracteres."
        )]
        [EmailAddress(
            ErrorMessage = "Ingrese un formato de correo electrónico válido."
        )]
        [RegularExpression(
            @"^\S+@\S+\.\S+$",
            ErrorMessage = "El correo electrónico no puede contener espacios."
        )]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(
            9,
            MinimumLength = 9,
            ErrorMessage = "El teléfono debe contener exactamente 9 dígitos."
        )]
        [RegularExpression(
            @"^\d{9}$",
            ErrorMessage = "El teléfono debe contener exactamente 9 números, sin letras, espacios ni caracteres especiales."
        )]
        [DataType(DataType.PhoneNumber)]
        public string Telefono { get; set; } = null!;


        public bool Activo { get; set; }
    }
}