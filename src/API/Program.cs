using System.Text;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.IRepositories;
using GraduationProject.Hubs;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories;
using Infrastructure.Seeding;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using AccountService = Application.Services.Implementations.AccountService;

namespace GraduationProject;

public abstract class Program
{
    public static void Main(string[] args)
    {
        const string ALLOWALL = "AllowAll";

        var builder = WebApplication.CreateBuilder(args);

        //--------------------------------------------------------------------------------

        //builder.Services.AddDbContext<SqlServerDbContext>(options =>
        //                    options.UseSqlServer(builder.Configuration
        //                                .GetConnectionString("SqlServerConnection")));
        builder.Services.AddDbContext<SqLiteDbContext>(options =>
                            options.UseSqlite(builder.Configuration
                                        .GetConnectionString("SqLiteConnection")));

        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<SqLiteDbContext>()
            .AddDefaultTokenProviders();
        builder.Services.AddScoped<FilesUploader>();
        builder.Services.AddScoped<JWTHandler>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IServiceService , ServiceService>();
        builder.Services.AddScoped<IAuthentcationService , AuthentcationService>();
        builder.Services.AddScoped<IUserActivitieService, UserActivitieService>();
        builder.Services.AddScoped<IChatService, ChatService>();
        builder.Services.AddScoped<ISocialRequestService, SocialRequestService>();
        builder.Services.AddScoped<IWalletService, WalletService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IConnectedUsersService,ConnectedUsersService>();
        builder.Services.AddScoped<UsersSeeder>();
        //builder.Services.BuildServiceProvider()
        //    .GetRequiredService<UsersSeeder>()
        //    .SeedUsersAsync().Wait();

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
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1",
            new OpenApiInfo
            {
                Title = "CamPus_Pay.API",
                Version = "v1"
            }
            );
            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        },
                        Scheme = "oauth2",
                        Name = JwtBearerDefaults.AuthenticationScheme,
                        In = ParameterLocation.Header
                    },
                    Array.Empty<string>()
                }
            });
        });
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
}
