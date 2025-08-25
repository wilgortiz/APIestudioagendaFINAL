using API2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace API2.Controllers
{
    [Route("[controller]")] // Define la ruta base para este controlador
    [ApiController] // Indica que este controlador es un API Controller
                    //habilita varias funciones como utiles automaticamente
                    //como [frombody] y [authorize]
    public class ActividadesController : ControllerBase
    {
        // Contexto de la base de datos
        //en este caso, se inyecta el contexto de la base de datos
        private readonly DataContext contexto;

        // <-- Aquí ocurre la inyección de dependencias
        public ActividadesController(DataContext context)
        {
            contexto = context; // Se asigna la instancia inyectada al campo privado
        }



        [Authorize] // Requiere autenticación, que el usuario esté logueado
        /* [Authorize] solo bloquea a los no autenticados.
        Si necesitas el ID del usuario autenticado dentro del método, debes buscarlo tú mismo en el contexto.
        Esto es necesario si quieres asociar datos al usuario, validar permisos, etc.
        Si solo te interesa que esté autenticado, con [Authorize] alcanza. Si necesitas saber quién es, tienes que buscar el dato 
        asi como lo hacemos en este metodo
        ejemplo:
        [Authorize] = ¿tienes carnet válido?
        Buscar usuarioId = ¿cuál es tu número de socio para darte tus cosas?*/
        [HttpGet("obtenerActividades")] // Ruta personalizada, Actividades/obtenerActividades
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                // Verifica que el usuario esté autenticado
                // Aquí se asume que el usuario está autenticado y su ID se guarda en HttpContext.Items
                //va al program.cs , al middlewware de autenticación y verifica que el usuario esté logueado

                if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                {
                    Console.WriteLine("Usuario no identificado");
                    return Unauthorized("Usuario no identificado");
                }

                int usuarioId = (int)usuarioIdObj;
                Console.WriteLine($"Obteniendo actividades para el usuario {usuarioId}");

                var actividades = await contexto.Actividades
             .Where(a => a.Id_Estudiante == usuarioId)
             .ToListAsync();

                // Log each activity before returning
                foreach (var act in actividades)
                {
                    Console.WriteLine($"Activity for user {usuarioId}: ID={act.idEvento}, tipo{act.Tipo_Evento}, Date={act.Fecha_Evento}, Desc={act.Descripcion ?? "null"}");
                }

                return Ok(actividades);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al obtener actividades: {e.Message}");
                return BadRequest(e.Message);
            }
        }



        // GET: Actividades/{id}
        [HttpGet("{id}")]
        public IActionResult ObtenerActividad(int id)
        {
            var actividad = contexto.Actividades.Include(a => a.Materia).Include(a => a.Estudiante).FirstOrDefault(a => a.idEvento == id);
            if (actividad == null) return NotFound();
            return Ok(actividad);
        }





        [HttpPost("Crear")]
        public async Task<IActionResult> CrearActividad([FromBody] Actividades nuevaActividad)
        {
            try
            {
                var usuarioId = HttpContext.Items["usuarioId"];
                if (usuarioId == null)
                    return Unauthorized("No se pudo identificar al usuario.");

                // Validaciones manuales
                if (string.IsNullOrWhiteSpace(nuevaActividad.Descripcion))
                    return BadRequest("El campo 'Título' es obligatorio.");

                if (string.IsNullOrWhiteSpace(nuevaActividad.Tipo_Evento))
                    return BadRequest("Debe seleccionar un tipo de actividad.");

                // Validar que la fecha incluya hora
                if (nuevaActividad.Fecha_Evento == default || nuevaActividad.Fecha_Evento.TimeOfDay == TimeSpan.Zero)
                {
                    // Si no tiene hora, asignar una por defecto (puedes quitarlo luego)
                    nuevaActividad.Fecha_Evento = nuevaActividad.Fecha_Evento.Date.AddHours(9); // 9 AM por defecto
                }
                // if (nuevaActividad.Fecha_Evento == default)
                //     return BadRequest("Debe proporcionar una fecha válida para la actividad.");

                // Asignar ID del estudiante logueado
                nuevaActividad.Id_Estudiante = (int)usuarioId;

                contexto.Actividades.Add(nuevaActividad);
                await contexto.SaveChangesAsync();

                return CreatedAtAction(nameof(ObtenerActividad), new { id = nuevaActividad.idEvento }, nuevaActividad);
            }
            catch (Exception e)
            {
                return BadRequest($"Error al crear la actividad: {e.Message}");
            }
        }


        [Authorize]
        [HttpPut("actualizarActividad")]
        public async Task<IActionResult> actualizarActividad([FromBody] Actividades actividad)
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
                var apunteExistente = await contexto.Actividades
                    .FirstOrDefaultAsync(a => a.idEvento == actividad.idEvento);

                if (apunteExistente == null)
                {
                    return NotFound("Apunte no encontrado");
                }

                // 3. Verificar que el apunte pertenece al usuario
                if (apunteExistente.Id_Estudiante != usuarioId)
                {
                    return Forbid("No tienes permiso para editar este apunte");
                }

                // 4. Actualizar solo los campos permitidos (evitar sobrescribir campos sensibles)
                apunteExistente.Fecha_Evento = actividad.Fecha_Evento;
                apunteExistente.Descripcion = actividad.Descripcion ?? apunteExistente.Descripcion;
                apunteExistente.Tipo_Evento = actividad.Tipo_Evento ?? apunteExistente.Tipo_Evento;

                // 5. Guardar cambios
                contexto.Actividades.Update(apunteExistente);
                await contexto.SaveChangesAsync();

                return Ok(apunteExistente);
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error interno: {e.Message}");
            }
        }





        //eliminar
        [Authorize]
        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var actividad = await contexto.Actividades.FindAsync(id);
                if (actividad == null) return NotFound();

                contexto.Actividades.Remove(actividad);
                await contexto.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private bool ApunteExists(int id)
        {
            return contexto.Apuntes.Any(e => e.IdApunte == id);
        }

    }
}

