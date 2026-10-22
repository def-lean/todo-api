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

todoitems.MapGet("/", GetAllTodos);
todoitems.MapGet("/complete", GetCompletedTodos);
todoitems.MapGet("/{id}", GetTodo);
todoitems.MapPost("/", CreateTodo); 
todoitems.MapPatch("/{id}", PatchTodo);
todoitems.MapPut("/{id}", UpdateTodo);  
todoitems.MapDelete("/{id}", DeleteTodo);

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
}

static async Task<IResult> GetCompletedTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsCompleted).Select(x => new TodoItemDTO(x)).ToListAsync());
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo))
            : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
{
    var todoItem = new Todo
    {
        Name = todoItemDTO.Name,
        IsCompleted = todoItemDTO.IsCompleted
    };
    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();
    todoItemDTO = new TodoItemDTO(todoItem);
    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}
static async Task<IResult> PatchTodo(int id, TodoPatchDto inputTodo, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return TypedResults.NotFound();

    if (inputTodo.Name is not null) todo.Name = inputTodo.Name;
    if (inputTodo.IsCompleted is not null) todo.IsCompleted = inputTodo.IsCompleted.Value;
    if (inputTodo.Secret is not null) todo.Secret = inputTodo.Secret;

    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}

static async Task<IResult> UpdateTodo(int id, TodoItemDTO todoItemDTO, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDTO.Name;
    todo.IsCompleted = todoItemDTO.IsCompleted;
    
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}


