using System.Security.Claims;
using ToDoWithAuth.Models;
using ToDoWithAuth.Models.DTOs;

namespace ToDoWithAuth.Repositories
{
    public interface IAuthRepository
    {
        public string _accessToken { get; set; }
        public Roles role { get; set; }
        Task<Tokens> Login(string username, string password);
        Tokens RenewAccessToken(string token);
        Task<bool> Logout(string token);
        Task<bool> LogoutAll(string token);
        List<Claim> ReadClaims(string token);
        bool SetRoleByToken(string token);
        string HashPassword(string password);
        bool validateAccessToken(string token);
        Guid? FindUserId(string username);
        Guid? FindTaskId(string titleId);
    }
}