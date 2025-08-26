using API2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ContactosController : ControllerBase
    {
        private readonly DataContext contexto;

        public ContactosController(DataContext context)
        {
            contexto = context;
        }

        // GET: Contactos
        [HttpGet]
        public IActionResult ObtenerPorId()
        {
            var profesores = contexto.Profesores.ToList();
            return Ok(profesores);
        }


        // GET: Contactos/{id}
        [HttpGet("{id}")]
        public IActionResult ObtenerContacto(int id)
        {
            var contacto = contexto.Contactos.FirstOrDefault(p => p.idContacto == id);
            if (contacto == null) return NotFound();
            return Ok(contacto);
        }






        [Authorize]
        [HttpPost("Agregar")]
        public async Task<IActionResult> AgregarContactos([FromBody] ContactosDTO nuevoContactoDto)
        {
            if (nuevoContactoDto == null)
            {
                return BadRequest("Los datos del contacto son inválidos.");
            }

            try
            {
                if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
                {
                    return Unauthorized("Usuario no identificado");
                }

                int usuarioId = (int)usuarioIdObj;

                var nuevoContacto = new Contactos
                {
                    Nombre = nuevoContactoDto.Nombre,
                    Apellido = nuevoContactoDto.Apellido,
                    idEstudiante = usuarioId,
                    Email = nuevoContactoDto.Email,
                    Celular = nuevoContactoDto.Celular
                };

                contexto.Contactos.Add(nuevoContacto);
                await contexto.SaveChangesAsync();
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoContacto.idContacto }, nuevoContacto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }



        //ACTUALIZAR UN CONTACTO
        [Authorize]
        [HttpPut("actualizarContactos")]
        public async Task<IActionResult> ActualizarContacto([FromBody] Contactos c)
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
                var profesorExistente = await contexto.Contactos
                        .FirstOrDefaultAsync(p => p.idContacto == c.idContacto && p.idEstudiante == usuarioId);
                if (profesorExistente == null)
                {
                    return NotFound("contacto no encontrado o no tienes permiso para editarlo");
                }

                // 3. Actualizar todos los campos (asumiendo que el DTO trae todos los datos actualizados)
                profesorExistente.Nombre = c.Nombre ?? profesorExistente.Nombre;
                profesorExistente.Apellido = c.Apellido ?? profesorExistente.Apellido;
                profesorExistente.Email = c.Email ?? profesorExistente.Email;
                profesorExistente.Celular = c.Celular ?? profesorExistente.Celular;

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
                        return NotFound("Elcontacto ya no existe en la base de datos");
                    }

                    return Conflict("El contacto fue modificado por otro usuario. Recarga los datos e intenta nuevamente.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error interno: {e.Message}");
            }
        }



        [Authorize]
        [HttpGet("obtenerContactos")]
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
                Console.WriteLine($"Obteniendo contactos asociados para el usuario {usuarioId}");

                var contactos = await contexto.Contactos
             .Where(a => a.idEstudiante == usuarioId)
             .ToListAsync();

                foreach (var contacto in contactos)
                {
                    Console.WriteLine($"Contacto para el usuario {usuarioId}: ID={contacto.idContacto}, Nombre={contacto.Nombre}, Apellido={contacto.Apellido}, Email={contacto.Email}, Celular={contacto.Celular}");
                }
                return Ok(contactos);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al obtener contactos : {e.Message}");
                return BadRequest(e.Message);
            }
        }



        // eliminar un contacto
        [Authorize]
        [HttpDelete("Eliminar/{id}")]
        public IActionResult EliminarContacto(int id)
        {
            var contacto = contexto.Contactos.Find(id);
            if (contacto == null) return NotFound();

            contexto.Contactos.Remove(contacto);
            contexto.SaveChanges();
            return NoContent();
        }
    }
}
