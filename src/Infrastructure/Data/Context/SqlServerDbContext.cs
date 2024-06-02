using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Context;

public class SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : AppDbContext(options);
