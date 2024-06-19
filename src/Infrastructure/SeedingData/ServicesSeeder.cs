using Domain.Entities;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SeedingData
{
    public class ServicesSeeder
    {
        private readonly AppDbContext _context;

        public ServicesSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedServicesAsync()
        {
            if (await _context.Services.AnyAsync()) return;

            var services = new List<Service>
            {
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
            };

            await _context.Services.AddRangeAsync(services);
            await _context.SaveChangesAsync();
        }
    }
}
