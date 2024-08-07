using System.Reflection;
using System.Text;
using Domain.Entities;
using Domain.IUnitOfWork;
using API.Hubs;
using Infrastructure.Data.Context;
using Infrastructure.Data.UnitOfWork;
using Infrastructure.SeedingData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using Application.Services;

namespace API;

public abstract class Program
{
    public static void Main(string[] args)
    {
        const string ALLOWALL = "AllowAll";

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<SqlServerDbContext>(options =>
                            options.UseSqlServer(builder.Configuration
                                        .GetConnectionString("SqlServerConnection")));



        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<SqlServerDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<FilesUploaderService>();
        builder.Services.AddScoped<JWTHandlerService>();
        builder.Services.AddScoped<EmailHandlerService>();
        builder.Services.AddScoped<HttpClient>();
        builder.Services.AddScoped<PaymentHandlerService>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<ChatService>();
        builder.Services.AddScoped<WalletService>();
        

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
            {
                //o.RequireHttpsMetadata = false;
                o.SaveToken = false;
                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration.GetSection("Jwt")["Issuer"],
                    ValidAudience = builder.Configuration.GetSection("Jwt")["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt")["Key"]))
                };
            });


        //--------------------------------------------------------------------------------
        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(ALLOWALL, policyBuilder =>
            {
                policyBuilder.AllowAnyOrigin();
                policyBuilder.AllowAnyMethod();
                policyBuilder.AllowAnyHeader();
            }
            );
        }
       );


        SeedData(builder).Wait();

        var app = builder.Build();
        

        // Configure the HTTP request pipeline.

        app.UseSwagger();
        app.UseSwaggerUI();
        
        app.UseWebSockets();
        
        app.MapHub<ChatHub>("/chatHub");
        
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Images")),
            RequestPath = "/Images"
        }
        );
        
        app.UseCors(ALLOWALL);
        
        app.UseAuthentication();

        app.UseAuthorization();
        
        app.MapControllers();

        StripeConfiguration.ApiKey = app.Configuration.GetSection("Stripe")["SecretKey"];

        app.Run();



    }

    private static async Task SeedData(WebApplicationBuilder builder)
    {
        var userManager = builder.Services.BuildServiceProvider().GetRequiredService<UserManager<User>>();

        var roleManager = builder.Services.BuildServiceProvider().GetRequiredService<RoleManager<IdentityRole>>();

        var context = builder.Services.BuildServiceProvider().GetRequiredService<SqlServerDbContext>();

        var rolesSeeder = new RolesSeeder(roleManager);
        await rolesSeeder.SeedRolesAsync();

        var usersSeeder = new UsersSeeder(userManager);
        await usersSeeder.SeedUsersAsync();

        var servicesSeeder = new ServicesSeeder(context);
        await servicesSeeder.SeedServicesAsync();
    }
}
