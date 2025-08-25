using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API2.Models
{
    public class Estudiante
    {
        [Key]
        public int Id_Estudiante { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(50)]
        public string Apellido { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Clave { get; set; }

        // Relación con Estudiantes_horarios
        public virtual ICollection<Estudiantes_horarios> EstudiantesHorarios { get; set; }
        //public string Token { get; internal set; }
    }
}
