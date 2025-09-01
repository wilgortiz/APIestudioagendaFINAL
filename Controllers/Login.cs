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


        public LoginController(DataContext context, IConfiguration config, IWebHostEnvironment env)
        {
            contexto = context;
            configuracion = config;
            hashSalt = configuracion["Salt"] ?? "";
            hostEnvironment = env;
        }



        // POST: Login/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        public IActionResult Login([FromForm] Login loginView)
        {

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
                    string secretKey = configuracion["TokenAuthentication:SecretKey"] ??
                      throw new ArgumentNullException(nameof(secretKey));
                    var securityKey = secretKey != null ? new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)) : null;
                    var credenciales = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

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




        //cambio de clave
        [HttpPost("CambiarClave")]
        public IActionResult CambiarClave([FromBody] CambioClaveDTO cambioClave)
        {
            try
            {
                var email = User.Identity?.Name; // El email viene del token JWT
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });
                }

                var estudiante = contexto.Estudiante.FirstOrDefault(e => e.Email == email);
                if (estudiante == null)
                {
                    return NotFound(new { mensaje = "Estudiante no encontrado." });
                }

                // Hashear la clave actual enviada por el usuario
                string hashedClaveActual = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: cambioClave.ClaveActual,
                    salt: Encoding.ASCII.GetBytes(hashSalt),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                ));

                // Validar que la clave actual coincida
                if (hashedClaveActual != estudiante.Clave)
                {
                    return BadRequest(new { mensaje = "La clave actual es incorrecta." });
                }

                // Verificar que la nueva clave no sea igual a la actual
                string hashedClaveNueva = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: cambioClave.ClaveNueva,
                    salt: Encoding.ASCII.GetBytes(hashSalt),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                ));

                if (hashedClaveNueva == estudiante.Clave)
                {
                    return BadRequest(new { mensaje = "La nueva clave no puede ser igual a la anterior." });
                }

                // Guardar la nueva clave
                estudiante.Clave = hashedClaveNueva;
                contexto.SaveChanges();

                return Ok(new { mensaje = "Clave actualizada con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }


        //cuando el estudiante se olvido la clave, le doy una random al mail
        [HttpPost("OlvidoClave")]
        [AllowAnonymous]
        public IActionResult OlvidarClave([FromBody] string email)
        {
            try
            {
                var estudiante = contexto.Estudiante.FirstOrDefault(e => e.Email == email);
                if (estudiante == null)
                {
                    return NotFound(new { mensaje = "No existe un usuario con ese correo." });
                }

                // 1) Generar clave aleatoria
                var nuevaClave = GenerarClaveAleatoria();

                // 2) Hashear la clave
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: nuevaClave,
                    salt: Encoding.ASCII.GetBytes(hashSalt),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                ));

                // 3) Guardar en la DB
                estudiante.Clave = hashed;
                contexto.SaveChanges();

                // 4) Enviar por email con Mailtrap
                using (var smtp = new System.Net.Mail.SmtpClient())
                {
                    smtp.Host = configuracion["Mailtrap:Host"];
                    smtp.Port = int.Parse(configuracion["Mailtrap:Port"]);
                    smtp.Credentials = new NetworkCredential(
                        configuracion["Mailtrap:User"],
                        configuracion["Mailtrap:Pass"]);
                    smtp.EnableSsl = true;

                    var mensaje = new System.Net.Mail.MailMessage();
                    mensaje.From = new System.Net.Mail.MailAddress(configuracion["Mailtrap:From"]);
                    mensaje.To.Add(email);
                    mensaje.Subject = "Nueva clave de acceso";
                    mensaje.Body = $"Hola {estudiante.Nombre},\n\nTu nueva clave temporal es: {nuevaClave}\n\nRecuerda cambiarla después de iniciar sesión.";

                    smtp.Send(mensaje);
                }

                return Ok(new { mensaje = "Se envió una nueva clave a tu correo." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }




        private string GenerarClaveAleatoria(int longitud = 10)
        {
            var random = new Random();
            const string letras = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numeros = "0123456789";
            const string todos = letras + numeros;

            string clave;
            do
            {
                clave = new string(Enumerable.Repeat(todos, longitud)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (!clave.Any(char.IsLetter) || !clave.Any(char.IsDigit));

            return clave;
        }

    }


}
