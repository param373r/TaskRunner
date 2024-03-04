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
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserRepository _repo;
        private readonly IAuthRepository _authRepository;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private string _username;

        public UserController(IUserRepository repo, IAuthRepository authRepository, ILogger<UserController> logger, IMapper mapper)
        {
            _repo = repo;
            _authRepository = authRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {

            string authHeader = HttpContext.Request.Headers.Authorization.ToString();
            if (authHeader == null) {
                context.Result = BadRequest(new { error = "Authorization not found"});
                base.OnActionExecuting(context);
                return;
            }

            string token = authHeader.Split(" ")[1];
            
            _logger.LogInformation("{d}",token);

            bool isTokenValid = _authRepository.validateAccessToken(token);
            if (!isTokenValid) {
                context.Result = BadRequest();
                base.OnActionExecuting(context);
            }

            _authRepository._accessToken = token;
            if (!_authRepository.SetRoleByToken(token)) {
                context.Result = BadRequest(new { error = "Invalid token"});
                base.OnActionExecuting(context);
            }
            _username = _authRepository.ReadClaims(token).First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
        }

        [HttpGet]
        [Route("status/")]
        public ActionResult<string> Status() => "User API - Online!";

        // read
        [HttpGet]
        [Route("{username}/")]
        public async Task<ActionResult<AppUser>> GetUser(string username) {
            if (_authRepository.role != Roles.Admin && _username != username) {
                return BadRequest(new {error = "User not authorized!"});
            }
            var user = await _repo.Read(username);
            return _mapper.Map<User, AppUser>(user);
        }

        // update
        [HttpPut]
        [Route("updateUser/")]
        public async Task<ActionResult<AppUser>> UpdateUserDetails([FromBody] User user) {
            var _user = await _repo.UpdateDetails(user, _username);
            if (_user == null) {
                return BadRequest(new { error = "hello"});
            }
            return _mapper.Map<User, AppUser>(_user);

        }

        [HttpPut]
        [Route("updateEmail/")]
        public async Task<ActionResult<AppUser>> UpdateUserEmail([FromForm] string email) {
            var user = await _repo.ChangeEmail(_username, email);
            return _mapper.Map<User, AppUser>(user);

        }

        [HttpPut]
        [Route("updatePassword/")]
        public async Task<ActionResult<Response>> UpdateUserPassword([FromForm] string password) {
            var user = await _repo.ChangePassword(_username, password);
            return new Response {
                Status = "Success",
                Message = "Password changed successfully! Please log in again..."
            };
        }
        
        [HttpPut]
        [Route("updateUsername/")]
        public async Task<ActionResult<Response>> UpdateUsername([FromForm] string username) {
            _logger.LogInformation("New username {u}", username);
            var user = await _repo.ChangeUsername(_username, username);
            return new Response {
                Status = "Success",
                Message = "Username changed successfully! Please log in again..."
            };
        }

        [HttpGet]
        [Route("avatar/{username}")]
        public ActionResult GetUserAvatar(string username)
        {

            if (_username != username) {
                return BadRequest(new { error = "User not authorized!"});
            }
            string avatarName = _repo.GetUserAvatar(username);
            byte[] imageData = System.IO.File.ReadAllBytes($"./wwwroot/avatars/{avatarName}");
            return File(imageData, $"image/{avatarName.Split('.')[1]}");
        }
        [HttpPost]
        [Route("uploadAvatar")]
        public async Task<ActionResult<Response>> AvatarUpload([FromForm] IFormFile imageData, [FromForm] string fileType) {
            return await _repo.UploadAvatar(imageData, fileType, _username);
        }

        [HttpPut]
        [Route("assignRole")]
        public async Task<ActionResult<Response>> AssignRole([FromBody] RolesDto roleObj) {
            return await _repo.AssignRole(roleObj.Username, roleObj.Role);
        }

        // delete
        [HttpDelete]
        [Route("delete/")]
        public async Task<ActionResult<Response>> DeleteUser() {
            var user = await _repo.Delete(_username);

            if (user == null) {
                return BadRequest(new Response {
                    Status = "Failed",
                    Message = "User couldn't be deleted!"
                });
            }
            return new Response {
                Status = "Success",
                Message = "User deleted successfully!"
            };
        }
    }
}