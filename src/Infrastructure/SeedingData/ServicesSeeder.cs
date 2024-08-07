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
                    Name = "University expenses",
                    Description = "University expenses",
                    CollegeName = "FCI",
                    SquadYear = 4,
                    Cost = 1000,
                    Type = "expenses"
                },
                new Service
                {
                    Name = "Medical examination",
                    Description = "Medical examination",
                    CollegeName = "FCI",
                    SquadYear = 4,
                    Cost = 200,
                    Type = "expenses"
                },
                new Service
                {
                    Name = "Gym",
                    Description = "Gym",
                    CollegeName = "FCI",
                    SquadYear = 4,
                    Cost = 350,
                    Type = "entertaining"
                },
                new Service
                {
                    Name = "University Library",
                    Description = "University Library",
                    CollegeName = "FCI",
                    SquadYear = 4,
                    Cost = 50,
                    Type = "expenses"
                },
                new Service
                {
                    Name = "University trip",
                    Description = "University trip",
                    CollegeName = "FCI",
                    SquadYear = 4,
                    Cost = 250,
                    Type = "entertaining"
                },
                new Service
                {
                    Name = "College town",
                    Description = "College town",
                    CollegeName = "FCI",
                    SquadYear = 4,
                    Cost = 2500,
                    Type = "expenses"
                },
                new Service
                {
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
