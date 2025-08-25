namespace API2.Models;

//esto se hace en relaciones muchops a muchos
//donde se le asigna un profesor a una materia 
//tomando los 2 id de las tablas que la componen
//y luego se crea una instancia de la clase Profesor_materia
public class AsignarProfesorMateriaDTO
{
    public int idProfesor { get; set; }
    public int idMateria { get; set; }
}