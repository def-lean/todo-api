using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt =>
    opt.UseInMemoryDatabase("TodoList"));
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // app.MapScalarApiReference(); // Requiere la referencia a Scalar.AspNetCore en el proyecto.
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/persona/{nombre}", async (HttpContext context) =>
{
    var nombre = context.Request.RouteValues["nombre"]?.ToString();
    var dto = await context.Request.ReadFromJsonAsync<PersonaRequest>();

    if (string.IsNullOrWhiteSpace(nombre) || dto is null)
    {
        return Results.BadRequest("La solicitud es inválida.");
    }

    if (nombre != dto.nombre)
    {
        return Results.BadRequest("El nombre en la ruta no coincide con el nombre en el cuerpo de la solicitud.");
    }

    Console.WriteLine($"Recibido nombre: {nombre}, nombre: {dto.nombre}, edad: {dto.edad}");
    var persona = new Persona(dto.nombre, dto.edad);
    persona.Saludar();
    return Results.Ok(persona);
});

app.Run();

record PersonaRequest(string nombre, int edad);
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
