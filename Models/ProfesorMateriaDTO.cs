
namespace API2.Models;

public class ProfesorMateriaDTO
{
    public int idProfesor { get; set; }
    public ProfesoresDTO Profesor { get; set; }
    public int idMateria { get; set; }
    public MateriaDto Materia { get; set; }
}