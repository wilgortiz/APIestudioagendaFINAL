// Importación de namespaces necesarios para la configuración de la API
// (Permiten usar funcionalidades de autenticación, base de datos, JWT, etc.)
using API2.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Creación del builder para la aplicación ASP.NET Core
// (Inicializa la configuración y servicios de la app)
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Configuración de URLs para la API
// (Define en qué direcciones y puertos escucha la API)
builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001", "http://*:5000", "https://*:5001");

// Agregar servicios al contenedor de servicios
// (Habilita el uso de controladores para manejar las rutas de la API)
builder.Services.AddControllers();

// Configuración de la autenticación JWT
// (Prepara la app para validar y aceptar tokens JWT en las peticiones)
string secretKey = configuration["TokenAuthentication:SecretKey"] ?? throw new ArgumentNullException(nameof(secretKey));
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

// Configura autenticación con JWT Bearer
// (Define cómo se validan los tokens y qué hacer en cada evento de autenticación)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Parámetros de validación del token JWT
        // (Valida emisor, audiencia, vigencia y firma del token)
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,      // Valida el emisor del token
            ValidateAudience = true,    // Valida el público objetivo del token
            ValidateLifetime = true,    // Valida la vigencia del token
            ValidateIssuerSigningKey = true, // Valida la firma del token
            ValidIssuer = configuration["TokenAuthentication:Issuer"],
            ValidAudience = configuration["TokenAuthentication:Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero // Elimina el margen de tiempo para la expiración
        };

        // Eventos para manejar el procesamiento del token
        // (Permite personalizar acciones cuando el token es validado, falla o se recibe)
        options.Events = new JwtBearerEvents
        {
            // Cuando el token es validado
            // (Extrae el ID del usuario del token y lo guarda en el contexto de la petición)
            OnTokenValidated = async context =>
            {
                var userId = context.Principal?.FindFirst("Id")?.Value;
                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int id))
                {
                    context.HttpContext.Items["usuarioId"] = id;
                    Console.WriteLine($"ID de usuario extraído del token: {id}");
                }
                else
                {
                    context.Fail("No se pudo obtener el ID del usuario del token");
                    Console.WriteLine("Error: No se pudo obtener el ID del usuario del token");
                }
            },
            // Cuando falla la autenticación
            // (Muestra el error en consola)
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Error de autenticación: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            // Cuando se recibe el mensaje (útil para WebSockets)
            // (Permite recibir el token como query parameter/parametro de consulta)
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"]; // Se obtiene el token de la consulta
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask; 
            }
        };
    });

// Configuración de Swagger para documentación de la API
// (Permite generar y visualizar la documentación interactiva de la API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Definición de seguridad para usar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme  
    {
        Description = "JWT Authorization header using the Bearer scheme", // Descripción de la seguridad
        Name = "Authorization", // Nombre del encabezado de autorización
        In = Microsoft.OpenApi.Models.ParameterLocation.Header, // Ubicación del encabezado
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey // Tipo de seguridad
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement 
    // Se agrega la seguridad a la API para que se pueda usar en Swagger
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme // Se define el esquema de seguridad
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference // Se define el esquema de seguridad
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, // Tipo de referencia
                    Id = "Bearer" // ID del esquema de seguridad
                }
            },
            Array.Empty<string>() // Se define el esquema de seguridad
        }
    });
});

// Configuración de la conexión a la base de datos MySQL usando Entity Framework
// (Permite a la app conectarse y trabajar con la base de datos MySQL)
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql( // Se usa MySql como motor de base de datos
        configuration["ConnectionStrings:MySql"], // Se obtiene la cadena de conexión de la configuración
        ServerVersion.AutoDetect(configuration["ConnectionStrings:MySql"])) // Se detecta la versión de MySql
);

// Construir la aplicación
// (Crea la instancia final de la app lista para ejecutarse)
var app = builder.Build();

// Configuración del pipeline de HTTP request para el ambiente de desarrollo
// (Si está en desarrollo, habilita Swagger para probar la API desde el navegador)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"); // Se define el endpoint de Swagger
        c.RoutePrefix = "swagger"; // Se define el prefijo de la ruta de Swagger
    });
}

// Middleware para manejar excepciones no capturadas
// (Captura errores globales y devuelve un mensaje genérico al cliente)
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error no manejado: {ex}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Ocurrió un error interno en el servidor");
    }
});

// Configuración de archivos estáticos y CORS
// (Permite servir archivos estáticos y habilita CORS para aceptar peticiones de cualquier origen)
app.UseDefaultFiles();
app.UseStaticFiles();

//Intercambio de recursos entre orígenes (CORS)
//(Permite que la API se comunique con otros servicios web)
app.UseCors(x => x
    .AllowAnyOrigin() // Permite peticiones desde cualquier origen
    .AllowAnyMethod() // Permite cualquier método HTTP
    .AllowAnyHeader()); // Permite cualquier encabezado

// Habilitar autenticación y autorización
// (Activa la validación de tokens y control de acceso en la API)
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
// Esta línea le indica a ASP.NET Core que debe buscar y registrar automáticamente todos los controladores de tu proyecto 
// (los que están en la carpeta Controllers/ y que heredan de ControllerBase o Controller).
app.MapControllers(); // Se mapean los controladores de la API

// Endpoint para verificar el estado de la API
// (Ruta simple para saber si la API está funcionando "run")
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })); // Se define el endpoint para verificar el estado de la API

// Middleware para loggear requests (útil para debug)
// (Muestra en consola cada request y su respuesta)
//Es muy útil para depuración (debug) y monitoreo, ya que puedes ver en tiempo real qué peticiones llegan a tu API y cómo responde.
//Te ayuda a detectar errores, rutas no encontradas, o simplemente a entender el flujo de uso de tu API.
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request recibida: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Response enviada: {context.Response.StatusCode}");
});

// Iniciar la aplicación
// (Arranca el servidor web y comienza a escuchar peticiones)
//Sin esta línea, la aplicación no se pondría en marcha y no podrías acceder a tu API.
app.Run();
