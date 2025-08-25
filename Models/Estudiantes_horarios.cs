/*

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



//relacion muchos a muchos entre Estudiante y Horario
namespace API2.Models
{
    public class Estudiantes_horarios
    {
        [Key, Column(Order = 0)]
        public int Id_Estudiante { get; set; } // Llave primaria, parte de la clave compuesta

        [Key, Column(Order = 1)]
        public int Id_Horario { get; set; } // Llave primaria, parte de la clave compuesta

        // Relación con la entidad Estudiante
        [ForeignKey("Estudiante")]
        public Estudiante Estudiante { get; set; } // Relación con Estudiante

        // Relación con la entidad Horario
        [ForeignKey("Horario")]
        public Horarios Horario { get; set; } // Relación con Horario
    }
}
*/




using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API2.Models;
public class Estudiantes_horarios
{
    [Key]
    public int Id_Estudiante { get; set; } // Este es un error, debería ser otro campo

    [ForeignKey("Estudiante")]
    public int Id_Estudiante_FK { get; set; } // Cambia el nombre para indicar que es clave foránea

    [ForeignKey("Horario")]
    public int Id_Horario { get; set; }

    public virtual Estudiante Estudiante { get; set; }
    public virtual Horarios Horario { get; set; }
}
