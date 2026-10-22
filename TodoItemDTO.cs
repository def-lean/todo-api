public class TodoItemDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsCompleted { get; set; }
   

    public TodoItemDTO() { }
    public TodoItemDTO(Todo todoItem) =>
    (Id, Name, IsCompleted) = (todoItem.Id, todoItem.Name, todoItem.IsCompleted);
}