using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    public class ClienteViewModel
    {
        [Key]
        public int IdCliente { get; set; }


        [Required(ErrorMessage = "El número de identidad es obligatorio.")]
        [Display(Name = "DNI o RUC")]
        public string NumeroIdentidad { get; set;}

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Correo { get; set; }
        [Required]
        public string Telefono { get; set; }

    }
}
