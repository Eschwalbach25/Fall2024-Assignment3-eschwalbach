using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_eschwalbach.Models;

namespace Fall2024_Assignment3_eschwalbach.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; } = default!;
    public DbSet<Actor> Actors { get; set; } = default!;
    public DbSet<MovieActor> MovieActors { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MovieActor>()
            .HasOne(ma => ma.Movie)
            .WithMany(m => m.MovieActors)
            .HasForeignKey(ma => ma.MovieId);

        modelBuilder.Entity<MovieActor>()
            .HasOne(ma => ma.Actor)
            .WithMany(a => a.MovieActors)
            .HasForeignKey(ma => ma.ActorId);
    }
}