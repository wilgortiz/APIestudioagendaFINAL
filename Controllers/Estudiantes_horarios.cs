using API2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EstudiantesHorariosController : ControllerBase
    {
        private readonly DataContext _contexto;

        public EstudiantesHorariosController(DataContext context)
        {
            _contexto = context;
        }

        // GET: EstudiantesHorarios
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var estudiantesHorarios = await _contexto.Estudiantes_horarios
                    .Include(e => e.Estudiante)
                    .Include(h => h.Horario)
                    .ToListAsync();
                return Ok(estudiantesHorarios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // GET: EstudiantesHorarios/Obtener/{idEstudiante}
        [HttpGet("Obtener/{idEstudiante}")]
        public async Task<IActionResult> ObtenerPorEstudiante(int idEstudiante)
        {
            try
            {
                var horarios = await _contexto.Estudiantes_horarios
                    .Where(eh => eh.Id_Estudiante == idEstudiante)
                    .Include(h => h.Horario)
                    .ToListAsync();

                if (horarios == null || !horarios.Any()) return NotFound();

                return Ok(horarios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: EstudiantesHorarios/Agregar
        [HttpPost("Agregar")]
        public async Task<IActionResult> Agregar([FromBody] Estudiantes_horarios nuevoEstudianteHorario)
        {
            if (nuevoEstudianteHorario == null)
            {
                return BadRequest("Los datos de la relación son inválidos.");
            }

            try
            {
                _contexto.Estudiantes_horarios.Add(nuevoEstudianteHorario);
                await _contexto.SaveChangesAsync();
                return CreatedAtAction(nameof(ObtenerPorEstudiante), new { idEstudiante = nuevoEstudianteHorario.Id_Estudiante }, nuevoEstudianteHorario);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // DELETE: EstudiantesHorarios/Eliminar
        [HttpDelete("Eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] Estudiantes_horarios estudianteHorario)
        {
            if (estudianteHorario == null)
            {
                return BadRequest("Los datos de la relación son inválidos.");
            }

            try
            {
                var relacion = await _contexto.Estudiantes_horarios
                    .FindAsync(estudianteHorario.Id_Estudiante, estudianteHorario.Id_Horario);

                if (relacion == null) return NotFound();

                _contexto.Estudiantes_horarios.Remove(relacion);
                await _contexto.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
