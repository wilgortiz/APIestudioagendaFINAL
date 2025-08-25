using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API2.Models
{
    public class Actividades
    {
        [Key]
        public int idEvento { get; set; }  // Llave primaria con auto-incremento


        public int? Id_Materia { get; set; }  // Llave foránea hacia Materia

        [ForeignKey(nameof(Id_Materia))]
        public Materia? Materia { get; set; }  // Relación con la tabla Materia

        [Required]
        public int Id_Estudiante { get; set; }  // Llave foránea hacia Estudiante

        [ForeignKey(nameof(Id_Estudiante))]
        public Estudiante? Estudiante { get; set; }  // Relación con la tabla Estudiante

        [Required]
        // [StringLength(20)]  // Longitud máxima de 20 caracteres para el tipo de evento
        //public string? Tipo_Evento { get; set; }
        [JsonPropertyName("Tipo_Evento")]
        public string? Tipo_Evento { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha_Evento { get; set; }  // Fecha del evento

        // [StringLength(150)]
        public string? Descripcion { get; set; }  // Descripción opcional del evento

        public bool Recordatorio { get; set; } = false;  // Indicador de recordatorio

        public DateTime? Fecha_Recordatorio { get; set; }  // Fecha y hora del recordatorio, si es que existe
    }
}
