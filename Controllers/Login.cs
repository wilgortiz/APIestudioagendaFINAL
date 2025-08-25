using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Microsoft.EntityFrameworkCore;
//using MailKit.Net.Smtp;
//using MimeKit;
using System.Net.Sockets;
using System.Net;

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using API2.Models;

namespace wepApi.Controllers
{
    [Route("[Controller]")]
    // Asegura que este controlador requiere autenticación JWT
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    [ApiController]
    public class LoginController : ControllerBase
    {
        // Contexto de la base de datos
        // Se inyecta el contexto de la base de datos y la configuración
        private readonly DataContext contexto;
        private readonly IConfiguration configuracion;
        private string hashSalt = "";

        private readonly IWebHostEnvironment hostEnvironment;

        // Constructor que recibe el contexto de la base de datos y la configuración y el entorno de ejecución
        // Se utiliza para inyectar dependencias y configurar el controlador
        public LoginController(DataContext context, IConfiguration config, IWebHostEnvironment env)
        {
            contexto = context;
            configuracion = config; //guarda la configuración de la aplicación(appsettings.json:salt,claves, etc.)
            hashSalt = configuracion["Salt"] ?? ""; // Lee el valor de "Salt" del archivo de configuración (appsettings.json) 
            // y lo guarda en la variable hashSalt. Si no existe, usa una cadena vacía.
            hostEnvironment = env; /*Guarda el entorno de hosting (por ejemplo, para saber si estás en desarrollo o producción, 
            o para acceder a rutas de archivos). */
        }


        // Método de inicio de sesión
        // Este método recibe un objeto Login que contiene el email y la contraseña del usuario
        //usamos hash fijo en esta api, lo mejor es usar un hash dinámico por usuario
        //y guardarlo en la base de datos junto con la contraseña hasheada
        //para que cada usuario tenga un hash diferente
        //y así mejorar la seguridad
        // POST: Login/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        public IActionResult Login([FromForm] Login loginView)
        {
            /* Recibe un objeto de inicio de sesión(al modelo login) que contiene el correo electrónico y la contraseña del usuario.
            A continuación, consulta la base de datos para encontrar un usuario con las credenciales correspondientes.
            Si se encuentra un usuario, devuelve un mensaje de éxito; de lo contrario, devuelve un mensaje de error.*/
            //1)hasheo de la contraseña
            // Se utiliza KeyDerivation.Pbkdf2 para hashear la contraseña con un salt
            try
            {
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                  password: loginView.Password,
                  salt: System.Text.Encoding.ASCII.GetBytes(hashSalt),
                  prf: KeyDerivationPrf.HMACSHA1,
                  iterationCount: 10000,
                  numBytesRequested: 256 / 8
                ));
                //2)verificación de usuario
                // Busca el estudiante en la base de datos por email y contraseña hasheada
                var estudiante = contexto.Estudiante.FirstOrDefault(x => x.Email == loginView.Email);
                if (estudiante == null || hashed != estudiante.Clave)
                {
                    return BadRequest("Nombre de usuario o clave incorrecta");
                }
                else
                {
                    //3)generación de token JWT
                    // Si el estudiante existe y la contraseña es correcta, se genera un token JWT
                    string secretKey = configuracion["TokenAuthentication:SecretKey"] ??
                      throw new ArgumentNullException(nameof(secretKey));
                    var securityKey = secretKey != null ? new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)) : null;
                    var credenciales = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    // se Crea una lista de claims que contiene el email del estudiante y su ID
                    // Claims son datos que se pueden incluir en el token JWT para identificar al usuario
                    // Estos claims se pueden usar para identificar al usuario en el sistema
                    // y para autorizar acciones específicas basadas en su identidad
                    // En este caso, se usa el email del estudiante como nombre de usuario y su ID
                    // como un claim adicional para identificar al usuario de manera única
                    // Claims son pares clave-valor que se incluyen en el token JWT
                    var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, estudiante.Email), // El email del estudiante se usa como nombre de usuario
            new Claim("Id", estudiante.Id_Estudiante.ToString()) // Se agrega el ID del estudiante como un claim
          };
                    // Se agrega el ID del estudiante a HttpContext.Items para su uso posterior
                    var token = new JwtSecurityToken(
                      issuer: configuracion["TokenAuthentication:Issuer"],
                      audience: configuracion["TokenAuthentication:Audience"],
                      claims: claims,
                      expires: DateTime.Now.AddMinutes(60),
                      signingCredentials: credenciales
                    );

                    //return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                    // Retorna el token JWT y un mensaje de éxito
                    // El token se envuelve en un objeto anónimo para incluir un mensaje
                    return Ok(new
                    {
                        //aca se devuelve el token JWT como una cadena
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        mensaje = "Login exitoso"
                    });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        // Método para obtener el estudiante autenticado
        [HttpGet("ObtenerEstudiante")]
        public IActionResult ObtenerEstudiante()
        {
            try
            {
                var email = User.Identity.Name; // El email viene del token JWT
                var estudiante = contexto.Estudiante.FirstOrDefault(e => e.Email == email);

                if (estudiante == null)
                {
                    return NotFound(new { mensaje = "Estudiante no encontrado." });
                }

                return Ok(new
                {
                    estudiante.Id_Estudiante,
                    estudiante.Nombre,
                    estudiante.Apellido,
                    estudiante.Email
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            try
            {
                // JWT es stateless, por lo que el servidor no puede invalidar tokens
                // El cliente debe eliminar el token de su almacenamiento local
                return Ok(new
                {
                    mensaje = "Logout exitoso. Elimina el token del almacenamiento local del cliente.",
                    instrucciones = "El cliente debe eliminar el token JWT de su almacenamiento local (localStorage, sessionStorage, etc.)"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }







        //     // Método de registro
        //     [HttpPost("Registro")]
        //     [AllowAnonymous]
        //     public IActionResult Registro([FromBody] RegistroEstudiantes registro)
        //     {
        //         if (!ModelState.IsValid)
        //         {
        //             //return BadRequest(ModelState);
        //             return BadRequest(new { mensaje = "Modelo inválido" });
        //         }

        //         // Verificar si el correo ya existe
        //         if (contexto.Estudiante.Any(x => x.Email == registro.Email))
        //         {
        //             //return BadRequest("El correo electrónico ya está en uso.");
        //             return BadRequest(new { mensaje = "El correo electrónico ya está en uso." });
        //         }

        //         // Hashear la contraseña
        //         string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        //             password: registro.Clave,
        //             salt: System.Text.Encoding.ASCII.GetBytes(hashSalt),
        //             prf: KeyDerivationPrf.HMACSHA1,
        //             iterationCount: 10000,
        //             numBytesRequested: 256 / 8));

        //         //    // Crear nuevo estudiante
        //         //  var nuevoEstudiante = new Estudiante
        //         //{
        //         //  Email = registro.Email,
        //         //Clave = hashedPassword
        //         //};


        //         // Crear nuevo estudiante
        //         var nuevoEstudiante = new Estudiante
        //         {
        //             Nombre = registro.Nombre,
        //             Apellido = registro.Apellido,
        //             Email = registro.Email,
        //             Clave = hashedPassword
        //         };


        //         contexto.Estudiante.Add(nuevoEstudiante);
        //         contexto.SaveChanges();

        //         // return Ok("Estudiante registrado con éxito.");
        //         return Ok(new { mensaje = "Estudiante registrado con éxito." });

        //     }
        // }



        // Método de registro
        //usamos hash fijo en esta api, lo mejor es usar un hash dinámico por usuario
        //y guardarlo en la base de datos junto con la contraseña hasheada
        //para que cada usuario tenga un hash diferente
        //y así mejorar la seguridad
        [HttpPost("Registro")]
        [AllowAnonymous]
        public IActionResult Registro([FromBody] RegistroEstudiantes registro)
        {
            if (!ModelState.IsValid) //verificamos que en el modelo RegistroEstudiantes
            //todos los campos requeridos estén completos y sean válidos
            {
                return BadRequest(new { mensaje = "Modelo inválido" });
            }

            // Verificar si los campos están vacíos
            if (string.IsNullOrEmpty(registro.Nombre) || string.IsNullOrEmpty(registro.Apellido) || string.IsNullOrEmpty(registro.Email) || string.IsNullOrEmpty(registro.Clave))
            {
                return BadRequest(new { mensaje = "Todos los campos son obligatorios" });
            }

            // Verificar si el correo electrónico es válido
            string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(registro.Email, emailRegex))
            {
                return BadRequest(new { mensaje = "El correo electrónico no es válido" });
            }

            // Verificar si la contraseña es válida
            string passwordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(registro.Clave, passwordRegex))
            {
                return BadRequest(new { mensaje = "La contraseña debe tener al menos 8 caracteres, una mayúscula y un número" });
            }

            // Verificar si el correo ya existe
            if (contexto.Estudiante.Any(x => x.Email == registro.Email))
            {
                return BadRequest(new { mensaje = "El correo electrónico ya está en uso." });
            }

            // Hashear la contraseña
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: registro.Clave,
                salt: System.Text.Encoding.ASCII.GetBytes(hashSalt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Crear nuevo estudiante
            var nuevoEstudiante = new Estudiante
            {
                Nombre = registro.Nombre,
                Apellido = registro.Apellido,
                Email = registro.Email,
                Clave = hashedPassword
            };

            contexto.Estudiante.Add(nuevoEstudiante);
            contexto.SaveChanges();

            return Ok(new { mensaje = "Estudiante registrado con éxito." });
        }


        
        // Método para verificar el token JWT, pero no lo estamos usando
        [HttpGet("VerificarToken")]
        public IActionResult VerificarToken()
        {
            try
            {
                var email = User.Identity?.Name;
                var userId = User.FindFirst("Id")?.Value;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new
                    {
                        mensaje = "Token inválido o no proporcionado",
                        estado = "no_autenticado"
                    });
                }

                return Ok(new
                {
                    mensaje = "Token válido",
                    email = email,
                    userId = userId,
                    estado = "autenticado"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }



    
}
