using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API2.Models
{
    public class Apuntes
    {
        [Key]
        [Column("id_apunte")]
        public int IdApunte { get; set; } // Llave primaria

        [ForeignKey("Estudiante")]
        [Column("Id_Estudiante")]
        public int? IdEstudiante { get; set; } // Clave foránea de estudiante, puede ser nulo
        public Estudiante? Estudiante { get; set; } // Relación con la entidad Estudiante

        [ForeignKey("Materia")]
        [Column("Id_Materia")]
        public int? IdMateria { get; set; } // Clave foránea de materia, puede ser nulo
        public Materia? Materia { get; set; } // Relación con la entidad Materia

        [MaxLength(255)]
        public string? Titulo { get; set; } // Campo opcional para el título

        public string? Descripcion { get; set; } // Campo opcional para la descripción (tipo TEXT)

[Column("fechaCreacion")]
public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
