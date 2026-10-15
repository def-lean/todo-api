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
    app.MapScalarApiReference();
}

var todoitems = app.MapGroup("/todoitems");

todoitems.MapGet("/", async (TodoDb db) =>
    await db.Todos.ToListAsync());

todoitems.MapGet("/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsCompleted).ToListAsync());

todoitems.MapGet("/{id}", async (int id, TodoDb db) => 
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());
//agregar endpoint para crear un nuevo todo
todoitems.MapPost("/", async (Todo todo, TodoDb db) => { 
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/{todo.Id}", todo);
});
//actualizar un elemento existente con PATCH
todoitems.MapPatch("/{id}", async (int id, TodoPatchDto inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    if (inputTodo.Name is not null) todo.Name = inputTodo.Name;
    if (inputTodo.IsCompleted is not null) todo.IsCompleted = inputTodo.IsCompleted.Value;
    if (inputTodo.Secret is not null) todo.Secret = inputTodo.Secret;

    await db.SaveChangesAsync();

    return Results.NoContent();
});
//Actualizar un todo existente
todoitems.MapPut("/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsCompleted = inputTodo.IsCompleted;
    todo.Secret = inputTodo.Secret;

    await db.SaveChangesAsync();

    return Results.NoContent();
});
//Eliminar un todo existente
todoitems.MapDelete("/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});
app.Run();
