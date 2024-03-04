using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ToDoWithAuth.Data;
using ToDoWithAuth.Models;
using ToDoWithAuth.Models.DTOs;

namespace ToDoWithAuth.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        private readonly IAuthRepository _authRepository;

        public UserRepository(ILogger<UserRepository> logger, ApplicationDbContext context, IAuthRepository authRepository)
        {
            _context = context;
            _logger = logger;
            _authRepository = authRepository;
        }

        public async Task<Tokens> Register(User user)
        {
            if (user.Username == null || user.Email == null || user.Password == null) {
                return null;
            }
            string username = validateUsername(user.Username);
            if (username != null) {
                user.Username = username;
            } else {
                _logger.LogInformation("Repository: Invalid username!");
                return null;
            }
            string password = user.Password;
            string pass = _authRepository.HashPassword(user.Password);
            if (pass != null) {
                user.Password = pass;
            } else {
                _logger.LogInformation("Password policy not met!");
                return null;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Repository: User registered successfully!");
            
            var tokens = await _authRepository.Login(user.Username, password);

            return tokens;
        }

        public async Task<Response> AssignRole(string username, string role) {
            if (_authRepository.role != Roles.Admin) {
                _logger.LogInformation("{x}", _authRepository.role);
                return new Response{
                    Status = "Unauthorized",
                    Message = "Please log in as admin to assign roles!"
                };
            }
                _logger.LogInformation("{x}", _authRepository.role);

            var user = _context.Users.First(u => u.Username == username);
            Enum.TryParse(role, out Roles _role);
            user.Role = _role;
            user.Tokens = new List<string>();
            await _context.SaveChangesAsync();

            return new Response{
                    Status = "Success",
                    Message = "Role was successfully assigned!"
                };
        }

        public async Task<Response> UploadAvatar(IFormFile imageData, string fileType, string username) {
            var user = _context.Users.First(u => u.Username == username);
            string uniqueFileName = null;
            using (var stream = new MemoryStream())
            {
                imageData.CopyTo(stream);
                var image = stream.ToArray();
                if (!validateImage(image, fileType)) {
                    return new Response {
                        Status = "Failure",
                        Message = "Image couldn't be validated!"
                    };
                }
                string avatarsFolder = Path.Combine("./wwwroot/avatars");  
                uniqueFileName = user.Id.ToString().Split("-")[0] + "_avatar";
                string filePath = Path.Combine(avatarsFolder, uniqueFileName);  
                foreach (string file in Directory.GetFiles(avatarsFolder)) {
                    if (Path.GetFileNameWithoutExtension(file) == uniqueFileName) {
                        string ft = "png";
                        if (fileType == "png") ft = "jpg";
                        File.Delete(filePath + "." + ft);
                        _logger.LogInformation("Previous avatar deleted successfully!");
                        break;
                    }
                }
                filePath += "." + fileType.ToLower();
                File.WriteAllBytes(filePath, image);
            }

            // Store avatar in database

            user.Avatar = uniqueFileName + "." + fileType.ToLower();
            await _context.SaveChangesAsync();

            // admin is able to view all the avatar requested... 
            return new Response{
                Status = "Success",
                Message = "Avatar updated successfully!"
            };
        }

        public async Task<User> Delete(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) {
                return null;
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Repository: User deleted successfully");
            return user;
        }

        public string GetUserAvatar(string username) {
            var user = _context.Users.First(u => u.Username == username);
            return user.Avatar;
        }
        public async Task<User> Read(string username)
        {
            _logger.LogInformation("Repository: User is being retrieved!");
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> UpdateDetails(User user, string username)
        {
            var _user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (_user != null) {
                _user.FirstName = user.FirstName;
                _user.LastName = user.LastName;
                if (!isTimezoneValid(user.Timezone)) {
                    return null;
                }
                _user.Timezone = user.Timezone.ToLower();

                await _context.SaveChangesAsync();
                _logger.LogInformation("Repository: User details updated successfully!");
            }
            return _user;
        }

        private bool isTimezoneValid(string timezone)
        {
            using(StreamReader reader = new StreamReader("./wwwroot/timezones.json")) {
                string json = reader.ReadToEnd();
                // Timezones timezones = new Timezones();
                var t = JsonSerializer.Deserialize<Timezones>(json);
                _logger.LogInformation("{x}",t.timezones);
                foreach(var tz in t.timezones) {
                    if (timezone.ToLower() == tz.ToLower()) {
                        return true;
                    }
                }
                return false;
            }
        }

        public async Task<User> ChangeEmail(string username, string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null) {
                _logger.LogInformation("ERROR: Email already found in the database!");
                return null;
            }
            user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null) {
                
                // add email verification by sending otp and shit...
                user.Email = email;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Repository: User email updated successfully!");
            }

            return user;
        }

        public async Task<User> ChangePassword(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null) {
                var pass = _authRepository.HashPassword(password);   
                if (pass == null) {
                    return null;
                }

                user.Password = pass;
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Repository: User password updated successfully!");
            }

            return user;
        }

        public async Task<User> ChangeUsername(string oldUsername, string newUsername)
        {
            if (validateUsername(newUsername) == null) {
                _logger.LogInformation("Repository: Username is not valid!");
                return null;
            }
            if (oldUsername == newUsername) {
                _logger.LogInformation("Repository: old username and new username are same!");
                return null;
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == newUsername);
            if (user != null) {
                _logger.LogInformation("Username already taken pick another one!");
                return null;
            }
            user = await _context.Users.FirstOrDefaultAsync(u => u.Username == oldUsername);
            if (user != null) {
                user.Tokens = new List<string>();
                user.Username = newUsername.ToLower();

                // _logger.LogInformation("{x}", newUsername);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Repository: User username updated successfully!");
            }

            return user;
        }

        public string validateUsername(string username)
        {
            /*
            Pattern matches when:
                - Username 8 - 20 characters long
                - Cannot start or end with either "." / "_"
                - Allowed characters are a-z0-9
                - no "__" / "._" / "_." / ".." allowed inside
            */
            
            username = username.ToLower();
            string pattern = @"^(?=.{8,20}$)(?![_.])(?!.*[_.]{2})[a-z0-9._]+(?<![_.])$";
            Match match = Regex.Match(username, pattern);
            if (match.Success) {
                return username;
            }
            return null;
        }
        
        private bool validateImage(byte[] image, string fileType)
        {
            if (image.Length > 1024 * 1024) {
                _logger.LogInformation("File greater than 1MB");
                return false;
            }
            _logger.LogInformation(image.Length.ToString());

            string[] hexArray = BitConverter.ToString(image).Split('-');
            var fileHeader = String.Join("", hexArray).Substring(0,8);
            
            _logger.LogInformation($"Fileheader {fileHeader}");
           
            if (fileType != "png" && fileType != "jpg") {
                _logger.LogInformation("Only png and jpg files are allowed!");
                return false;
            }
            else if (fileType == "png" && fileHeader != "89504E47") {
                _logger.LogInformation("File headers for this png are not correct!");
                return false;
            } else if (fileType == "jpg" && fileHeader != "FFD8FFE0") {
                _logger.LogInformation("File header for this jpg are not correct!");
                return false;
            }

            // file is jpg or png
            // PNG header: E2 80 B0 50
            // JPG header: C3 BF C3 98
            // Png file headers : 89 50 4E 47
            // jpg file headers : FF D8 FF E0
            return true;
        }

    }
}