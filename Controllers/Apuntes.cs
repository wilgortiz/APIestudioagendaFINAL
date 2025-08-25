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
    public class ApuntesController : ControllerBase
    {
        private readonly DataContext _contexto;

        public ApuntesController(DataContext context)
        {
            _contexto = context;
        }




        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                {
                    return Unauthorized("Usuario no identificado");
                }

                int usuarioId = (int)usuarioIdObj;

                var apuntes = await _contexto.Apuntes
                    .Where(a => a.IdEstudiante == usuarioId)
                    .Select(a => new
                    {
                        IdApunte = a.IdApunte,
                        IdEstudiante = a.IdEstudiante,
                        IdMateria = a.IdMateria,
                        Titulo = a.Titulo ?? "", // Manejo explícito de nulos
                        Descripcion = a.Descripcion ?? "",
                        FechaCreacion = a.FechaCreacion
                    })
                    .ToListAsync();

                return Ok(apuntes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        //editar un apunte
        // PUT: Apuntes/actualizarApunte
        [Authorize]
        [HttpPut("actualizarApunte")]
        public async Task<IActionResult> actualizarApunte([FromBody] Apuntes apunteActualizado)
        {
            try
            {
                // 1. Verificar que el usuario está autenticado
                if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                {
                    return Unauthorized("Usuario no identificado");
                }
                int usuarioId = (int)usuarioIdObj;

                // 2. Verificar que el apunte existe
                var apunteExistente = await _contexto.Apuntes
                    .FirstOrDefaultAsync(a => a.IdApunte == apunteActualizado.IdApunte);

                if (apunteExistente == null)
                {
                    return NotFound("Apunte no encontrado");
                }

                // 3. Verificar que el apunte pertenece al usuario
                if (apunteExistente.IdEstudiante != usuarioId)
                {
                    return Unauthorized("No tienes permiso para editar este apunte");
                }

                // 4. Actualizar solo los campos permitidos (evitar sobrescribir campos sensibles)
                apunteExistente.Titulo = apunteActualizado.Titulo ?? apunteExistente.Titulo;
                apunteExistente.Descripcion = apunteActualizado.Descripcion ?? apunteExistente.Descripcion;
                apunteExistente.IdMateria = apunteActualizado.IdMateria;

                // 5. Guardar cambios
                _contexto.Entry(apunteExistente).State = EntityState.Modified;
                await _contexto.SaveChangesAsync();

                return Ok(apunteExistente);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApunteExists(apunteActualizado.IdApunte))
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

        // GET: Apuntes/Obtener/{id}
        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var apunte = await _contexto.Apuntes.FirstOrDefaultAsync(a => a.IdApunte == id);

                if (apunte == null) return NotFound();

                return Ok(apunte);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost("Crear")]
        public async Task<IActionResult> Crear([FromBody] Apuntes nuevoApunte)
        {
            try
            {
                var usuarioId = HttpContext.Items["usuarioId"]; // Obtiene el ID del usuario desde el token
                nuevoApunte.IdEstudiante = (int)usuarioId;
                _contexto.Apuntes.Add(nuevoApunte);
                await _contexto.SaveChangesAsync();
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoApunte.IdApunte }, nuevoApunte);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // PUT: Apuntes/Actualizar/{id}
        [HttpPut("Actualizar/{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Apuntes apunteActualizado)
        {
            if (id != apunteActualizado.IdApunte)
            {
                return BadRequest("El ID del apunte no coincide.");
            }

            try
            {
                _contexto.Entry(apunteActualizado).State = EntityState.Modified;
                await _contexto.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApunteExists(id))
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

        // DELETE: Apuntes/Eliminar/{id}
        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var apunte = await _contexto.Apuntes.FindAsync(id);
                if (apunte == null) return NotFound();

                _contexto.Apuntes.Remove(apunte);
                await _contexto.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private bool ApunteExists(int id)
        {
            return _contexto.Apuntes.Any(e => e.IdApunte == id);
        }
    }
}
