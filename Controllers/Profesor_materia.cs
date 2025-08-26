using API2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProfesorMateriaController : ControllerBase
    {
        private readonly DataContext contexto;

        public ProfesorMateriaController(DataContext context)
        {
            contexto = context;
        }

        // GET: ProfesorMateria
        [HttpGet]
        public IActionResult ObtenerProfesorMaterias()
        {
            var profesorMaterias = contexto.Profesor_materia.Include(pm => pm.Profesor).Include(pm => pm.Materia).ToList();
            return Ok(profesorMaterias);
        }

        // GET: ProfesorMateria/{idProfesor}/{idMateria}
        [HttpGet("{idProfesor}/{idMateria}")]
        public IActionResult ObtenerProfesorMateria(int idProfesor, int idMateria)
        {
            var profesorMateria = contexto.Profesor_materia.FirstOrDefault(pm => pm.idProfesor == idProfesor && pm.idMateria == idMateria);
            if (profesorMateria == null) return NotFound();
            return Ok(profesorMateria);
        }




        // POST: ProfesorMateria/AsignarProfesorMateria
        [HttpPost("AsignarProfesorMateria")]
        public IActionResult CrearProfesorMateria([FromBody] AsignarProfesorMateriaDTO nuevoProfesorMateria)
        {
            if (nuevoProfesorMateria == null) return BadRequest("ProfesorMateria no puede ser nulo.");

            var profesorMateria = new Profesor_materia
            {
                idProfesor = nuevoProfesorMateria.idProfesor,
                idMateria = nuevoProfesorMateria.idMateria
            };

            contexto.Profesor_materia.Add(profesorMateria);
            contexto.SaveChanges();
            return CreatedAtAction(nameof(ObtenerProfesorMateria), new { idProfesor = profesorMateria.idProfesor, idMateria = profesorMateria.idMateria }, profesorMateria);
        }



        [HttpGet("ObtenerMateriasConProfesores")]
        public IActionResult ObtenerMateriasConProfesores()
        {
            var materiasConProfesores = contexto.Profesor_materia
                .Include(pm => pm.Profesor)
                .Include(pm => pm.Materia)
                .Select(pm => new ProfesorMateriaDTO
                {
                    idProfesor = pm.idProfesor,
                    Profesor = new ProfesoresDTO
                    {
                        Nombre = pm.Profesor.Nombre,
                        Apellido = pm.Profesor.Apellido,
                        Email = pm.Profesor.Email,
                        Celular = pm.Profesor.Celular
                    },
                    idMateria = pm.idMateria,
                    Materia = new MateriaDto
                    {
                        Nombre = pm.Materia.Nombre,
                        Periodo = pm.Materia.Periodo
                    }
                })
                .ToList();

            return Ok(materiasConProfesores);
        }



        // GET: ProfesorMateria/ObtenerMateriaConProfesores/{idMateria}
        [HttpGet("ObtenerMateriaConProfesores/{idMateria}")]
        public IActionResult ObtenerMateriaConProfesores(int idMateria)
        {
            var materiaConProfesores = contexto.Profesor_materia
                .Include(pm => pm.Materia)
                .Include(pm => pm.Profesor)
                .Where(pm => pm.idMateria == idMateria)
                .ToList();

            return Ok(materiaConProfesores);
        }



        // DELETE: ProfesorMateria/{idProfesor}/{idMateria}
        [HttpDelete("{idProfesor}/{idMateria}")]
        public IActionResult EliminarProfesorMateria(int idProfesor, int idMateria)
        {
            var profesorMateria = contexto.Profesor_materia.FirstOrDefault(pm => pm.idProfesor == idProfesor && pm.idMateria == idMateria);
            if (profesorMateria == null) return NotFound();

            contexto.Profesor_materia.Remove(profesorMateria);
            contexto.SaveChanges();
            return NoContent();
        }
    }
}
