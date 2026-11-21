using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    // ============================================================
    // VIEWMODEL PARA EL ADMINISTRADOR
    // CRUD COMPLETO DE INVITADOS
    // ============================================================
    public class InvitadoAdminViewModel
    {
        [Key]
        public int IdInvitado { get; set; }


        [Required(ErrorMessage = "Debe seleccionar un evento.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un evento válido.")]
        [Display(Name = "Evento")]
        public int IdEvento { get; set; }


        [Required(ErrorMessage = "Debe asignar una mesa.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar una mesa válida.")]
        [Display(Name = "Mesa Asignada")]
        public int? IdMesa { get; set; }


        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "Los nombres son obligatorios."
        )]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Los nombres deben tener entre 3 y 100 caracteres."
        )]
        [RegularExpression(
            @"^[\p{L}]+(?: [\p{L}]+)*$",
            ErrorMessage = "Los nombres solo pueden contener letras y espacios entre palabras. No se permiten números, caracteres especiales ni espacios innecesarios."
        )]
        public string Nombres { get; set; } = null!;


        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "El DNI es obligatorio."
        )]
        [StringLength(
            20,
            MinimumLength = 8,
            ErrorMessage = "El documento debe tener entre 8 y 20 dígitos."
        )]
        [RegularExpression(
            @"^[0-9]{8,20}$",
            ErrorMessage = "El DNI solo puede contener números, sin letras, espacios ni caracteres especiales."
        )]
        [Display(Name = "DNI")]
        public string DNI { get; set; } = null!;

        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "El correo electrónico es obligatorio."
        )]
        [StringLength(
            100,
            ErrorMessage = "El correo no puede exceder los 100 caracteres."
        )]
        [EmailAddress(
            ErrorMessage = "Ingrese un formato de correo electrónico válido."
        )]
        [RegularExpression(
            @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
            ErrorMessage = "Ingrese un correo válido y sin espacios."
        )]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = null!;

        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "El teléfono es obligatorio."
        )]
        [StringLength(
            9,
            MinimumLength = 9,
            ErrorMessage = "El teléfono debe contener exactamente 9 dígitos."
        )]
        [RegularExpression(
            @"^[0-9]{9}$",
            ErrorMessage = "El teléfono debe contener exactamente 9 números, sin letras, espacios ni caracteres especiales."
        )]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = null!;

        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "Debe especificar las restricciones alimentarias. Si no posee restricciones, escriba 'Ninguna'."
        )]
        [StringLength(
            255,
            MinimumLength = 3,
            ErrorMessage = "Las restricciones alimentarias deben contener entre 3 y 255 caracteres."
        )]
        [RegularExpression(
            @"^[\p{L}\p{N}]+(?:[ ,.();:/¿?¡!-][\p{L}\p{N}]+)*[.!?]?$",
            ErrorMessage = "Las restricciones contienen caracteres no permitidos o espacios innecesarios."
        )]
        [Display(Name = "Restricciones Alimentarias")]
        public string RestriccionesAlimentarias { get; set; } = null!;

        [Display(Name = "¿Asistencia Confirmada?")]
        public bool Confirmado { get; set; }

        [Range(
            0,
            10,
            ErrorMessage = "Solo se permiten entre 0 y 10 acompañantes."
        )]
        [Display(Name = "Acompañantes")]
        public int NumAcompañantes { get; set; } = 0;

        [Required(ErrorMessage = "La fecha de confirmación es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Confirmación")]
        [DisplayFormat(
            DataFormatString = "{0:yyyy-MM-dd}",
            ApplyFormatInEditMode = true
        )]
        public DateTime? FechaConfirmacion { get; set; }
    }

    public class BuscarInvitacionViewModel
    {
        [Required(ErrorMessage = "Debe especificar un evento.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "El evento seleccionado no es válido."
        )]
        public int IdEvento { get; set; }

        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "Por favor ingrese su número de documento (DNI)."
        )]
        [StringLength(
            20,
            MinimumLength = 8,
            ErrorMessage = "El DNI debe contener entre 8 y 20 dígitos."
        )]
        [RegularExpression(
            @"^[0-9]{8,20}$",
            ErrorMessage = "El DNI solo puede contener números, sin letras, espacios ni caracteres especiales."
        )]
        [Display(Name = "Número de Documento")]
        public string DNI { get; set; } = null!;
    }

    public class InvitadoConfirmacionViewModel
    {

        [Required(ErrorMessage = "El identificador del invitado es obligatorio.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "El identificador del invitado no es válido."
        )]
        public int IdInvitado { get; set; }

        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "El nombre del invitado es obligatorio."
        )]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Los nombres deben contener entre 3 y 100 caracteres."
        )]
        [RegularExpression(
            @"^[\p{L}]+(?: [\p{L}]+)*$",
            ErrorMessage = "Los nombres solo pueden contener letras y espacios entre palabras."
        )]
        public string Nombres { get; set; } = null!;

        [Display(Name = "¿Confirmas tu asistencia?")]
        public bool Confirmado { get; set; }

        [Range(
            0,
            10,
            ErrorMessage = "Solo se permiten de 0 a 10 acompañantes."
        )]
        [Display(Name = "Número de acompañantes")]
        public int NumAcompañantes { get; set; } = 0;

        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "Debe especificar sus alergias o restricciones. Si no tiene ninguna, escriba 'Ninguna'."
        )]
        [StringLength(
            255,
            MinimumLength = 3,
            ErrorMessage = "Las restricciones deben contener entre 3 y 255 caracteres."
        )]
        [RegularExpression(
            @"^[\p{L}\p{N}]+(?:[ ,.();:/¿?¡!-][\p{L}\p{N}]+)*[.!?]?$",
            ErrorMessage = "Las restricciones contienen caracteres no permitidos o espacios innecesarios."
        )]
        [Display(Name = "Alergias o Restricciones Alimentarias")]
        public string RestriccionesAlimentarias { get; set; } = null!;

        public List<string> CompañerosDeMesa { get; set; }
            = new List<string>();
    }
}