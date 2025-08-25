using System.ComponentModel.DataAnnotations;

namespace API2.Models

{
    public class ProfesoresDTO
    {
       public int idProfesor {get;set;}
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        public string? Celular { get; set; }
    }
}