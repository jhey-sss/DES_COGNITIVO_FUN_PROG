using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{

    public class AdministradorViewModel
    {
        [Key]
        public int IdAdmin {  get; set; }

        [Required]
        public string Usuario { get; set; } = null!;

        [Required]
        public string ContraseñaHash { get; set; } = null!;
    }
}
