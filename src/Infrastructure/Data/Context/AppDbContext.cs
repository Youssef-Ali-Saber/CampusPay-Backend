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
    public DbSet<Transformation> Transfers { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Donation> Donations { get; set; }
    public DbSet<ConnectedUser> ConnectedUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().HasIndex(p => p.SSN).IsUnique();
        modelBuilder.Entity<User>().HasIndex(p => p.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(p => p.UserName).IsUnique();
        modelBuilder.Entity<Service>().HasData(
                    new Service
                    {
                        Id = 1,
                        Name = "University expenses",
                        Description = "University expenses",
                        CollegeName = "FCI",
                        SquadYear = 4,
                        Cost = 1000,
                        Type = "expenses"
                    },
                    new Service
                    {
                        Id = 2,
                        Name = "Medical examination",
                        Description = "Medical examination",
                        CollegeName = "FCI",
                        SquadYear = 4,
                        Cost = 200,
                        Type = "expenses"
                    },
                    new Service
                    {
                        Id = 3,
                        Name = "Gym",
                        Description = "Gym",
                        CollegeName = "FCI",
                        SquadYear = 4,
                        Cost = 350,
                        Type = "entertaining"
                    },
                    new Service
                    {
                        Id = 4,
                        Name = "University Library",
                        Description = "University Library",
                        CollegeName = "FCI",
                        SquadYear = 4,
                        Cost = 50,
                        Type = "expenses"
                    },
                    new Service
                    {
                        Id = 5,
                        Name = "University trip",
                        Description = "University trip",
                        CollegeName = "FCI",
                        SquadYear = 4,
                        Cost = 250,
                        Type = "entertaining"
                    },
                    new Service
                    {
                        Id = 6,
                        Name = "College town",
                        Description = "College town",
                        CollegeName = "FCI",
                        SquadYear = 4,
                        Cost = 2500,
                        Type = "expenses"
                    },
                    new Service
                    {
                        Id = 7,
                        Name = "Skills Exam",
                        Description = "Skills Exam",
                        CollegeName = "FCI",
                        SquadYear = 4,
                        Cost = 600,
                        Type = "expenses"
                    }
                    );

        modelBuilder.Entity<IdentityRole>().HasData(
                    new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Student",
                        NormalizedName = "Student".ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Donor",
                        NormalizedName = "Donor".ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new IdentityRole<string>
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Admin",
                        NormalizedName = "Admin".ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new IdentityRole<string>
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Moderator",
                        NormalizedName = "Moderator".ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    }
                    );

    }
}