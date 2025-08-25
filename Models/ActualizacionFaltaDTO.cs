using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API2.Models
{ }

public class ActualizacionFaltaDTO
{

    public int IdEstudiante { get; set; }

    [key]
    public int IdFalta { get; set; }

    public int idMateria { get; set; }

    public int Cantidad { get; set; }
}

internal class keyAttribute : Attribute
{
}