using API2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CalificacionesController : ControllerBase
    {
        private readonly DataContext _contexto;

        public CalificacionesController(DataContext context)
        {
            _contexto = context;
        }

        // GET: Calificaciones
        [Authorize]
        [HttpGet("obtenerNotas")]
        public async Task<IActionResult> ObtenerNotas()
        {
            try
            {
                var usuarioId = (int)HttpContext.Items["usuarioId"];
                var calificaciones = await _contexto.Calificaciones
                    .Include(c => c.Estudiante)
                    .Include(c => c.Materia)
                    .Where(c => c.idEstudiante == usuarioId)
                    .Select(c => new
                    {
                        IdCalificacion = c.idCalificacion,
                        IdEstudiante = c.idEstudiante,
                        Estudiante = new
                        {
                            Id_Estudiante = c.Estudiante.Id_Estudiante,
                            Nombre = c.Estudiante.Nombre
                        },
                        IdMateria = c.idMateria,
                        Materia = new
                        {
                            IdMateria = c.Materia.idMateria,
                            Nombre = c.Materia.Nombre
                        },
                        TipoEvaluacion = c.TipoEvaluacion,
                        Calificacion = c.Calificacion,
                        //Fecha = c.Fecha.HasValue ? c.Fecha.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null
                    })
                    .ToListAsync();

                return Ok(calificaciones);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error interno: {e.Message}");
            }
        }


        [Authorize]
        [HttpGet("obtenerNotasAgrupadas")]
        public async Task<IActionResult> ObtenerNotasAgrupadas()
        {
            try
            {
                var usuarioId = (int)HttpContext.Items["usuarioId"];
                var calificaciones = await _contexto.Calificaciones
                    .Include(c => c.Materia)
                    .Where(c => c.idEstudiante == usuarioId)
                    .GroupBy(c => new { c.idMateria, c.Materia.Nombre })
                    .Select(g => new
                    {
                        Materia = new
                        {
                            IdMateria = g.Key.idMateria,
                            Nombre = g.Key.Nombre
                        },
                        Notas = g.Select(c => c.Calificacion).ToList(),
                        Promedio = g.Average(c => c.Calificacion)
                    })
                    .ToListAsync();

                return Ok(calificaciones);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error interno: {e.Message}");
            }
        }





        // GET: Calificaciones/Obtener/{id}
        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var calificacion = await _contexto.Calificaciones
                    .Include(c => c.Estudiante)
                    .Include(c => c.Materia)
                    .FirstOrDefaultAsync(c => c.idCalificacion == id);

                if (calificacion == null) return NotFound();

                return Ok(calificacion);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }










        // POST: Calificaciones/Crear
        [Authorize]
        [HttpPost("AgregarNotas")]
        public async Task<IActionResult> AgregarNotas([FromBody] Calificaciones nuevaNota)
        {
            try
            {
                var usuarioId = HttpContext.Items["usuarioId"]; // Obtiene el ID del usuario desde el token
                nuevaNota.idEstudiante = (int)usuarioId;
                Console.WriteLine("el id del estudiante" + usuarioId);

                Console.WriteLine($"Id_Materia: {nuevaNota.idMateria}");
                // Verifica si la materia existe
                var materia = await _contexto.Materia.FindAsync(nuevaNota.idMateria);
                if (materia == null)
                {
                    return BadRequest("La materia seleccionada no existe");
                }

                _contexto.Calificaciones.Add(nuevaNota);
                Console.WriteLine($"Datos recibidos: IdMateria={nuevaNota.idMateria}, Calificacion={nuevaNota.Calificacion}");
                await _contexto.SaveChangesAsync();
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaNota.idCalificacion }, nuevaNota);
            }
            catch (DbUpdateException e)
            {
                return BadRequest($"Error al guardar la falta: {e.InnerException?.Message}");
            }
            catch (Exception e)
            {
                return BadRequest($"Error inesperado: {e.Message}");
            }
        }







        //ACTUALIZAR NOTAS
        [Authorize]
        [HttpPut("actualizarNotas")]
        public async Task<IActionResult> ActualizarNotas([FromBody] Calificaciones c)
        {
            try
            {
                // 1. Verificar que el usuario está autenticado
                if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                {
                    return Unauthorized("Usuario no identificado");
                }
                int usuarioId = (int)usuarioIdObj;

                // 2. Verificar que el profesor existe y pertenece al usuario
                var notaExistente = await _contexto.Calificaciones
                        .FirstOrDefaultAsync(p => p.idCalificacion == c.idCalificacion && p.idEstudiante == usuarioId);
                if (notaExistente == null)
                {
                    return NotFound("contacto no encontrado o no tienes permiso para editarlo");
                }

                // 3. Actualizar todos los campos 
                notaExistente.Calificacion= c.Calificacion;
                

                // 4. Guardar cambios con manejo de concurrencia
                try
                {
                    await _contexto.SaveChangesAsync();
                    return Ok(notaExistente);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        return NotFound("la nota ya no existe en la base de datos");
                    }

                    return Conflict("la nota fue modificada por otro usuario. Recarga los datos e intenta nuevamente.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error interno: {e.Message}");
            }
        }
        





        // DELETE: Calificaciones/Eliminar/{id}
        [Authorize]
        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var calificacion = await _contexto.Calificaciones.FindAsync(id);
                if (calificacion == null) return NotFound();

                _contexto.Calificaciones.Remove(calificacion);
                await _contexto.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private bool CalificacionExists(int id)
        {
            return _contexto.Calificaciones.Any(e => e.idCalificacion == id);
        }
    }
}
