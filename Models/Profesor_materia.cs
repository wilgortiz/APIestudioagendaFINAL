using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API2.Models;


public class Profesor_materia
{
    public int idProfesor { get; set; }

     [JsonIgnore]
    public Profesores Profesor { get; set; }
    public int idMateria { get; set; }
    public Materia Materia { get; set; }

}

