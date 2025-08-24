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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace LightNotes.API;

/// <summary>
/// Клас-розширення для реєстрації залежностей у DI контейнері
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Реєстрація сервісів бізнес-логіки додатку
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserService, UserService>();

        // Підключення AutoMapper з профілем NoteProfile
        services.AddAutoMapper(typeof(NoteProfile).Assembly);

        return services;
    }

    /// <summary>
    /// Реєстрація інфраструктурних сервісів (бд MySQL та політика CORS)
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        // Підключення до MySQL бази даних за допомогою EF Core
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // Налаштування CORS політики для локального доступу з клієнтів
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", builder =>
                builder.WithOrigins("http://localhost:3000", "http://localhost:5173")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials()); // дозволити надсилати куки або токени
        });

        return services;
    }

    /// <summary>
    /// Налаштування JWT-аутентифікації
    /// </summary>
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // Вказуємо, що використовуємо JWT (Bearer) як метод аутентифікації
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true, // Перевірка цифрового підпису токена
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!))
                    };

                    // Конфігурація подій для JWT Bearer аутентифікації
                    // дозволяє SignalR аутентифікації працювати, коли токен передається в URL, а не в заголовку, уникаючи помилки Unauthorized
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

        // Реєстрація базової авторизації
        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Підключення Swagger з підтримкою JWT та XML-коментарів
    /// </summary>
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services, string title = "LightNotes API", string version = "v1")
    {
        services.AddSwaggerGen(options =>
        {
            // Основна інформація про API
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = title,
                Version = version
            });

            // Підключення XML-коментарів (документація генерується з xml коментарів)
            var xmlPath = GetXmlCommentsPath();
            if (xmlPath != null)
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Додаткові налаштування для підтримки JWT у Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Введіть JWT токен",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Повертає шлях до XML-файлу документації, якщо файл існує
    /// </summary>
    private static string? GetXmlCommentsPath()
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        return File.Exists(xmlPath) ? xmlPath : null;
    }
}
