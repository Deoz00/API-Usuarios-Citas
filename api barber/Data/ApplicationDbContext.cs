using api_barber.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace api_barber.Data
{
    public class ApplicationDbContext: IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Agregar los modelos

        public DbSet<User> usuario { get; set; }
        public DbSet<Cita> Citas { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relacion entre cita y cliente (usuario que solicita la cita)
            builder.Entity<Cita>()
                .HasOne(c => c.Cliente)
                .WithMany(u => u.citasCliente)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Cita>().
                HasOne(c => c.Empleado)
                .WithMany(u => u.citasEmpleado)
                .HasForeignKey(c => c.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);




            builder.Entity<User>()
               .HasMany(u => u.citasEmpleado)
               .WithOne(c => c.Empleado)
               .HasForeignKey(c => c.EmpleadoId);

            builder.Entity<User>()
                .HasMany(u => u.citasCliente)
                .WithOne(c => c.Cliente)
                .HasForeignKey(c => c.ClienteId);

        }



       
    }
}
