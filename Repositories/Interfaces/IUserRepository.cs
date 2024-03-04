using ToDoWithAuth.Models;
using ToDoWithAuth.Models.DTOs;

namespace ToDoWithAuth.Repositories
{
    public interface IUserRepository
    {
        // create user
        Task<Tokens> Register(User user);
        Task<Response> AssignRole(string username, string role);
        Task<Response> UploadAvatar(IFormFile imageData, string fileType, string username);
        string GetUserAvatar(string username);
        // read single user profile
        Task<User> Read(string username);
        // update user
        Task<User> UpdateDetails(User user, string username);
        Task<User> ChangeUsername(string oldUsername, string newUsername);
        Task<User> ChangeEmail(string username, string email);
        Task<User> ChangePassword(string username, string password);
        // delete user
        Task<User> Delete(string username);
    }
}