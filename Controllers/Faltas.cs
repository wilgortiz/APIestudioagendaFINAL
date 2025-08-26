using API2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FaltasController : ControllerBase
    {
        private readonly DataContext _contexto;
        private readonly ILogger<FaltasController> _logger;


        public FaltasController(DataContext context, ILogger<FaltasController> logger)
        {
            _contexto = context;
            _logger = logger;
        }



        [Authorize]
        [HttpGet("obtenerFaltas")]
        public async Task<IActionResult> ObtenerFaltas()
        {
            try
            {
                if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                {
                    Console.WriteLine("Usuario no identificado");
                    return Unauthorized("Usuario no identificado");
                }

                int usuarioId = (int)usuarioIdObj;
                Console.WriteLine($"Obteniendo faltas para el usuario {usuarioId}");

                var faltas = await _contexto.Faltas
                    .Include(f => f.Estudiante)
                    .Include(f => f.Materia)
                    .Where(f => f.Estudiante.Id_Estudiante == usuarioId)
                    .ToListAsync();

                foreach (var falta in faltas)
                {
                    Console.WriteLine($"Falta para el usuario {usuarioId}: ID={falta.IdFalta}, Materia={falta.Materia.Nombre}, Cantidad={falta.Cantidad}");
                }
                return Ok(faltas);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al obtener faltas: {e.Message}");
                return BadRequest(e.Message);
            }
        }



        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var falta = await _contexto.Faltas.FirstOrDefaultAsync(a => a.IdFalta == id);

                if (falta == null) return NotFound();

                return Ok(falta);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }




        [Authorize]
        [HttpPost("AgregarFaltaMateria")]
        public async Task<IActionResult> AgregarFaltaMateria([FromBody] Faltas nuevaFalta)
        {
            
            try
            {
                var usuarioId = HttpContext.Items["usuarioId"]; // Obtiene el ID del usuario desde el token
                nuevaFalta.IdEstudiante = (int)usuarioId;
                Console.WriteLine("el id del estudiante" + usuarioId);

                Console.WriteLine($"Id_Materia: {nuevaFalta.idMateria}");
                // Verifica si la materia existe
                var materia = await _contexto.Materia.FindAsync(nuevaFalta.idMateria);
                if (materia == null)
                {
                    return BadRequest("La materia seleccionada no existe");
                }

                _contexto.Faltas.Add(nuevaFalta);
                await _contexto.SaveChangesAsync();
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaFalta.IdFalta }, nuevaFalta);
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

       

        [Authorize]
        [HttpPut("actualizarCantidadFalta")]
        public async Task<IActionResult> ActualizarCantidadFalta([FromBody] ActualizacionFaltaDTO dto)
        {
            // Verificar que el usuario existe
            if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                return Unauthorized();

            int usuarioId = (int)usuarioIdObj;

            // Buscar la falta específica por SU ID (no por materia)
            var falta = await _contexto.Faltas.FirstOrDefaultAsync(f => f.IdFalta == dto.IdFalta);
            Console.WriteLine("id:" + falta);
            if (falta == null) return NotFound();

            // Verificar que la falta pertenece al estudiante
            if (falta.IdEstudiante != usuarioId)
                return Forbid();

            // Actualizar SOLO la cantidad
            falta.Cantidad = dto.Cantidad;
            await _contexto.SaveChangesAsync();

            return Ok();
        }





        // GET: Faltas/ObtenerPorEstudiante/{idEstudiante}
        [HttpGet("ObtenerPorEstudiante/{idEstudiante}")]
        public async Task<IActionResult> ObtenerPorEstudiante(int idEstudiante)
        {
            try
            {
                var faltas = await _contexto.Faltas
                    .Where(f => f.IdEstudiante == idEstudiante)
                    .Include(f => f.Materia)
                    .ToListAsync();

                if (faltas == null || !faltas.Any()) return NotFound();

                return Ok(faltas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }



        // DELETE: Faltas/Eliminar/{id}
        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var falta = await _contexto.Faltas.FindAsync(id);
                if (falta == null) return NotFound();

                _contexto.Faltas.Remove(falta);
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
