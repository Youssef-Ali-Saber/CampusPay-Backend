using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Context;

public class SqLiteDbContext(DbContextOptions<SqLiteDbContext> options) : AppDbContext(options);