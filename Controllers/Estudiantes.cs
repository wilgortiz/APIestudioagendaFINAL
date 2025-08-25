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
    public class EstudiantesController : ControllerBase
    {
        private readonly DataContext _contexto;

        public EstudiantesController(DataContext context)
        {
            _contexto = context;
        }

        // GET: Estudiantes
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var estudiantes = await _contexto.Estudiante.ToListAsync();
                return Ok(estudiantes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // GET: Estudiantes/Obtener/{id}
        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var estudiante = await _contexto.Estudiante.FindAsync(id);
                if (estudiante == null) return NotFound();

                return Ok(estudiante);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: Estudiantes/Crear
        [HttpPost("Crear")]
        public async Task<IActionResult> Crear([FromBody] Estudiante nuevoEstudiante)
        {
            if (nuevoEstudiante == null)
            {
                return BadRequest("Los datos del estudiante son inválidos.");
            }

            try
            {
                _contexto.Estudiante.Add(nuevoEstudiante);
                await _contexto.SaveChangesAsync();
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoEstudiante.Id_Estudiante }, nuevoEstudiante);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // PUT: Estudiantes/Actualizar/{id}
        [HttpPut("Actualizar/{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Estudiante estudianteActualizado)
        {
            if (id != estudianteActualizado.Id_Estudiante)
            {
                return BadRequest("El ID del estudiante no coincide.");
            }

            try
            {
                _contexto.Entry(estudianteActualizado).State = EntityState.Modified;
                await _contexto.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstudianteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // DELETE: Estudiantes/Eliminar/{id}
        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var estudiante = await _contexto.Estudiante.FindAsync(id);
                if (estudiante == null) return NotFound();

                _contexto.Estudiante.Remove(estudiante);
                await _contexto.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private bool EstudianteExists(int id)
        {
            return _contexto.Estudiante.Any(e => e.Id_Estudiante == id);
        }
    }
}
