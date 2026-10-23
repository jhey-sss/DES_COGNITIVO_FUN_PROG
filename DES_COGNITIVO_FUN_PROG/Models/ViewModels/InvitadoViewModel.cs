using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    public class InvitadoViewModel
    {
        [Key]
        public int IdInvitado { get; set; }
        [Required]
        public int IdEvento { get; set; }
        [Required]
        public int? IdMesa { get; set; }
        [Required]
        public string Nombres { get; set; } = null!;
        [Required]
        public string Dni { get; set; } = null!;
        [Required]
        public bool? Confirmado { get; set; }
        [Required]
        public int? NumAcompañantes { get; set; }
        [Required]
        public DateOnly? FechaConfirmacion { get; set; }
    }
}
