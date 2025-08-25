using API2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API2.Controllers
{
    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class HorariosController : ControllerBase
    {
        private readonly DataContext _contexto;
        private readonly ILogger<FaltasController> _logger;


        public HorariosController(DataContext context, ILogger<FaltasController> logger)
        {
            _contexto = context;
            _logger = logger;
        }



        [Authorize]
        [HttpGet("obtenerHorarios")]
        public async Task<IActionResult> ObtenerHorarios()
        {
            try
            {
                if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                {
                    Console.WriteLine("Usuario no identificado");
                    return Unauthorized("Usuario no identificado");
                }

                int usuarioId = (int)usuarioIdObj;
                Console.WriteLine($"Obteniendo horarios para el usuario {usuarioId}");

                var horarios = await _contexto.Horarios
                    .Include(h => h.Estudiante)
                    .Include(h => h.Materia)
                    .Where(h => h.idEstudiante == usuarioId)  // Corregido para usar idEstudiante en lugar de navegar a Estudiante
                    .ToListAsync();

                // Log detallado de los horarios encontrados
                foreach (var horario in horarios)
                {
                    Console.WriteLine($"Horario encontrado: " +
                        $"ID={horario.idHorario}, " +
                        $"Día={horario.diaSemana}, " +
                        $"HoraInicio={horario.horaInicio}, " +
                        $"HoraFin={horario.horaFin}, " +
                        $"Materia={horario.Materia?.Nombre ?? "N/A"}");
                }

                return Ok(horarios);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al obtener horarios: {e.Message}");
                return StatusCode(500, $"Error interno al obtener horarios: {e.Message}");
            }
        }

        // GET: Horarios/{id}


        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var horario = await _contexto.Horarios.FirstOrDefaultAsync(a => a.idHorario == id);

                if (horario == null) return NotFound();

                return Ok(horario);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        // POST: Horarios
        [Authorize]
        [HttpPost("AgregarHorariosMateria")]
        public async Task<IActionResult> AgregarHorariosMateria([FromBody] Horarios nuevoHorario)
        {
            try
            {
                var usuarioId = HttpContext.Items["usuarioId"]; // Obtiene el ID del usuario desde el token
                nuevoHorario.idEstudiante = (int)usuarioId;
                Console.WriteLine("el id del estudiante" + usuarioId);

                Console.WriteLine($"Id_Materia: {nuevoHorario.idMateria}");
                // Verifica si la materia existe
                var materia = await _contexto.Materia.FindAsync(nuevoHorario.idMateria);
                if (materia == null)
                {
                    return BadRequest("La materia seleccionada no existe");
                }

                _contexto.Horarios.Add(nuevoHorario);
                await _contexto.SaveChangesAsync();
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoHorario.idHorario }, nuevoHorario);
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





        //Actualizar una materia
        [Authorize]
        [HttpPut("actualizarHorario")]
        public async Task<IActionResult> ActualizarHorario([FromBody] Horarios horario)
        {
            Console.WriteLine("entra");
            try
            {
                // 1. Verificar que el usuario está autenticado
                if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                {
                    return Unauthorized("Usuario no identificado");
                }
                int usuarioId = (int)usuarioIdObj;

                // 2. Verificar que el profesor existe y pertenece al usuario
                var horarioExistente = await _contexto.Horarios
                    .FirstOrDefaultAsync(p => p.idHorario == horario.idHorario && p.idEstudiante == usuarioId);
                Console.WriteLine("idHorario recibido: " + horario.idHorario);
                if (horarioExistente == null)
                {
                    return NotFound("Profesor no encontrado o no tienes permiso para editarlo");
                }



                horarioExistente.diaSemana = horario.diaSemana;
                horarioExistente.horaInicio = horario.horaInicio;
                horarioExistente.horaFin = horario.horaFin;
                horarioExistente.idMateria = horario.idMateria;
                // 4. Guardar cambios con manejo de concurrencia
                try
                {
                    await _contexto.SaveChangesAsync();
                    return Ok(horarioExistente);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        return NotFound("horario ya no existe en la base de datos");
                    }

                    return Conflict("horario fue modificado por otro usuario. Recarga los datos e intenta nuevamente.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error interno: {e.Message}");
            }
        }







        // PUT: Horarios/{id}
        [HttpPut("{id}")]
        public IActionResult ActualizarHorarios(int id, [FromBody] Horarios horarioActualizado)
        {
            if (horarioActualizado == null || horarioActualizado.idHorario != id) return BadRequest();

            var horario = _contexto.Horarios.Find(id);
            if (horario == null) return NotFound();

            horario.idMateria = horarioActualizado.idMateria;
            horario.diaSemana = horarioActualizado.diaSemana;
            horario.horaInicio = horarioActualizado.horaInicio;
            horario.horaFin = horarioActualizado.horaFin;

            _contexto.SaveChanges();
            return NoContent();
        }

        //ELIMINAR HORARIO
        [Authorize]
        [HttpDelete("Eliminar/{id}")]
        public IActionResult EliminarHorario(int id)
        {
            var h = _contexto.Horarios.Find(id);
            if (h == null) return NotFound();

            _contexto.Horarios.Remove(h);
            _contexto.SaveChanges();
            return NoContent();
        }
    }
}
