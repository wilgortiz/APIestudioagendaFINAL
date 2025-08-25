using API2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API2.Controllers
{
    [Route("[controller]")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class ProfesoresController : ControllerBase
    {
        private readonly DataContext contexto;

        public ProfesoresController(DataContext context)
        {
            contexto = context;
        }

        // GET: Profesores
        [HttpGet]
        public IActionResult ObtenerPorId()
        {
            var profesores = contexto.Profesores.ToList();
            return Ok(profesores);
        }

        // GET: Profesores/{id}
        [HttpGet("{id}")]
        public IActionResult ObtenerProfesor(int id)
        {
            var profesor = contexto.Profesores.FirstOrDefault(p => p.idProfesor == id);
            if (profesor == null) return NotFound();
            return Ok(profesor);
        }






        [Authorize]
        [HttpPost("Agregar")]
        public async Task<IActionResult> AgregarProfes([FromBody] ProfesoresDTO nuevoProfesorDto)
        {
            if (nuevoProfesorDto == null)
            {
                return BadRequest("Los datos del profesor son inválidos.");
            }

            try
            {
                if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                {
                    return Unauthorized("Usuario no identificado");
                }

                int usuarioId = (int)usuarioIdObj;

                var nuevoProfe = new Profesores
                {
                    Nombre = nuevoProfesorDto.Nombre,
                    idEstudiante = usuarioId,
                    Email = nuevoProfesorDto.Email,
                    Apellido = nuevoProfesorDto.Apellido,
                    Celular = nuevoProfesorDto.Celular
                };

                contexto.Profesores.Add(nuevoProfe);
                await contexto.SaveChangesAsync();
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoProfe.idProfesor }, nuevoProfe);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }









        [Authorize]
        [HttpGet("obtenerProfes")]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                {
                    Console.WriteLine("Usuario no identificado");
                    return Unauthorized("Usuario no identificado");
                }

                int usuarioId = (int)usuarioIdObj;
                Console.WriteLine($"Obteniendo porfes asociados para el usuario {usuarioId}");

                var profes = await contexto.Profesores
             .Where(a => a.idEstudiante == usuarioId)
             .ToListAsync();

                foreach (var profe in profes)
                {
                    Console.WriteLine($"Profe para el usuario {usuarioId}: ID={profe.idProfesor}, Nombre={profe.Nombre}, apellido={profe.Apellido}, email={profe.Email}");
                }
                return Ok(profes);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al obtener profesores: {e.Message}");
                return BadRequest(e.Message);
            }
        }


        [Authorize]
        [HttpPut("actualizarProfe")]
        public async Task<IActionResult> ActualizarProfe([FromBody] ProfesoresDTO profesorDto)
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
                var profesorExistente = await contexto.Profesores
                    .FirstOrDefaultAsync(p => p.idProfesor == profesorDto.idProfesor && p.idEstudiante == usuarioId);

                if (profesorExistente == null)
                {
                    return NotFound("Profesor no encontrado o no tienes permiso para editarlo");
                }

                // 3. Actualizar todos los campos (asumiendo que el DTO trae todos los datos actualizados)
                profesorExistente.Nombre = profesorDto.Nombre ?? profesorExistente.Nombre;
                profesorExistente.Apellido = profesorDto.Apellido ?? profesorExistente.Apellido;
                profesorExistente.Email = profesorDto.Email ?? profesorExistente.Email;
                profesorExistente.Celular = profesorDto.Celular ?? profesorExistente.Celular;

                // 4. Guardar cambios con manejo de concurrencia
                try
                {
                    await contexto.SaveChangesAsync();
                    return Ok(profesorExistente);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        return NotFound("El profesor ya no existe en la base de datos");
                    }

                    return Conflict("El profesor fue modificado por otro usuario. Recarga los datos e intenta nuevamente.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error interno: {e.Message}");
            }
        }


        // DELETE: Profesores/{id}
        [Authorize]
        [HttpDelete("Eliminar/{id}")]
        public IActionResult EliminarProfesor(int id)
        {
            var profesor = contexto.Profesores.Find(id);
            if (profesor == null) return NotFound();

            contexto.Profesores.Remove(profesor);
            contexto.SaveChanges();
            return NoContent();
        }
    }
}
