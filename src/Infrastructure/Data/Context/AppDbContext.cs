using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Context;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<User, IdentityRole, string>(options)
{
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<SocialRequest> SocialRequests { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<AppWallet> AppWallets { get; set; }
    public DbSet<Deposition> Deposits { get; set; }
    public DbSet<Transformation> Transformations { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Donation> Donations { get; set; }
    public DbSet<ConnectedUser> ConnectedUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(p => p.SSN)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(p => p.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(p => p.UserName)
            .IsUnique();

    }
}