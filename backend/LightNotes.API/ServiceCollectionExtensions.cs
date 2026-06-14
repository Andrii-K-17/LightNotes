using LightNotes.Application.MappingProfiles;
using LightNotes.Application.Services.Auth;
using LightNotes.Application.Services.Chat;
using LightNotes.Application.Services.Notes;
using LightNotes.Application.Services.Users;
using LightNotes.Infrastructure.Data;
using LightNotes.Infrastructure.Services.Auth;
using LightNotes.Infrastructure.Services.Chat;
using LightNotes.Infrastructure.Services.Notes;
using LightNotes.Infrastructure.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LightNotes.API;

/// <summary>
/// Extension methods for registering application dependencies in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core application business logic services.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserService, UserService>();

        services.AddAutoMapper(cfg => cfg.AddProfile<NoteProfile>(), typeof(NoteProfile).Assembly);

        return services;
    }

    /// <summary>
    /// Registers infrastructure services, including the database context and CORS policies.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", builder =>
                builder.WithOrigins("http://localhost:3000", "http://localhost:5173")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials());
        });

        return services;
    }

    /// <summary>
    /// Configures JWT-based authentication and authorization services.
    /// </summary>
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/notechathub"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

        services.AddAuthorization();

        return services;
    }
}
