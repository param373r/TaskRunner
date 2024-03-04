using ToDoWithAuth.Data;
using ToDoWithAuth.Models;
using Microsoft.EntityFrameworkCore;
using ToDoWithAuth.Models.DTOs;

namespace ToDoWithAuth.Repositories
{
    public class ToDoRepository : IToDoRepository
    {
        private readonly ILogger<ToDoRepository> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IAuthRepository _authRepository;

        public ToDoRepository(ILogger<ToDoRepository> logger, ApplicationDbContext context, IAuthRepository authRepository)
        {
            _logger = logger;
            _context = context;
            _authRepository = authRepository;
        }

        public async Task<List<TaskItem>> GetAllTasks(SearchDto search, int steps, int pageNo, Guid userId)
        {
            // Search/Filter/Page data
            bool? isComplete = search.IsComplete;
            _logger.LogInformation($"{isComplete}");
            bool? isModified = search.IsModified;
            _logger.LogInformation($"{isModified}");
            bool? isDue = search.IsDue;
            _logger.LogInformation($"{isDue}");
            int step = (steps < 1) ? 1 : steps;
            int page = (pageNo < 1) ? 1 : pageNo;
            string searchString = search.SearchString;
            _logger.LogInformation($"{searchString}");

            // Apply searching
            var task = await _context.TaskItems
                .Where(t => t.UserId == userId)
                .Where(t => (searchString == null && t.Title == t.Title) || t.Title.ToLower().Contains(searchString.ToLower()) || t.Description.ToLower().Contains(searchString.ToLower()))
            // Apply filtering
                .Where(t => (isComplete == null && t.Title == t.Title) || t.MarkCompleted == isComplete)
                .Where(t => (isModified == null && t.Title == t.Title) || (isModified == true && t.CreationTime < t.ModifiedTime) || (isModified == false && t.CreationTime == t.ModifiedTime))
                .Where(t => (isDue == null && t.Title == t.Title) || (isDue == true && t.DueDate <= DateTime.UtcNow) || (isDue == false && t.DueDate > DateTime.UtcNow))
            // Apply pagination
                .OrderBy(t => t.Title)
                .Take(step * page)
                .OrderByDescending(c => c.Title)
                .Take(step)
                .OrderBy(t => t.Title)
                .ToListAsync();
            // Apply sorting (frontend)

            _logger.LogInformation("Repository: All Tasks getting retrieved!");
            if (_authRepository.role == Roles.Admin) {
                return await _context.TaskItems.ToListAsync();
            }
            return task;
        }

        private async Task<TaskItem> checkAuthorization(string titleId, Guid userId)
        {
            return await _context.TaskItems.FirstOrDefaultAsync(t => t.TitleId == titleId && t.UserId == userId);
        }

        public async Task<TaskItem> GetTaskById(string titleId, Guid userId)
        {
            var task = await checkAuthorization(titleId, userId);
            if (task == null) {
                _logger.LogInformation("Repository: User is not authorized to view this task!");
            }
            _logger.LogInformation("Repository: Task retrieved!");
            return task;
        }

        public async Task<TaskItem> CreateTask(TaskItem task, Guid userId)
        {
            if (task.Title == null) {
                return null;
                // throw an error.
            }

            var _task = new TaskItem
            {
                TitleId = task.Title.ToLower().Replace(' ', '-'),
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                UserId = userId
            };

            var tempTask = _context.TaskItems.AsNoTracking().Where(t => t.UserId == _task.UserId).FirstOrDefault(t => t.TitleId == _task.TitleId);
            if (tempTask != null) {
                _logger.LogInformation("Title already exists!! {x}", tempTask);
                return null;
            }

            _context.TaskItems.Add(_task);
            // _context.Users.Find(userId).Tasks.Add(_task.Id);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Repository: Task created successfully!");
            return _task;
        }

        public async Task<TaskItem> DeleteTask(string titleId, Guid userId)
        {
            var task = await checkAuthorization(titleId, userId);
            if (task == null) {
                _logger.LogInformation("Repository: User not authorized to delete the task!");
                // throw error
                return null;
            }

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Repository: Task deleted successfully!");
            return task;
        }

        public async Task<TaskItem> UpdateTask(TaskItem task, string titleId, Guid userId)
        {
            var _task = await checkAuthorization(titleId, userId);
            if (_task != null) {
                _task.Title = task.Title;
                _task.TitleId = task.Title.ToLower().Replace(' ', '-');
                _task.Description = task.Description;
                _task.DueDate = task.DueDate;
                _task.MarkCompleted = task.MarkCompleted;
                _task.ModifiedTime = DateTime.UtcNow;
                // Creation time is prevented from changing via user by not updating it.
                await _context.SaveChangesAsync();
            } else {
                _logger.LogInformation("Repository: User is not authorized to update the task!");
            }
            _logger.LogInformation("Repository: Task updated successfully!");
            return _task;
        }

    }
}