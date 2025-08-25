using System.ComponentModel.DataAnnotations;

namespace API2.Models

{
	public class RegistroEstudiantes
	{
		[Required]
		public string Nombre { get; set; }

		[Required]
		public string Apellido { get; set; }

	//	[Required]
	//	public long Dni { get; set; }

	//	[Required]
	//	[DataType(DataType.PhoneNumber)]
	//	public string Telefono { get; set; }

		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Clave { get; set; }
	}
}
