using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;



namespace API2.Models
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions<DataContext> options) : base(options)
		{

		}
		public DbSet<Estudiante> Estudiante { get; set; }
		public DbSet<Profesores> Profesores { get; set; }
		public DbSet<Materia> Materia { get; set; }
		public DbSet<Horarios> Horarios { get; set; }
		public DbSet<Calificaciones> Calificaciones { get; set; }

		public DbSet<Contactos> Contactos { get; set; }
		public DbSet<Actividades> Actividades { get; set; }
		public DbSet<Apuntes> Apuntes { get; set; }
		public DbSet<Faltas> Faltas { get; set; }
		public DbSet<Estudiantes_horarios> Estudiantes_horarios { get; set; }
		public DbSet<Profesor_materia> Profesor_materia { get; set; }



		//relacion muchos a muchos entre Profesores y Materias
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Profesor_materia>()
				.HasKey(pm => new { pm.idProfesor, pm.idMateria });

			modelBuilder.Entity<Profesor_materia>()
				.HasOne(pm => pm.Profesor)
				.WithMany(p => p.ProfesorMateria)
				.HasForeignKey(pm => pm.idProfesor);

			modelBuilder.Entity<Profesor_materia>()
				.HasOne(pm => pm.Materia)
				.WithMany(m => m.ProfesorMateria)
				.HasForeignKey(pm => pm.idMateria);
		}
	}
}