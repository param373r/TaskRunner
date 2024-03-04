using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ToDoWithAuth.Models;
using ToDoWithAuth.Models.DTOs;
using ToDoWithAuth.Repositories;

namespace ToDoWithAuth.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/app/")]
    public class ApplicationController : Controller
    {
        private readonly ILogger<ApplicationController> _logger;
        private readonly IToDoRepository _repo;
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;

        private Guid _userId;

        public ApplicationController(ILogger<ApplicationController> logger, IToDoRepository repo, IAuthRepository authRepository, IMapper mapper)
        {
            _logger = logger;
            _repo = repo;
            _authRepository = authRepository;
            _mapper = mapper;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            
            string authHeader = HttpContext.Request.Headers.Authorization.ToString();
            if (authHeader == null) {
                context.Result = BadRequest(new { error = "User not authorized"});
                base.OnActionExecuting(context);
                return;
            }

            string token = authHeader.Split(" ")[1];
            
            _logger.LogInformation("{d}",token);

            bool isTokenValid = _authRepository.validateAccessToken(token);
            if (!isTokenValid) {
                context.Result = BadRequest("Invalid token!");
                base.OnActionExecuting(context);
            }

            var userId = _authRepository.FindUserId(_authRepository.ReadClaims(token).First(claim => claim.Type == ClaimTypes.NameIdentifier).Value);
            if (userId == null) {
                context.Result = BadRequest(new { error = "Invalid token!"});
                base.OnActionExecuting(context);
                return;
            }

            _authRepository._accessToken = token;
            if (!_authRepository.SetRoleByToken(token)) {
                context.Result = BadRequest(new { error = "Invalid token"});
                base.OnActionExecuting(context);
            }

            _userId = (Guid) userId;
        }
        
        [HttpGet]
        [Route("status/")]
        public ActionResult<string> AppStatus() => "App API - Online";

        [HttpGet]
        [Route("allTasks/")]
        public async Task<ActionResult<List<AppTask>>> GetAllTasks([FromBody] SearchDto search, [FromQuery] int step, [FromQuery] int page) {
            
            var tasks = await _repo.GetAllTasks(search, step, page, _userId);
            List<AppTask> _tasks = new();
            foreach( var task in tasks) {
                _tasks.Add(_mapper.Map<TaskItem, AppTask>(task));
            }
            return _tasks;
        }
        
        [HttpGet]
        [Route("task/{titleId}/")]
        public async Task<ActionResult<AppTask>> GetTaskById(string titleId) {
            var task = await _repo.GetTaskById(titleId, _userId);
            return _mapper.Map<TaskItem, AppTask>(task);
        }

        [HttpPost]
        [Route("createTask/")]
        public async Task<ActionResult<Response>> CreateTask([FromBody] TaskItem task) {
            var _task = await _repo.CreateTask(task, _userId);
            if (_task == null) {
                return BadRequest(new { error = "Failed to create task!"});
            }
            return new Response {
                Status = "Success",
                Message = "Task created successfully!"
            };
        }

        [HttpPut]
        [Route("updateTask/{titleId}")]
        public async Task<ActionResult<AppTask>> UpdateTask([FromBody] TaskItem task, string titleId) {
            var _task = await _repo.UpdateTask(task, titleId, _userId);
            return _mapper.Map<TaskItem, AppTask>(_task);
        }

        [HttpDelete]
        [Route("deleteTask/{titleId}/")]
        public async Task<ActionResult<Response>> DeleteTask(string titleId) {
            var task = await _repo.DeleteTask(titleId, _userId);
            
            if (task == null) {
                return BadRequest(new Response {
                    Status = "Failure",
                    Message = "Task could not be deleted!"
                });
            }

            return new Response {
                Status = "Success",
                Message = "Task deleted successfully!"
            };
        }
        
    }
}