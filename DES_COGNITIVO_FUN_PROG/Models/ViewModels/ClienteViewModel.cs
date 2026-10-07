using System.ComponentModel.DataAnnotations;
namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
//En este archivo van todas las validaciones que se agregan antes de enviar los datos a al vista
{
    public class ClienteViewModel // Data Annotations
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
        public bool Activo { get; set; }

    }
}
