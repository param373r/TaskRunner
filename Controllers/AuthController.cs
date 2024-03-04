using Microsoft.AspNetCore.Mvc;
using ToDoWithAuth.Repositories;
using ToDoWithAuth.Models;
using ToDoWithAuth.Models.DTOs;
using AutoMapper;

namespace ToDoWithAuth.Controllers
{
    [ApiController]
    [Route("/auth/")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, ILogger<AuthController> logger, IUserRepository userRepository, IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _userRepository = userRepository;
            _mapper = mapper;
        }
        
        [HttpGet]
        [Route("status")]
        public ActionResult<string> Status() => "Auth API - Online";

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<Tokens>> Register([FromBody] RegisterDto user) {
            var _user = _mapper.Map<RegisterDto, User>(user);
            var tokens = await _userRepository.Register(_user);

            if (tokens == null) {
                return BadRequest();
            }

            return tokens;
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<Tokens>> Login([FromForm] string username, [FromForm] string password) {
            var tokens = await _repo.Login(username,password);

            if (tokens.AccessToken == null || tokens.RefreshToken == null) {
                return BadRequest();
            }
            return tokens;
        }

        [HttpPut]
        [Route("refresh")]
        public ActionResult<Tokens> RenewAccessToken([FromBody] Tokens token) {
            var tokens = _repo.RenewAccessToken(token.RefreshToken);
            if (tokens == null) {
                return BadRequest( new { error = "Refresh token couldn't be validated!"});
            }
            return Ok(tokens); // or just return the ACCESS TOKEN
        }
        
        [HttpGet]
        [Route("logout")]
        public async Task<ActionResult<Response>> Logout([FromHeader] string Authorization) {
            var action = await _repo.Logout(Authorization.Split(" ")[1]);
            Response response;
            if (!action) {
                response = new Response {
                    Status = "Failed",
                    Message = "Can't logout! User not logged in!"
                };
            } else {
                response = new Response {
                    Status = "Success",
                    Message = "Logged out of current session!"
                };
            }
            return response;
        }

        [HttpGet]
        [Route("logoutAll")]
        public async Task<ActionResult<Response>> LogoutAll([FromHeader] string Authorization) {
            var action = await _repo.LogoutAll(Authorization.Split(" ")[1]);
            Response response;
            if (!action) {
                response = new Response {
                    Status = "Failed",
                    Message = "Can't logout from all devices! User not logged in!"
                };
            } else {
                response = new Response {
                    Status = "Success",
                    Message = "Logged out from all sessions!"
                };
            }
            return response;
        }
    }
}