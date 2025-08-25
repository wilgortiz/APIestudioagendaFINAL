using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API2.Models
{
    public class Horarios
    {
        [Key]
        public int idHorario { get; set; }  // Llave primaria con auto-incremento
        public int idEstudiante { get; set; }  // Llave foránea hacia Estudiante

        [ForeignKey(nameof(idEstudiante))]
        public Estudiante? Estudiante { get; set; }  // Relación con la tabla Estudiante
        public int idMateria { get; set; }  // Llave foránea hacia Materia

        [ForeignKey(nameof(idMateria))]
        public Materia? Materia { get; set; }  // Relación con la tabla Materia

        // Longitud máxima de 20 caracteres para el día
        public string diaSemana { get; set; }  // Día de la semana

        [DataType(DataType.Time)]
        public TimeSpan horaInicio { get; set; }  // Hora de inicio de la clase

        [DataType(DataType.Time)]
        public TimeSpan horaFin { get; set; }  // Hora de fin de la clase

        // Relación con Estudiantes_horarios
       // public virtual ICollection<Estudiantes_horarios> EstudiantesHorarios { get; set; }
    }
}
  