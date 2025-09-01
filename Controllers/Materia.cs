// using API2.Models;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Linq;
// using System.Threading.Tasks;

// namespace API2.Controllers
// {
//     [Route("[controller]")]
//     [ApiController]
//     public class MateriaController : ControllerBase
//     {
//         private readonly DataContext contexto;

//         public MateriaController(DataContext context)
//         {
//             contexto = context;
//         }




//         [Authorize]
//         [HttpGet("obtenerMaterias")]
//         public async Task<IActionResult> ObtenerTodas()
//         {
//             try
//             {
//                 if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
//                 {
//                     Console.WriteLine("Usuario no identificado");
//                     return Unauthorized("Usuario no identificado");
//                 }

//                 int usuarioId = (int)usuarioIdObj;
//                 Console.WriteLine($"Obteniendo actividades para el usuario {usuarioId}");

//                 var materias = await contexto.Materia
//                     .Where(a => a.IdEstudiante == usuarioId)
//                     .Include(m => m.ProfesorMateria)
//                         .ThenInclude(pm => pm.Profesor)
//                     .ToListAsync();

//                 var result = materias.Select(m => new
//                 {
//                     m.idMateria,
//                     m.Nombre,
//                     m.Periodo,
//                     ProfesorMateria = m.ProfesorMateria.Select(pm => new
//                     {
//                         Profesor = new
//                         {
//                             pm.Profesor.idProfesor,
//                             pm.Profesor.Nombre,
//                             pm.Profesor.Apellido
//                         }
//                     })
//                 });

//                 return Ok(result);
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine($"Error al obtener actividades: {e.Message}");
//                 return BadRequest(e.Message);
//             }
//         }



//         // GET: Materia/Obtener/{id}
//         [HttpGet("Obtener/{id}")]
//         public async Task<IActionResult> ObtenerPorId(int id)
//         {
//             try
//             {
//                 var materia = await contexto.Materia.FindAsync(id);
//                 if (materia == null) return NotFound();

//                 return Ok(materia);
//             }
//             catch (Exception e)
//             {
//                 return BadRequest(e.Message);
//             }
//         }





//         [Authorize]
//         [HttpPost("Agregar")]
//         public async Task<IActionResult> AgregarMaterias([FromBody] MateriaDto nuevaMateriaDto)
//         {
//             if (nuevaMateriaDto == null)
//             {
//                 return BadRequest("Los datos de la materia son inválidos.");
//             }

//             try
//             {
//                 if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
//                 {
//                     return Unauthorized("Usuario no identificado");
//                 }

//                 int usuarioId = (int)usuarioIdObj;

//                 var nuevaMateria = new Materia
//                 {
//                     Nombre = nuevaMateriaDto.Nombre,
//                     Periodo = nuevaMateriaDto.Periodo,
//                     IdEstudiante = usuarioId
//                 };

//                 contexto.Materia.Add(nuevaMateria);
//                 await contexto.SaveChangesAsync();

//                 // Asignar profesores si existen en el DTO
//                 if (nuevaMateriaDto.ProfesoresIds != null && nuevaMateriaDto.ProfesoresIds.Any())
//                 {
//                     foreach (var profesorId in nuevaMateriaDto.ProfesoresIds)
//                     {
//                         // Verificar que el profesor existe y pertenece al usuario
//                         var profesor = await contexto.Profesores
//                             .FirstOrDefaultAsync(p => p.idProfesor == profesorId && p.idEstudiante == usuarioId);

//                         if (profesor != null)
//                         {
//                             contexto.Profesor_materia.Add(new Profesor_materia
//                             {
//                                 idMateria = nuevaMateria.idMateria,
//                                 idProfesor = profesorId
//                             });
//                         }
//                     }
//                     await contexto.SaveChangesAsync();
//                 }

//                 return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaMateria.idMateria }, nuevaMateria);
//             }
//             catch (Exception e)
//             {
//                 return BadRequest(e.Message);
//             }
//         }




//         [Authorize]
//         [HttpPut("actualizarMateria")]
//         public async Task<IActionResult> ActualizarMateria([FromBody] MateriaDto materiaDto)
//         {
//             try
//             {
//                 if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
//                 {
//                     return Unauthorized("Usuario no identificado");
//                 }
//                 int usuarioId = (int)usuarioIdObj;

//                 var materiaExistente = await contexto.Materia
//                     .Include(m => m.ProfesorMateria)
//                     .ThenInclude(pm => pm.Profesor)
//                     .FirstOrDefaultAsync(p => p.idMateria == materiaDto.idMateria && p.IdEstudiante == usuarioId);

//                 if (materiaExistente == null)
//                 {
//                     return NotFound("Materia no encontrada o no tienes permiso para editarla");
//                 }

//                 // Actualizar propiedades básicas
//                 materiaExistente.Nombre = materiaDto.Nombre ?? materiaExistente.Nombre;
//                 materiaExistente.Periodo = materiaDto.Periodo;

//                 // Manejar asignación de profesores
//                 if (materiaDto.ProfesoresIds != null)
//                 {
//                     // Eliminar asignaciones existentes que no están en la nueva lista
//                     var asignacionesAEliminar = materiaExistente.ProfesorMateria
//                         .Where(pm => !materiaDto.ProfesoresIds.Contains(pm.idProfesor))
//                         .ToList();

//                     foreach (var asignacion in asignacionesAEliminar)
//                     {
//                         contexto.Profesor_materia.Remove(asignacion);
//                     }

//                     // Agregar nuevas asignaciones que no existían antes
//                     var profesoresExistentesIds = materiaExistente.ProfesorMateria.Select(pm => pm.idProfesor).ToList();
//                     var nuevosProfesoresIds = materiaDto.ProfesoresIds.Except(profesoresExistentesIds);

//                     foreach (var profesorId in nuevosProfesoresIds)
//                     {
//                         var profesor = await contexto.Profesores
//                             .FirstOrDefaultAsync(p => p.idProfesor == profesorId && p.idEstudiante == usuarioId);

//                         if (profesor != null)
//                         {
//                             contexto.Profesor_materia.Add(new Profesor_materia
//                             {
//                                 idMateria = materiaExistente.idMateria,
//                                 idProfesor = profesorId
//                             });
//                         }
//                     }
//                 }

//                 await contexto.SaveChangesAsync();

//                 // Devolver un DTO en lugar de la entidad completa
//                 var result = new
//                 {
//                     materiaExistente.idMateria,
//                     materiaExistente.Nombre,
//                     materiaExistente.Periodo,
//                     Profesores = materiaExistente.ProfesorMateria?.Select(pm => new
//                     {
//                         pm.Profesor.idProfesor,
//                         pm.Profesor.Nombre,
//                         pm.Profesor.Apellido
//                     }).ToList()
//                 };

//                 return Ok(result);
//             }
//             catch (Exception e)
//             {
//                 return StatusCode(500, $"Error interno: {e.Message}");
//             }
//         }




//         // obtener materia con sus profesores
//         [Authorize]
//         [HttpGet("ObtenerConProfesores/{id}")]
//         public async Task<IActionResult> ObtenerMateriaConProfesores(int id)
//         {
//             try
//             {
//                 if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
//                 {
//                     return Unauthorized("Usuario no identificado");
//                 }
//                 int usuarioId = (int)usuarioIdObj;

//                 var materia = await contexto.Materia
//                     .Include(m => m.ProfesorMateria)
//                     .ThenInclude(pm => pm.Profesor)
//                     .FirstOrDefaultAsync(m => m.idMateria == id && m.IdEstudiante == usuarioId);

//                 if (materia == null) return NotFound();

//                 var result = new
//                 {
//                     materia.idMateria,
//                     materia.Nombre,
//                     materia.Periodo,
//                     Profesores = materia.ProfesorMateria.Select(pm => new
//                     {
//                         pm.Profesor.idProfesor,
//                         pm.Profesor.Nombre,
//                         pm.Profesor.Apellido
//                     }).ToList()
//                 };

//                 return Ok(result);
//             }
//             catch (Exception e)
//             {
//                 return BadRequest(e.Message);
//             }
//         }


//         [Authorize]
//         [HttpDelete("Eliminar/{id}")]
//         public async Task<IActionResult> EliminarMateria(int id)
//         {
//             if (!HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
//             {
//                 Console.WriteLine("Intento de eliminación sin usuario identificado");
//                 return Unauthorized("Usuario no identificado");
//             }

//             int usuarioId = (int)usuarioIdObj;
//             Console.WriteLine($"[ELIMINAR] Usuario {usuarioId} intenta eliminar la materia con id {id}");

//             var materia = await contexto.Materia
//                 .FirstOrDefaultAsync(m => m.idMateria == id && m.IdEstudiante == usuarioId);

//             if (materia == null)
//             {
//                 Console.WriteLine($"[ELIMINAR] Usuario {usuarioId} no tiene acceso a la materia {id} o no existe.");
//                 return NotFound("Materia no encontrada o no tienes permiso para eliminarla");
//             }

//             contexto.Materia.Remove(materia);
//             await contexto.SaveChangesAsync();

//             Console.WriteLine($"[ELIMINAR] Usuario {usuarioId} eliminó exitosamente la materia {id} ({materia.Nombre})");

//             return NoContent();
//         }




//         // // ELIMINAR materias sin verificar usuario
//         // [Authorize]
//         // [HttpDelete("Eliminar/{id}")]
//         // public IActionResult EliminarMateria(int id)
//         // {
//         //     var materia = contexto.Materia.Find(id);
//         //     if (materia == null) return NotFound();

//         //     contexto.Materia.Remove(materia);
//         //     contexto.SaveChanges();
//         //     return NoContent();
//         // }
//     }
// }







//version simplificada verificando el usuario desde el constructor solamente
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
    public class MateriaController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly int usuarioId;

        public MateriaController(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            contexto = context;

            // Verifico el usuario una sola vez en el constructor
            if (!httpContextAccessor.HttpContext.Items.TryGetValue("usuarioId", out var usuarioIdObj))
            {
                throw new UnauthorizedAccessException("Usuario no identificado");
            }

            usuarioId = (int)usuarioIdObj;
        }

        [Authorize]
        [HttpGet("obtenerMaterias")]
        public async Task<IActionResult> ObtenerTodas()
        {
            try
            {
                Console.WriteLine($"Obteniendo actividades para el usuario {usuarioId}");

                var materias = await contexto.Materia
                    .Where(a => a.IdEstudiante == usuarioId)
                    .Include(m => m.ProfesorMateria)
                        .ThenInclude(pm => pm.Profesor)
                    .ToListAsync();

                var result = materias.Select(m => new
                {
                    m.idMateria,
                    m.Nombre,
                    m.Periodo,
                    ProfesorMateria = m.ProfesorMateria.Select(pm => new
                    {
                        Profesor = new
                        {
                            pm.Profesor.idProfesor,
                            pm.Profesor.Nombre,
                            pm.Profesor.Apellido
                        }
                    })
                });

                return Ok(result);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al obtener actividades: {e.Message}");
                return BadRequest(e.Message);
            }
        }



        [Authorize]
        [HttpPost("Agregar")]
        public async Task<IActionResult> AgregarMaterias([FromBody] MateriaDto nuevaMateriaDto)
        {
            if (nuevaMateriaDto == null)
            {
                return BadRequest("Los datos de la materia son inválidos.");
            }

            try
            {
                var nuevaMateria = new Materia
                {
                    Nombre = nuevaMateriaDto.Nombre,
                    Periodo = nuevaMateriaDto.Periodo,
                    IdEstudiante = usuarioId
                };

                contexto.Materia.Add(nuevaMateria);
                await contexto.SaveChangesAsync();

                // Asignar profesores si existen en el DTO
                if (nuevaMateriaDto.ProfesoresIds != null && nuevaMateriaDto.ProfesoresIds.Any())
                {
                    foreach (var profesorId in nuevaMateriaDto.ProfesoresIds)
                    {
                        // Verificar que el profesor existe y pertenece al usuario
                        var profesor = await contexto.Profesores
                            .FirstOrDefaultAsync(p => p.idProfesor == profesorId && p.idEstudiante == usuarioId);

                        if (profesor != null)
                        {
                            contexto.Profesor_materia.Add(new Profesor_materia
                            {
                                idMateria = nuevaMateria.idMateria,
                                idProfesor = profesorId
                            });
                        }
                    }
                    await contexto.SaveChangesAsync();
                }

                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaMateria.idMateria }, nuevaMateria);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // GET: Materia/Obtener/{id}
        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var materia = await contexto.Materia.FindAsync(id);
                if (materia == null) return NotFound();

                return Ok(materia);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPut("actualizarMateria")]
        public async Task<IActionResult> ActualizarMateria([FromBody] MateriaDto materiaDto)
        {
            try
            {
                var materiaExistente = await contexto.Materia
                    .Include(m => m.ProfesorMateria)
                    .ThenInclude(pm => pm.Profesor)
                    .FirstOrDefaultAsync(p => p.idMateria == materiaDto.idMateria && p.IdEstudiante == usuarioId);

                if (materiaExistente == null)
                {
                    return NotFound("Materia no encontrada o no tienes permiso para editarla");
                }

                // Actualizar propiedades básicas
                materiaExistente.Nombre = materiaDto.Nombre ?? materiaExistente.Nombre;
                materiaExistente.Periodo = materiaDto.Periodo;

                // Manejar asignación de profesores
                if (materiaDto.ProfesoresIds != null)
                {
                    // Eliminar asignaciones existentes que no están en la nueva lista
                    var asignacionesAEliminar = materiaExistente.ProfesorMateria
                        .Where(pm => !materiaDto.ProfesoresIds.Contains(pm.idProfesor))
                        .ToList();

                    foreach (var asignacion in asignacionesAEliminar)
                    {
                        contexto.Profesor_materia.Remove(asignacion);
                    }

                    // Agregar nuevas asignaciones que no existian antes
                    var profesoresExistentesIds = materiaExistente.ProfesorMateria.Select(pm => pm.idProfesor).ToList();
                    var nuevosProfesoresIds = materiaDto.ProfesoresIds.Except(profesoresExistentesIds);

                    foreach (var profesorId in nuevosProfesoresIds)
                    {
                        var profesor = await contexto.Profesores
                            .FirstOrDefaultAsync(p => p.idProfesor == profesorId && p.idEstudiante == usuarioId);

                        if (profesor != null)
                        {
                            contexto.Profesor_materia.Add(new Profesor_materia
                            {
                                idMateria = materiaExistente.idMateria,
                                idProfesor = profesorId
                            });
                        }
                    }
                }

                await contexto.SaveChangesAsync();

                // Devolver un DTO en lugar de la entidad completa
                var result = new
                {
                    materiaExistente.idMateria,
                    materiaExistente.Nombre,
                    materiaExistente.Periodo,
                    Profesores = materiaExistente.ProfesorMateria?.Select(pm => new
                    {
                        pm.Profesor.idProfesor,
                        pm.Profesor.Nombre,
                        pm.Profesor.Apellido
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error interno: {e.Message}");
            }
        }

        // obtener materia con sus profesores
        [Authorize]
        [HttpGet("ObtenerConProfesores/{id}")]
        public async Task<IActionResult> ObtenerMateriaConProfesores(int id)
        {
            try
            {
                var materia = await contexto.Materia
                    .Include(m => m.ProfesorMateria)
                    .ThenInclude(pm => pm.Profesor)
                    .FirstOrDefaultAsync(m => m.idMateria == id && m.IdEstudiante == usuarioId);

                if (materia == null) return NotFound();

                var result = new
                {
                    materia.idMateria,
                    materia.Nombre,
                    materia.Periodo,
                    Profesores = materia.ProfesorMateria.Select(pm => new
                    {
                        pm.Profesor.idProfesor,
                        pm.Profesor.Nombre,
                        pm.Profesor.Apellido
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }



        [Authorize]
        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> EliminarMateria(int id)
        {
            Console.WriteLine($"[ELIMINAR] Usuario {usuarioId} intenta eliminar la materia con id {id}");

            var materia = await contexto.Materia.FirstOrDefaultAsync(m => m.idMateria == id && m.IdEstudiante == usuarioId);

            if (materia == null)
            {
                Console.WriteLine($"[ELIMINAR] Usuario {usuarioId} no tiene acceso a la materia {id} o no existe.");
                return NotFound("Materia no encontrada o no tienes permiso para eliminarla");
            }

            contexto.Materia.Remove(materia);
            await contexto.SaveChangesAsync();

            Console.WriteLine($"[ELIMINAR] Usuario {usuarioId} eliminó exitosamente la materia {id} ({materia.Nombre})");

            return NoContent();
        }
    }
}