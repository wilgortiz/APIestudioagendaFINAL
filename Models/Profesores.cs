using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API2.Models
{
    public class Profesores
    {
        [Key]
        public int idProfesor { get; set; }  // Llave primaria con auto-incremento

        [Required]
        public int idEstudiante { get; set; }  // Llave foránea hacia Estudiante

        // Longitud máxima de 50 caracteres para el nombre
        public string Nombre { get; set; }  // Nombre del profesor


        // Longitud máxima de 50 caracteres para el apellido
        public string? Apellido { get; set; }  // Apellido del profesor
                                               // Longitud máxima de 50 caracteres para el email
        public string? Email { get; set; }  // Email opcional del profesor


        // Longitud máxima de 50 caracteres para el celular
        public string? Celular { get; set; }  // Celular opcional del profesor

         [JsonIgnore]
        public virtual ICollection<Profesor_materia> ProfesorMateria { get; set; }
    }
}
