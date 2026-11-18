using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels 
{ 

    // VIEWMODEL PARA EL ADMINISTRADOR (CRUD Completo)

    // est o es interno y no se puede acceder desde invitado
    public class InvitadoAdminViewModel
    {
        [Key]
        public int IdInvitado { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un evento.")]
        [Display(Name = "Evento")]
        public int IdEvento { get; set; }

        [Display(Name = "Mesa Asignada")]
        public int? IdMesa { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Los nombres deben tener entre 3 y 100 caracteres.")]
        public string Nombres { get; set; } = null!;

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "El documento debe tener entre 8 y 20 caracteres.")]
        [RegularExpression(@"^[0-9A-Za-z]+$", ErrorMessage = "Solo letras y números permitidos.")]
        public string DNI { get; set; } = null!;

        [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string? Correo { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
        [RegularExpression(@"^[\d\s\+\-]*$", ErrorMessage = "Formato de teléfono no válido.")]
        [DataType(DataType.PhoneNumber)]
        public string? Telefono { get; set; }

        [StringLength(255, ErrorMessage = "El texto no puede exceder los 255 caracteres.")]
        [Display(Name = "Restricciones Alimentarias")]
        public string? RestriccionesAlimentarias { get; set; }

        [Display(Name = "¿Asistencia Confirmada?")]
        public bool Confirmado { get; set; }

        [Required(ErrorMessage = "Especifique el número de acompañantes.")]
        [Range(0, 10, ErrorMessage = "El máximo permitido es 10 acompañantes.")]
        [Display(Name = "Acompañantes")]
        public int NumAcompañantes { get; set; } = 0;

        [DataType(DataType.Date)]
        public DateTime? FechaConfirmacion { get; set; }
    }


    // VIEWMODELS PARA EL INVITADO (Acceso y Confirmación)

    //plantiulla para el formularioo del cliente, esto si es visinle por el invitado, para realizar su registro
    public class BuscarInvitacionViewModel
    {
        public int IdEvento { get; set; }

        [Required(ErrorMessage = "Por favor ingrese su número de documento (DNI).")]
        [StringLength(20, ErrorMessage = "El DNI no puede ser mayor a 20 caracteres.")]
        [Display(Name = "Número de Documento")]
        public string DNI { get; set; } = null!;
    }

    public class InvitadoConfirmacionViewModel
    {
        public int IdInvitado { get; set; }

        public string Nombres { get; set; } = null!; // Solo lectura en la vista

        [Display(Name = "¿Confirmas tu asistencia?")]
        public bool Confirmado { get; set; }

        [Required(ErrorMessage = "Especifique la cantidad de acompañantes.")]
        [Range(0, 10, ErrorMessage = "Solo se permiten de 0 a 10 acompañantes.")]
        [Display(Name = "Número de acompañantes")]
        public int NumAcompañantes { get; set; } = 0;

        [StringLength(255, ErrorMessage = "Por favor resuma sus restricciones en menos de 255 caracteres.")]
        [Display(Name = "Alergias o Restricciones Alimentarias")]
        public string? RestriccionesAlimentarias { get; set; }

        public List<string> CompañerosDeMesa { get; set; } = new List<string>();
    }
}
