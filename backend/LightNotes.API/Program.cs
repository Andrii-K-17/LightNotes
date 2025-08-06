using LightNotes.API.Hubs;
using LightNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using LightNotes.Application;
using LightNotes.Infrastructure;
using LightNotes.API;

// Точка входу у додаток
var builder = WebApplication.CreateBuilder(args);

// Реєстрація контролерів та базових сервісів
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger(); // Налаштування Swagger
builder.Services.ConfigureAuthentication(builder.Configuration); // JWT автентифікація
builder.Services.AddApplicationServices(); // Базові сервіси застосунку
builder.Services.AddInfrastructureServices(builder.Configuration); // База даних, CORS
builder.Services.AddSignalR();

var app = builder.Build();

// Глобальна обробка необроблених винятків
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>()?.Error;
            logger.LogError(exception, "Unhandled exception occurred");

            context.Response.StatusCode = 500; // Внутрішня помилка сервера
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"An unexpected error occurred.\"}");
        });
    });

    app.UseHttpsRedirection();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LightNotes API v1");
        options.RoutePrefix = string.Empty; // Swagger UI буде доступний за адресою кореня сайту
    });
}

app.UseRouting();

app.UseCors("AllowSpecificOrigin"); // Дозвіл запитів з фронтенду

app.UseAuthentication(); // Аутентифікація через JWT
app.UseAuthorization(); // Перевірка доступу до ресурсів

app.MapHub<NoteChatHub>("/notechathub"); // Маршрут для SignalR чату
app.MapControllers(); // Маршрути контролерів

app.Run();

public partial class Program { }