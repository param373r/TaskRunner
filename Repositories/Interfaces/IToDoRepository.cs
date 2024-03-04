using ToDoWithAuth.Models;
using ToDoWithAuth.Models.DTOs;

namespace ToDoWithAuth.Repositories
{
    public interface IToDoRepository
    {
        // Get All tasks
        Task<List<TaskItem>> GetAllTasks(SearchDto search, int steps, int pageNo, Guid userId);
        // Get task by id
        Task<TaskItem> GetTaskById(string titleId, Guid userId);
        // Create task
        Task<TaskItem> CreateTask(TaskItem task, Guid userId);
        // Delete task by id
        Task<TaskItem> DeleteTask(string titleId, Guid userId);
        // Update task
        Task<TaskItem> UpdateTask(TaskItem task, string titleId, Guid userId);
    }
}