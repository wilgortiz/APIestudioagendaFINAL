using System.ComponentModel.DataAnnotations;

namespace API2.Models;


public class MateriaDto
{

    public int idMateria { get; set; }

    [Required]
    [StringLength(25)]
    public string Nombre { get; set; }

    [Required]
    public int Periodo { get; set; }
    
     //campo para los IDs de profesores
    public List<int>? ProfesoresIds { get; set; }
}
