using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API2.Models
{
    public class Contactos
    {
        [Key]
        public int idContacto { get; set; }

        public int idEstudiante { get; set; }

        [ForeignKey(nameof(idEstudiante))]
        public Estudiante? Estudiante { get; set; }  // Relación con la tabla Estudiante

      
        public string Nombre { get; set; }

       
        public string Apellido { get; set; }

        
        public string? Email { get; set; }

         // Longitud máxima de 50 caracteres para el celular
        public string? Celular { get; set; }  // Celular opcional del profesor


    }
}