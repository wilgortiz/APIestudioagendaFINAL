using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API2.Models
{
    public class Materia
    {
        [Key]
        public int idMateria { get; set; }  // Llave primaria con auto-incremento

        [Required]
        [Column("IdEstudiante")]
        public int IdEstudiante { get; set; }  // Llave foránea hacia Estudiante

        [ForeignKey(nameof(IdEstudiante))]
        public Estudiante? Estudiante { get; set; }  // Relación con la tabla Estudiante

        [Required]
        //[StringLength(25)]  // Longitud máxima de 25 caracteres para el nombre de la materia
        [Column("Nombre")]
        public string Nombre { get; set; }

        [Required]
         [Column("Periodo")]
        public int Periodo { get; set; }  // Período de la materia

        public virtual ICollection<Profesor_materia> ProfesorMateria { get; set; }

    }
}
