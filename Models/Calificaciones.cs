using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API2.Models
{
    public class Calificaciones
    {
        [Key]
        public int idCalificacion { get; set; } // Llave primaria

       
        public int idEstudiante { get; set; }  // Llave foránea hacia Estudiante

        [ForeignKey(nameof(idEstudiante))]
        public Estudiante? Estudiante { get; set; }  // Relación con la tabla Estudiante

        public int idMateria { get; set; }

        [ForeignKey(nameof(idMateria))]
        public Materia? Materia { get; set; }  // Relación con la tabla Materia


        public string? TipoEvaluacion { get; set; } // Campo opcional para el tipo de evaluación

        public float? Calificacion { get; set; } // Calificación, no puede ser nula

        public DateTime? Fecha { get; set; } // Fecha de la calificación, no puede ser nula
    }
}
