using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace API2.Models
{
    public class Faltas
    {
        [Key]
        public int IdFalta { get; set; }

        [Required]
        public int IdEstudiante { get; set; }  // Llave foránea hacia Estudiante

        [ForeignKey(nameof(IdEstudiante))]
        public Estudiante? Estudiante { get; set; }  // Relación con la tabla Estudiante

        public int idMateria { get; set; }

        [ForeignKey(nameof(idMateria))]
        public Materia? Materia { get; set; }  // Relación con la tabla Materia

        [DataType(DataType.Date)]
        public DateTime? FechaFalta { get; set; }  // Fecha en la que se registró la falta

        public int? Cantidad { get; set; }

        public bool? Justificada { get; set; }   // Indicador de si la falta está justificada

        public int FaltasPermitidas { get; set; }
    }
}
