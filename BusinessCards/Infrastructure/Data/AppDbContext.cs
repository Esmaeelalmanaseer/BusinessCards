using Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace BusinessCards.Infrastructure.Data;


public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BusinessCard> BusinessCards => Set<BusinessCard>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<BusinessCard>();
        e.HasKey(x => x.Id);
        e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        e.Property(x => x.Email).HasMaxLength(200);
        e.Property(x => x.Phone).HasMaxLength(50);
        e.Property(x => x.Address).HasMaxLength(500);
        e.Property(x => x.PhotoBase64).HasColumnType("nvarchar(max)");
        e.Property(x => x.PhotoSizeBytes);


        e.HasIndex(x => x.Email);
        e.HasIndex(x => x.Phone);
    }
}