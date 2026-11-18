using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    public class AdministradorViewModel
    {
        [Key]
        public int IdAdmin { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string ContraseñaHash { get; set; } = null!;
    }
}